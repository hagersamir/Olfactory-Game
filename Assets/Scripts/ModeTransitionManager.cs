using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;


public class ModeTransitionManager : MonoBehaviour
{
    [Header("Transition Button")]
    public Button transitionButton;

    [Header("Mode 1 Objects to Disable")]
    public List<GameObject> mode1ObjectsToDisable = new List<GameObject>();

    [Header("Mode 1 Colliders to Disable")]
    public List<Collider> mode1CollidersToDisable = new List<Collider>();

    [Header("Mode 2 Objects to Enable")]
    public List<GameObject> mode2ObjectsToEnable = new List<GameObject>();

    [Header("Player Settings")]
    public Transform playerTransform;
    public Vector3 fixedPlayerPosition = new Vector3(-7.01000023f, 0.899999976f, -16.3600006f);

    [Header("Transition Settings")]
    public AudioClip transitionSound;
    public bool hideButtonAfterTransition = true;

    private AudioSource audioSource;
    private XRSimpleInteractable xrInteractable;
    private Vector3 originalPlayerPosition;

    private DatabaseReference dbReference;
    private bool firebaseInitialized = false;
    private FlowerGameManager flowerGameManager;
    private bool applicationQuitting = false;

    async void Start()
    {
        flowerGameManager = FindObjectOfType<FlowerGameManager>();
    
    try
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            firebaseInitialized = true;
            Debug.Log("Firebase initialized successfully");
            
            await SetSessionStarted(true);
            await SetTrainingMode(true);
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Firebase initialization failed: {ex.Message}");
        }        
        
        foreach (GameObject obj in mode2ObjectsToEnable)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (playerTransform != null)
        {
            originalPlayerPosition = playerTransform.position;
        }

        SetupButtonInteractions();
    }

    private async Task SetSessionStarted(bool started)
{
    if (!firebaseInitialized) return;
    
    try
    {
        await dbReference.Child("players/profile/sessionStarted").SetValueAsync(started);
        Debug.Log($"Session started flag set to: {started}");
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Failed to update session state: {ex.Message}");
    }
}

    public async void ResetToTrainingMode()
    {
        foreach (GameObject obj in mode1ObjectsToDisable)
        {
            if (obj != null) obj.SetActive(true);
        }

        foreach (Collider col in mode1CollidersToDisable)
        {
            if (col != null) col.enabled = true;
        }

        foreach (GameObject obj in mode2ObjectsToEnable)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (playerTransform != null)
        {
            playerTransform.position = originalPlayerPosition;
        }

        if (hideButtonAfterTransition)
        {
            transitionButton.gameObject.SetActive(true);
        }

        await SetTrainingMode(true);
    }

    void SetupButtonInteractions()
    {
        xrInteractable = transitionButton.GetComponent<XRSimpleInteractable>();

        if (xrInteractable == null)
        {
            xrInteractable = transitionButton.gameObject.AddComponent<XRSimpleInteractable>();

            if (transitionButton.GetComponent<Collider>() == null)
            {
                BoxCollider collider = transitionButton.gameObject.AddComponent<BoxCollider>();
                RectTransform rt = transitionButton.GetComponent<RectTransform>();
                collider.size = new Vector3(rt.rect.width, rt.rect.height, 10f);
                collider.center = new Vector3(0, 0, 5f);
            }
        }

        xrInteractable.selectEntered.AddListener(_ => OnTransitionButtonClicked());
        transitionButton.onClick.AddListener(OnTransitionButtonClicked);
    }

    public async void OnTransitionButtonClicked()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("audio is null found it ..");
            audioSource = GetComponent<AudioSource>();
        }
        
        if (transitionSound != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        foreach (GameObject obj in mode1ObjectsToDisable)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (Collider col in mode1CollidersToDisable)
        {
            if (col != null) col.enabled = false;
        }

        foreach (GameObject obj in mode2ObjectsToEnable)
        {
            if (obj != null) obj.SetActive(true);
        }

        if (playerTransform != null)
        {
            playerTransform.position = fixedPlayerPosition;
        }

        if (hideButtonAfterTransition)
        {
            transitionButton.gameObject.SetActive(false);
        }

        await SetTrainingMode(false);
        
        // Start the game after transition
        if (flowerGameManager != null)
        {
            flowerGameManager.StartGameAfterTransition();
        }
    }
    
    private async Task SetTrainingMode(bool isTraining)
    {
        if (dbReference != null && firebaseInitialized)
        {
            await dbReference.Child("players").Child("profile").Child("isTrainingMode").SetValueAsync(isTraining);
            Debug.Log($"Successfully set isTrainingMode = {isTraining}");
        }
    }

    private async void OnDestroy()
{
    if (!applicationQuitting) return;
    
    if (firebaseInitialized)
    {
        try
        {
            await dbReference.Child("players/profile/sessionStarted").SetValueAsync(false);
            Debug.Log("Session ended flag set in Firebase");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to update session end: {ex.Message}");
        }
    }
}
}