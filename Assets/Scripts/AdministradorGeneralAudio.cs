using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Script centralizado para gestionar la activación del menú (tecla 'M'), 
/// la carga de audio de múltiples instrumentos y la reproducción sincronizada.
/// </summary>
public class AdministradorGeneralAudio : MonoBehaviour
{
    // Clase interna para agrupar la información de cada instrumento.
    [System.Serializable]
    public class Instrumento
    {
        [Tooltip("Nombre exacto de la subcarpeta en 'Resources/Audio/' (Ej: Guitarra, Bajo).")]
        public string nombreCarpeta; 
        public AudioSource audioSource;
        public TMP_Dropdown dropdown;
        [Tooltip("Objeto 3D (Prop) del instrumento en la escena. Solo visible cuando hay audio.")]
        public GameObject propObjeto3D; 
        
        [HideInInspector] public AudioClip[] clips; // Clips cargados automáticamente desde Resources
    }

    // LISTA PRINCIPAL: Asignar en el Inspector los 4 instrumentos y sus Dropdowns/AudioSources
    [Header("Configuración de Instrumentos")]
    [Tooltip("Configura cada instrumento: nombre de carpeta, AudioSource, Dropdown y Prop 3D.")]
    public List<Instrumento> instrumentos = new List<Instrumento>();

    // UI y Control
    [Header("Configuración General")]
    [Tooltip("Panel UI principal que se activa con 'M'.")]
    public GameObject menuPanel;
    
    private bool isMenuOpen = false;

    // Referencias al jugador para la lógica de pausa
    private PlayerMovement playerMovement;
    // La referencia a 'Selected' se ignora por solicitud.

    void Start()
    {
        // 1. Ocultar menú y buscar scripts del jugador
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        playerMovement = FindObjectOfType<PlayerMovement>();

        // 2. Cargar audio, configurar Dropdowns e inicializar
        foreach (var inst in instrumentos)
        {
            CargarAudioEInicializarDropdown(inst);
            
            // 3. Configurar AudioSource para loop continuo
            if (inst.audioSource != null)
            {
                inst.audioSource.loop = true;
                inst.audioSource.Stop(); 
            }
            
            // 4. NUEVO: Oculta el Prop 3D al iniciar el juego
            if (inst.propObjeto3D != null)
            {
                inst.propObjeto3D.SetActive(false);
            }
        }
        
        if (playerMovement == null)
        {
            Debug.LogWarning("AdministradorGeneralAudio no encontró el script 'PlayerMovement'. La pausa/reanudación del movimiento podría no funcionar correctamente.");
        }
    }

    void Update()
    {
        // 1. Toglear menú con la tecla 'M'
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isMenuOpen)
            {
                CerrarMenu();
            }
            else
            {
                ActivarMenu();
            }
        }
    }

    /// <summary>
    /// Carga los clips de audio desde Resources y llena el Dropdown del instrumento.
    /// Añade la opción "Ninguno" (None) al inicio.
    /// </summary>
    void CargarAudioEInicializarDropdown(Instrumento inst)
    {
        // ... (El contenido de esta función no cambia) ...
        // Ruta: "Audio/<NombreCarpeta>" (Ej: "Audio/Guitarra")
        string ruta = "Audio/" + inst.nombreCarpeta;
        inst.clips = Resources.LoadAll<AudioClip>(ruta);
        
        if (inst.clips.Length == 0)
        {
            Debug.LogError($"[SETUP ERROR] No se encontraron clips de audio en la carpeta Resources/{ruta}.");
            if (inst.dropdown != null) inst.dropdown.ClearOptions();
            return;
        }

        if (inst.dropdown != null)
        {
            inst.dropdown.ClearOptions();
            
            List<string> nombresDeClips = new List<string>();
            // AÑADIDO: Opción "Ninguno" al inicio (será el índice 0)
            nombresDeClips.Add("- Ninguno -"); 
            
            foreach (AudioClip clip in inst.clips)
            {
                // Usamos el nombre del archivo para la opción del menú
                nombresDeClips.Add(clip.name); 
            }
            
            inst.dropdown.AddOptions(nombresDeClips);
            
            // Selecciona "Ninguno" por defecto (Índice 0)
            if (nombresDeClips.Count > 0)
            {
                inst.dropdown.value = 0;
            }
        }
        else
        {
            Debug.LogError($"[SETUP ERROR] El Dropdown para {inst.nombreCarpeta} no está asignado.");
        }
    }

    // ******************************************************
    // Lógica de Menú
    // ******************************************************

    /// <summary>
    /// Activa el menú, pausa el juego y libera el cursor.
    /// Llamada por la tecla 'M'.
    /// </summary>
    public void ActivarMenu()
    {
        if (menuPanel != null)
        {
            isMenuOpen = true;
            Time.timeScale = 0f; // Pausa el tiempo
            
            if (playerMovement != null)
            {
                playerMovement.SetPlayerMovementState(false); 
                playerMovement.SetCameraRotationState(false); 
                playerMovement.UnlockCursor(); // Desbloquea el cursor
            }
            
            menuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("¡El 'menuPanel' no está asignado en el AdministradorGeneralAudio!");
        }
    }

    /// <summary>
    /// Cierra el menú, reanuda el juego y bloquea el cursor.
    /// Llamada por la tecla 'M' o por el botón "Cerrar" de la UI.
    /// </summary>
    public void CerrarMenu()
    {
        if (menuPanel != null)
        {
            isMenuOpen = false;
            menuPanel.SetActive(false);
            
            Time.timeScale = 1f; // Reanuda el tiempo
            
            if (playerMovement != null)
            {
                playerMovement.SetPlayerMovementState(true); 
                playerMovement.SetCameraRotationState(true);  
                playerMovement.LockCursor(); // Bloquea el cursor
            }
        }
    }
    
    // ******************************************************
    // Lógica de Reproducción
    // ******************************************************

    /// <summary>
    /// Detiene todas las pistas y luego reproduce la pista seleccionada en cada Dropdown.
    /// Llamada por el botón "Reproducir Todo".
    /// </summary>
    public void ReproducirTodo()
    {
        // Detenemos cualquier pista anterior primero para un inicio limpio.
        DetenerSoloAudio(); // Usamos la nueva función para no afectar la visibilidad

        foreach (var inst in instrumentos)
        {
            // Verificación mínima de setup
            if (inst.audioSource == null || inst.dropdown == null)
            {
                Debug.LogWarning($"El instrumento '{inst.nombreCarpeta}' no está correctamente configurado (AudioSource/Dropdown). Saltando reproducción.");
                continue;
            }

            // El índice 0 es "Ninguno", por lo tanto, no se reproduce nada.
            int clipIndex = inst.dropdown.value;
            
            if (clipIndex > 0) // Si el índice es mayor que 0 (una pista real)
            {
                // El índice del clip en el array interno es clipIndex - 1 (por la opción "Ninguno" al inicio)
                int realClipIndex = clipIndex - 1;
                
                if (realClipIndex >= 0 && realClipIndex < inst.clips.Length)
                {
                    AudioClip clipASeleccionar = inst.clips[realClipIndex];
                    
                    // 1. Asigna y reproduce
                    inst.audioSource.clip = clipASeleccionar;
                    inst.audioSource.Play();
                    
                    // 2. MUESTRA el Prop 3D solo si hay un audio seleccionado
                    if (inst.propObjeto3D != null)
                    {
                        inst.propObjeto3D.SetActive(true);
                    }
                    
                    Debug.Log($"Reproduciendo {inst.nombreCarpeta}: {clipASeleccionar.name}");
                }
                else
                {
                    Debug.LogError($"Error: Índice de clip fuera de rango para {inst.nombreCarpeta}.");
                }
            }
            else // clipIndex == 0 (Opción "Ninguno")
            {
                 // 1. Detiene el AudioSource.
                inst.audioSource.Stop();
                
                // 2. OCULTA el Prop 3D solo si se selecciona "Ninguno"
                if (inst.propObjeto3D != null)
                {
                    inst.propObjeto3D.SetActive(false);
                }
                
                Debug.Log($"{inst.nombreCarpeta} configurado en Ninguno. Detenido.");
            }
        }
    }
    
    /// <summary>
    /// Detiene la reproducción de todos los AudioSources SIN afectar la visibilidad de los Props.
    /// Llamada internamente por ReproducirTodo().
    /// </summary>
    private void DetenerSoloAudio()
    {
        foreach (var inst in instrumentos)
        {
            if (inst.audioSource != null)
            {
                inst.audioSource.Stop();
            }
        }
    }
    
    /// <summary>
    /// Detiene la reproducción de todos los AudioSources.
    /// Llamada por el botón "Detener Todo".
    /// NOTA: El Prop 3D se mantiene visible/oculto según su estado anterior.
    /// </summary>
    public void DetenerTodo()
    {
        foreach (var inst in instrumentos)
        {
            if (inst.audioSource != null)
            {
                inst.audioSource.Stop();
            }
            // MODIFICADO: Se elimina la línea para ocultar el Prop 3D.
        }
        Debug.Log("Todas las pistas detenidas. Los Props 3D permanecen visibles/ocultos según la última configuración del Dropdown.");
    }
}