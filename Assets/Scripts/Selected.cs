using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selected : MonoBehaviour
{
    public float distancia = 1.5f;
    LayerMask mask;
    public Texture2D puntero;
    public GameObject TextureDetect;
    GameObject ultimoReconocido = null;
    
    // Bandera para permitir o bloquear la detección (la clave del fix)
    private bool canDetect = true; 

    void Start()
    {
        mask = LayerMask.GetMask("Raycast Detect");
        TextureDetect.SetActive(false);
    }

    void Update()
    {
        // Si la detección está deshabilitada (porque el menú está abierto), salimos.
        if (!canDetect)
        {
            Deselect(); // Forzamos la desactivación del mensaje
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distancia, mask))
        {
            SelectedObject(hit.transform);
            if (hit.collider.tag == "Instrumento Musical")
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.GetComponent<Interactuar>().ActivarObjeto();
                }
            }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * distancia, Color.red);
        }
        else
        {
            Deselect();
        }
    }

    void SelectedObject(Transform transform)
    {
        if (ultimoReconocido != transform.gameObject)
        {

            ultimoReconocido = transform.gameObject;
        }
    }

    void Deselect()
    {
        if (ultimoReconocido)
        {

            ultimoReconocido = null;
        }
    }
    
    // Método público llamado por Interactuar.cs para detener la detección
    public void SetDetectionState(bool state)
    {
        canDetect = state;
        if (!state)
        {
            // Aseguramos que el indicador se oculte inmediatamente
            TextureDetect.SetActive(false);
        }
    }

    void OnGUI()
    {
        Rect rect = new Rect(Screen.width / 2, Screen.height / 2, puntero.width, puntero.height);
        if (ultimoReconocido)
        {
            TextureDetect.SetActive(true);
        }
        else
        {
            TextureDetect.SetActive(false);
        }
    }
}
