using UnityEngine;
using System.Collections.Generic; 
// Se elimina using UnityEngine.UI y using TMPro

public class Interactuar : MonoBehaviour
{
    // ASIGNACIÓN EN INSPECTOR: Arrastra aquí el objeto del Canvas (ej: "CanvasMenuGuitarra")
    [Header("Configuración de Menú")]
    public GameObject menuPanel;
    
    // Referencias a los scripts del jugador (se encuentran automáticamente)
    private PlayerMovement playerMovement;
    private Selected selected; 
    
    void Start()
    {
        // 1. Asegúrate de que el menú esté oculto al inicio
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        
        // 2. Busca los scripts del jugador
        playerMovement = FindObjectOfType<PlayerMovement>();
        selected = FindObjectOfType<Selected>(); 
        
        // 3. Verificación de dependencias (PlayerMovement, Selected)
        bool dependenciesMissing = false;
        if (playerMovement == null)
        {
            Debug.LogError("Interactuar.cs no encontró el script 'PlayerMovement'.");
            dependenciesMissing = true;
        }
        if (selected == null) 
        {
            Debug.LogError("Interactuar.cs no encontró el script 'Selected'.");
            dependenciesMissing = true;
        }
        if (dependenciesMissing)
        {
            enabled = false;
            Debug.LogError("ERROR CRÍTICO: Interactuar.cs deshabilitado debido a dependencias faltantes.");
        }
    }
    
    // Esta función es llamada desde el script 'Selected' cuando presionas 'E'
    public void ActivarObjeto()
    {
        if (menuPanel != null)
        {
            Time.timeScale = 0f; // Detiene el juego
            
            // Deshabilita el movimiento del jugador, rotación de cámara y libera el cursor
            if (playerMovement != null)
            {
                playerMovement.SetPlayerMovementState(false); 
                playerMovement.SetCameraRotationState(false); 
                playerMovement.UnlockCursor();                
            }
            
            // Deshabilita la detección por raycast
            if (selected != null)
            {
                selected.SetDetectionState(false); 
            }

            menuPanel.SetActive(true);
        }
        else
        {
            // Mensaje de error simplificado
            Debug.LogError("¡El 'menuPanel' no está asignado en el script Interactuar!");
        }
    }
    
    /// <summary>
    /// Función para cerrar el menú y REANUDAR el control del jugador.
    /// Esta debe estar asignada al botón 'X' de tu UI. Es la única función de salida.
    /// </summary>
    public void CerrarMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false); // Oculta el Canvas
            
            Time.timeScale = 1f; // Reanuda el tiempo del juego
            
            // REACTIVA el movimiento, la cámara y BLOQUEA el cursor
            if (playerMovement != null)
            {
                playerMovement.SetPlayerMovementState(true); // Activa movimiento WASD/Salto
                playerMovement.SetCameraRotationState(true);  // Activa mouse look
                playerMovement.LockCursor();                 // Bloquea cursor
            }
            
            // REACTIVA la detección de objetos interactuables
            if (selected != null)
            {
                selected.SetDetectionState(true); 
            }
        }
    }
}