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

    void Start()
    {
        mask = LayerMask.GetMask("Raycast Detect");
        TextureDetect.SetActive(false);
    }

    void Update()
    {
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