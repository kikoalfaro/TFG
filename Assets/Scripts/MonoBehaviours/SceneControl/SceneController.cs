﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// This script exists in the Persistent scene and manages the content
// based scene's loading.  It works on a principle that the
// Persistent scene will be loaded first, then it loads the scenes that
// contain the player and other visual elements when they are needed.
// At the same time it will unload the scenes that are not needed when
// the player leaves them.
public class SceneController : MonoBehaviour
{

    [Header("First Scene")]
    public string startingSceneName = "Default";


    public event Action BeforeSceneUnload;          // Event delegate that is called just before a scene is unloaded. --> SAVE DATA
    public event Action AfterSceneLoad;             // Event delegate that is called just after a scene is loaded. --> LOAD DATA

    [Space]
    public bool fadeBetweenScenes = false;
    public CanvasGroup faderCanvasGroup;            // The CanvasGroup that controls the Image used for fading to black.
    public float fadeDuration = 1f;                 // How long it should take to fade to and from black.


    private bool isFading;                          // Flag used to determine if the Image is currently fading to or from black.

    private IEnumerator Start()
    {
        string initialStartingPositionName = startingSceneName;
        // OJO, SI DESPUÉS QUIERO TENER VARIOS STARTING POINTS, TENGO QUE VOLVER A PONERLOS PÚBLICOS COMO STRING

        // Set the initial alpha to start off with a black screen.
        faderCanvasGroup.alpha = 1f;

        // Start the first scene loading and wait for it to finish.
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName));

        // Once the scene is finished loading, start fading in.
        StartCoroutine(Fade(0f));
    }


    // This is the main external point of contact and influence from the rest of the project.
    // This will be called by a SceneReaction when the player wants to switch scenes.
    public void FadeAndLoadScene(SceneReaction sceneReaction)
    {
        // If a fade isn't happening then start fading and switching scenes.
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneReaction.sceneName));
        }
    }

    // Sobrecarga para que no haga falta una SceneReaction
    public void FadeAndLoadScene(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName));
        }        
    }


    // This is the coroutine where the 'building blocks' of the script are put together.
    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        // 1) --- EMPIEZA EL FADING A NEGRO ---
        // Start fading to black and wait for it to finish before continuing.
        if(fadeBetweenScenes) yield return StartCoroutine(Fade(1f));

        // 2) --- SE GUARDA LA INFORMACIÓN DE LA ESCENA ACTUAL ---
        // If this event has any subscribers, call it.
        if (BeforeSceneUnload != null)
            BeforeSceneUnload();

        // 3) --- SE "DESCARGA" LA ESCENA ACTUAL Y SE CARGA LA NUEVA ---
        // Unload the current active scene.
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        // Start loading the given scene and wait for it to finish.
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        // 4) --- SE CARGA LA INFORMACIÓN DE LA NUEVA ESCENA RECIÉN CARGADA ---
        // If this event has any subscribers, call it.
        if (AfterSceneLoad != null)
            AfterSceneLoad();

        // 5) --- FADING A ESCENA ---
        // Start fading back in and wait for it to finish before exiting the function.
        if (fadeBetweenScenes) yield return StartCoroutine(Fade(0f));

        // Aquí el último yield significa: "La corrutina no acaba hasta que el fade final acabe"
    }


    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        // Allow the given scene to load over several frames and add it to the already loaded scenes (just the Persistent scene at this point).
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Find the scene that was most recently loaded (the one at the last index of the loaded scenes).
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        // Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next).
        SceneManager.SetActiveScene(newlyLoadedScene);
    }


    private IEnumerator Fade(float finalAlpha) // 1 = opaco, 0 = transparente
    {
        // Set the fading flag to true so the FadeAndSwitchScenes coroutine won't be called again.
        isFading = true;

        // Make sure the CanvasGroup blocks raycasts into the scene so no more input can be accepted.
        faderCanvasGroup.blocksRaycasts = true;

        // Calculate how fast the CanvasGroup should fade based on it's current alpha, it's final alpha and how long it has to change between the two.
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        // While the CanvasGroup hasn't reached the final alpha yet...
        while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            // ... move the alpha towards it's target alpha.
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha,
                fadeSpeed * Time.deltaTime);

            // Wait for a frame then continue.
            yield return null;
        }

        // Set the flag to false since the fade has finished.
        isFading = false;

        // Stop the CanvasGroup from blocking raycasts so input is no longer ignored.
        faderCanvasGroup.blocksRaycasts = false;

    }

    /*
     * NOTAS INTERESANTES SOBRE LAS CORRUTINAS EN UNITYSCRIPT:
     * - NO HACE FALTA DECLARARLAS EXPLÍCITAMENTE. CON QUE TENGA UN RETORNO DEL TIPO "yield" SE SOBREENTIENDE (Seguirá al siguiente frame) --> Retardo por defecto.
     *              ---> Si sse define un objeto WaitForSeconds(x), tardará x segundos en seguir la ejecución habitual.
     * - SE PUEDE LLAMAR COMO SI FUESE UNA FUNCIÓN NORMAL.
     * 
     * for(;;) == while(true)
     * */
}
