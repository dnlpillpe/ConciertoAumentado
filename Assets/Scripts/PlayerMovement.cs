using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float playerMinSpeed = 4f; 
    public float playerMaxSpeed = 7f; 

    [Header("Salto")]
    public float jumpForce = 6f; 
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private Rigidbody rb;
    private bool isGrounded = true;

    [Header("Cámara")]
    public float sensitivity = 2f;
    public float limitX = 90f;
    public Transform cam;

    private float rotationX;

    // Bandera para habilitar/deshabilitar rotación de cámara (mouse look)
    private bool canRotateCamera = true;
    // Bandera para habilitar/deshabilitar movimiento del jugador (WASD y Salto)
    private bool canMovePlayer = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();    
        LockCursor(); // Bloqueamos el cursor al iniciar
    }

    void Update()
    {
        // Solo permitir movimiento si canMovePlayer es verdadero
        if (canMovePlayer) 
        {
            // Movimiento WASD
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? playerMaxSpeed : playerMinSpeed;

            Vector3 move = transform.right * x + transform.forward * y;
            // Mantenemos la velocidad vertical (rb.velocity.y) para la gravedad
            rb.velocity = new Vector3(move.x * currentSpeed, rb.velocity.y, move.z * currentSpeed);

            // Salto
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            // Ajuste de gravedad (multiplicadores para salto más natural)
            if (rb.velocity.y < 0) // cayendo
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space)) // soltó espacio
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
        else
        {
             // Si el jugador no puede moverse, detenemos el movimiento horizontal/frontal
             rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // Rotación cámara (mouse look)
        if (canRotateCamera)
        {
            rotationX += -Input.GetAxis("Mouse Y") * sensitivity;
            rotationX = Mathf.Clamp(rotationX, -limitX, limitX);
            cam.localRotation = Quaternion.Euler(rotationX, 0, 0);

            // Rotación jugador (cuerpo)
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * sensitivity);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // reset vertical
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // --- MÉTODOS PÚBLICOS PARA CONTROL EXTERNO ---

    // Llamado para activar/desactivar el movimiento WASD y Salto
    public void SetPlayerMovementState(bool state)
    {
        canMovePlayer = state;
    }

    // Llamado para activar/desactivar la rotación de la cámara (mouse look)
    public void SetCameraRotationState(bool state)
    {
        canRotateCamera = state;
    }

    // Llamado para bloquear el cursor (Modo juego)
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 
    }

    // Llamado para liberar el cursor (Modo menú)
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 
    }
}