using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using TMPro; 


public class FlowerZoneTracker : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string currentZone = "none";
    private bool firebaseReady = false;
    private bool applicationQuitting = false;


    public ParticleSystem LavFlowerSteam;
    public ParticleSystem RedFlowerSteam;
    public GameObject zoneDisplayPanel; 
    public TextMeshProUGUI zoneDisplayText; 

    async void Start()
    {
        // Initialize Firebase
    var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
    if (dependencyStatus == DependencyStatus.Available)
    {
        firebaseReady = true;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase initialized successfully");
        
        await SetSessionStarted(true);
        await SetZoneToNoneInFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
        }

        // Initialize all visual effects to off
        InitializeVisuals();
    }

    private async Task SetSessionStarted(bool started)
{
    if (!firebaseReady) return;
    
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
//     void OnApplicationQuit()
// {
//     applicationQuitting = true;
// }

private async void OnDestroy()
{
    if (!applicationQuitting) return;
    
    if (firebaseReady)
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

    void InitializeVisuals()
    {
        if (LavFlowerSteam != null)
        {
            LavFlowerSteam.Stop();
            LavFlowerSteam.gameObject.SetActive(false);
        }

        if (RedFlowerSteam != null)
        {
            RedFlowerSteam.Stop();
            RedFlowerSteam.gameObject.SetActive(false);
        }

        if (zoneDisplayPanel != null)
        {
            zoneDisplayPanel.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!firebaseReady) return;

        if (other.CompareTag("LavFlowerZone"))
        {
            HandleZoneEntry("Lavender", LavFlowerSteam, "LAVENDER ZONE");
        }
        else if (other.CompareTag("RedFlowerZone"))
        {
            HandleZoneEntry("Red", RedFlowerSteam, "RED ZONE");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!firebaseReady) return;

        if (other.CompareTag("LavFlowerZone") || other.CompareTag("RedFlowerZone"))
        {
            HandleZoneExit();
        }
    }

    void HandleZoneEntry(string zoneCode, ParticleSystem steamEffect, string displayText)
    {
        if (currentZone != zoneCode)
        {
            currentZone = zoneCode;
            Debug.Log($"Entered {displayText}");

            // Activate the correct steam effect
            if (steamEffect != null)
            {
                steamEffect.gameObject.SetActive(true);
                steamEffect.Play();
            }

            // Deactivate the other steam effect
            if (zoneCode == "Lavender" && RedFlowerSteam != null)
            {
                RedFlowerSteam.Stop();
                RedFlowerSteam.gameObject.SetActive(false);
            }
            else if (zoneCode == "Red" && LavFlowerSteam != null)
            {
                LavFlowerSteam.Stop();
                LavFlowerSteam.gameObject.SetActive(false);
            }

            // Update UI
            UpdateZoneDisplay(displayText, true);
            
            UpdateFirebase();
        }
    }

    void HandleZoneExit()
    {
        if (currentZone != "none")
        {
            currentZone = "none";

            Debug.Log("Exited Flower Zone");
            // Deactivate all steam effects
            if (LavFlowerSteam != null)
            {
                LavFlowerSteam.Stop();
                LavFlowerSteam.gameObject.SetActive(false);
            }

            if (RedFlowerSteam != null)
            {
                RedFlowerSteam.Stop();
                RedFlowerSteam.gameObject.SetActive(false);
            }

            // Hide UI
            UpdateZoneDisplay("", false);
            
            UpdateFirebase();
        }
    }

    void UpdateZoneDisplay(string text, bool show)
    {
        if (zoneDisplayPanel != null)
        {
            zoneDisplayPanel.SetActive(show);
        }

        if (zoneDisplayText != null && show)
        {
            zoneDisplayText.text = $"CURRENT ZONE: {text}";
        }
    }

    async void UpdateFirebase()
    {
        if (!firebaseReady || dbReference == null) return;

        try
        {
            await dbReference.Child("players").Child("profile").Child("player1_train").Child("currentZone").SetValueAsync(currentZone);
            Debug.Log($"Firebase updated: {currentZone}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase update failed: {e.Message}");
        }
    }

    private async Task SetZoneToNoneInFirebase()
    {
        try
        {
            await dbReference.Child("players").Child("profile").Child("player1_train").Child("currentZone").SetValueAsync("none");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set initial zone: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        if (!firebaseReady) return;
        Task.Run(() => SetZoneToNoneInFirebase());
        applicationQuitting = true;
    }
}