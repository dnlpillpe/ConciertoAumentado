using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float playerMinSpeed = 4f;   // 🔹 antes 10f
    public float playerMaxSpeed = 7f;   // 🔹 antes 20f

    [Header("Salto")]
    public float jumpForce = 6f;        // 🔹 un poco más bajo que antes
    public float fallMultiplier = 2.5f; // 🔹 más gravedad al caer
    public float lowJumpMultiplier = 2f;// 🔹 salto más corto si sueltas espacio

    private Rigidbody rb;
    private bool isGrounded = true;

    [Header("Cámara")]
    public float sensitivity = 2f;
    public float limitX = 90f;
    public Transform cam; 

    private float rotationX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();    
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Movimiento WASD
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? playerMaxSpeed : playerMinSpeed;

        Vector3 move = transform.right * x + transform.forward * y;
        rb.velocity = new Vector3(move.x * currentSpeed, rb.velocity.y, move.z * currentSpeed);

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
       {
           Jump();
        }

        // Ajuste de gravedad para que el salto sea más natural
        if (rb.velocity.y < 0) // cayendo
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space)) // soltó espacio
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Rotación cámara
        rotationX += -Input.GetAxis("Mouse Y") * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -limitX, limitX);
        cam.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Rotación jugador
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * sensitivity);
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
}
