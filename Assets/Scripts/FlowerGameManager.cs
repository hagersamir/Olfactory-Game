using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using TMPro;



public class FlowerGameManager : MonoBehaviour
{
    [Header("Flower Objects")]
    public GameObject lavenderFlower;
    public GameObject redFlower;

    [Header("Game Settings")]
    public string patientId = "patient123";

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;

    [Header("Particle Effects")]
    public GameObject correctParticles;
    public GameObject wrongParticles;

    [Header("Game score accuracy")]
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    public GameObject timePanel;
    public TextMeshProUGUI timeText;
    
    [Header("Transition Buttons")]
    public Button trainAgainButton;
    public Button testButton;

    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private int currentRound = 0; 
    private string currentCorrectFlower;
    private bool isRoundActive = false;
    private float roundStartTime;
    private bool flowerGrabbed = false;
    private bool wasCorrectChoice = false;
    private string userId;
    private bool firebaseInitialized = false;
    private bool waitingForSmellReset = false;
    private bool gameStarted = false;
    private bool applicationQuitting = false;

    async void Start()
    {
      scorePanel.SetActive(false);
      applicationQuitting = false;
    
    try
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            throw new Exception($"Firebase dependencies failed: {dependencyStatus}");
        }
        auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null)
        {
            await auth.SignInAnonymouslyAsync();
        }
        userId = auth.CurrentUser.UserId;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        firebaseInitialized = true;
        
        // Set session started to true
        await SetSessionStarted(true);
        
        await dbReference.Child($"players/profile/player1_test").RemoveValueAsync();
        await InitializeDatabaseStructure();
            SetupFlowers();
            lavenderFlower.SetActive(false);
            redFlower.SetActive(false);
            if (correctParticles != null) correctParticles.SetActive(false);
            if (wrongParticles != null) wrongParticles.SetActive(false);
            StartListeningForSmellChanges();
            
            // Initialize with round 0
            await dbReference.Child($"players/profile/player1_test/gameState/currentRound").SetValueAsync(0);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Initialization failed: {ex.Message}");
            SetupOfflineMode();
        }
    }

    async void OnApplicationQuit()
{
    applicationQuitting = true;
    if (firebaseInitialized)
    {
        try
        {
            await dbReference.Child("players/profile/sessionStarted").SetValueAsync(false);
            Debug.Log("Session ended flag set in Firebase");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update session end: {ex.Message}");
        }
    }
}
  private async Task SetSessionStarted(bool started)
{
    if (!firebaseInitialized) return;
    
    try
    {
        await dbReference.Child("players/profile/sessionStarted").SetValueAsync(started);
        Debug.Log($"Session started flag set to: {started}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to update session state: {ex.Message}");
    }
}

    public async void StartGameAfterTransition()
    {
        if (gameStarted) return;
        
        gameStarted = true;
        Debug.Log("Starting game after transition...");
        
        // Wait for 5 seconds before starting round 1
        await Task.Delay(5000);
        
        currentRound = 1;
        await dbReference.Child($"players/profile/player1_test/gameState/currentRound").SetValueAsync(currentRound);
        Debug.Log("Round 1 started after transition delay");
    }

    async Task InitializeDatabaseStructure()
    {
        try
        {
            await dbReference.Child($"players/profile/player1_test").RemoveValueAsync();
            var updates = new Dictionary<string, object>
            {
                {$"players/profile/info", new Dictionary<string, object>
                    {
                        {"ID", patientId},
                        {"displayName", "Player1"},
                        {"email", "player1@example.com"},
                        {"created", DateTime.UtcNow.ToString("o")}
                    }
                },
                {$"players/profile/player1_test/gameState", new Dictionary<string, object>
                    {
                        {"currentRound", 0}, // Start with round 0
                        {"currentSmell", new Dictionary<string, object>
                            {
                                {"lavender", false},
                                {"red", false}
                            }
                        }
                    }
                },
                {$"players/profile/player1_test/gameResults", CreateEmptyRounds()}
            };
            await dbReference.UpdateChildrenAsync(updates);
            await dbReference.Child("players/profile/isTrainingMode").SetValueAsync(true);
            Debug.Log("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Database initialization failed: {ex.Message}");
            throw;
        }
    }
  
    private Dictionary<string, object> CreateEmptyRounds()
    {
        var rounds = new Dictionary<string, object>();
        for (int i = 1; i <= 4; i++)
        {
            rounds[$"round{i}"] = new Dictionary<string, object>
            {
                {"decisionTime", 0f},
                {"smellType", ""},
                {"wasCorrect", false},
                {"completed", false}
            };
        }
        rounds["accuracy"] = $"{0}%";
        rounds["allRoundsCompleted"] = false;
        return rounds;
    }

    void SetupOfflineMode()
    {
        userId = "offline_user_" + SystemInfo.deviceUniqueIdentifier;
        currentRound = 1;
        SetupFlowers();
        Debug.LogWarning("Running in offline mode - data won't be saved to Firebase");
    }

    void SetupFlowers()
    {
        if (lavenderFlower != null)
        {
            var interactable = lavenderFlower.GetComponent<XRGrabInteractable>();
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(args => HandleFlowerGrabbed(lavenderFlower));
            }
        }

        if (redFlower != null)
        {
            var interactable = redFlower.GetComponent<XRGrabInteractable>();
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(args => HandleFlowerGrabbed(redFlower));
            }
        }
    }

    void StartListeningForSmellChanges()
    {
        dbReference.Child($"players/profile/player1_test/gameState/currentSmell").ValueChanged += HandleSmellChange;
    }

    void HandleSmellChange(object sender, ValueChangedEventArgs args)
{
    if (currentRound == 5) return;

    try
    {
        bool lavender = false, red = false;
        if (args.Snapshot.Exists)
        {
            if (args.Snapshot.Child("lavender").Exists)
                lavender = (bool)args.Snapshot.Child("lavender").Value;

            if (args.Snapshot.Child("red").Exists)
                red = (bool)args.Snapshot.Child("red").Value;
        }

        if (lavender && red)
        {
            Debug.LogWarning("Both smells are true - this shouldn't happen!");
            return;
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (lavender || red)
            {
                if (!isRoundActive && !waitingForSmellReset)
                {
                    StartNewRound(lavender ? "lavender" : "red");
                }

                lavenderFlower.SetActive(true);
                redFlower.SetActive(true);
            }
            else
            {
                lavenderFlower.SetActive(false);
                redFlower.SetActive(false);
                HideDecisionTime(); // Hide the time display when flowers disappear

                if (correctParticles != null) correctParticles.SetActive(false);
                if (wrongParticles != null) wrongParticles.SetActive(false);

                if (waitingForSmellReset)
                {
                    waitingForSmellReset = false;
                    isRoundActive = false;
                }
            }
        });
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error handling smell change: {ex.Message}");
    }
}

    void StartNewRound(string correctFlower)
    {
        if (currentRound == 5) return;

        isRoundActive = true;
        flowerGrabbed = false;
        currentCorrectFlower = correctFlower;
        roundStartTime = Time.time;
        waitingForSmellReset = false;

        Debug.Log($"Round {currentRound} started - Correct flower: {correctFlower}");
    }


    async void HandleFlowerGrabbed(GameObject grabbedFlower)
{
    if (!isRoundActive || flowerGrabbed || waitingForSmellReset) return;

    flowerGrabbed = true;
    wasCorrectChoice = (grabbedFlower == lavenderFlower && currentCorrectFlower == "lavender") ||
                     (grabbedFlower == redFlower && currentCorrectFlower == "red");

    // Calculate and show decision time
    float decisionTime = Time.time - roundStartTime;
    ShowDecisionTime(decisionTime);

    PlayFeedback(wasCorrectChoice);
    waitingForSmellReset = true;

    if (firebaseInitialized)
    {
        try
        {
            string roundKey = $"round{currentRound}";
            var updates = new Dictionary<string, object>
            {
                {"decisionTime", Math.Round(decisionTime, 2)},
                {"smellType", currentCorrectFlower},
                {"wasCorrect", wasCorrectChoice},
                {"completed", true}
            };
            await dbReference.Child($"players/profile/player1_test/gameResults/{roundKey}").UpdateChildrenAsync(updates);

            // Update current round in Firebase and locally
            currentRound++;
            await dbReference.Child($"players/profile/player1_test/gameState/currentRound").SetValueAsync(currentRound);

            // Only update accuracy if all rounds are completed
            if (currentRound > 4)
            {
                await UpdateAccuracy();
                await dbReference.Child($"players/profile/player1_test/gameResults/allRoundsCompleted").SetValueAsync(true);
            }

            StartCoroutine(DelayedResetSmell(currentCorrectFlower));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update Firebase: {ex.Message}");
        }
    }
}

    private IEnumerator DelayedResetSmell(string flowerName)
    {
        yield return new WaitForSeconds(5f);

        // Check if smell is still true before resetting
        var smellSnapshotTask = dbReference.Child($"players/profile/player1_test/gameState/currentSmell/{flowerName}").GetValueAsync();
        yield return new WaitUntil(() => smellSnapshotTask.IsCompleted);

        if (smellSnapshotTask.Result.Exists && smellSnapshotTask.Result.Value is bool smellIsTrue && smellIsTrue)
        {
            var resetTask = dbReference.Child($"players/profile/player1_test/gameState/currentSmell/{flowerName}").SetValueAsync(false);
            yield return new WaitUntil(() => resetTask.IsCompleted);

            Debug.Log($"{flowerName} smell auto-reset to false after 10 seconds.");
        }
        else
        {
            Debug.Log($"{flowerName} smell was manually turned off before 10 seconds, no auto-reset needed.");
        }
    }

    void PlayFeedback(bool isCorrect)
    {
        try
        {
            if (audioSource != null)
            {
                audioSource.clip = isCorrect ? correctAnswerSound : wrongAnswerSound;
                audioSource.Play();
            }

            if (isCorrect)
            {
                if (correctParticles != null)
                {
                    correctParticles.SetActive(true);
                    var particles = correctParticles.GetComponent<ParticleSystem>();
                    if (particles != null) particles.Play();
                }
                if (wrongParticles != null) wrongParticles.SetActive(false);
            }
            else
            {
                if (wrongParticles != null)
                {
                    wrongParticles.SetActive(true);
                    var particles = wrongParticles.GetComponent<ParticleSystem>();
                    if (particles != null) particles.Play();
                }
                if (correctParticles != null) correctParticles.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Feedback error: {ex.Message}");
        }
    }

    async Task UpdateAccuracy()
    {
        try
        {
            // First check if all rounds are completed
            bool allCompleted = true;
            for (int i = 1; i <= 4; i++)
            {
                var snapshot = await dbReference.Child($"players/profile/player1_test/gameResults/round{i}/completed").GetValueAsync();
                if (!snapshot.Exists || !(bool)snapshot.Value)
                {
                    allCompleted = false;
                    break;
                }
            }

            if (!allCompleted)
            {
                Debug.Log("Not all rounds completed - accuracy remains 0");
                return;
            }

            int correctCount = 0;
            for (int i = 1; i <= 4; i++)
            {
                var snapshot = await dbReference.Child($"players/profile/player1_test/gameResults/round{i}/wasCorrect").GetValueAsync();
                if (snapshot.Exists && snapshot.Value is bool isCorrect && isCorrect)
                {
                    correctCount++;
                }
            }

            int accuracy = (int)((correctCount / 4f) * 100);

            await dbReference.Child($"players/profile/player1_test/gameResults").UpdateChildrenAsync(
                new Dictionary<string, object> { { "accuracy", $"{accuracy}%" } }
            );
            ShowScore($"<b><color=#2A7FFF>Score : </color></b>{accuracy}%");
            ShowTransitionButtons();
            Debug.Log($"Accuracy updated: {accuracy}%");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update accuracy: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        if (dbReference != null && firebaseInitialized)
        {
            dbReference.Child($"players/profile/player1_test/gameState/currentSmell").ValueChanged -= HandleSmellChange;
        }
    }

    public void ShowScore(string message)
    {
        scorePanel.SetActive(true);
        scoreText.text = message;
    }

    public void HideScore()
    {
        scorePanel.SetActive(false);
    }
    void ShowTransitionButtons()
{
    if (trainAgainButton != null)
    {
        trainAgainButton.gameObject.SetActive(true);
        SetupButtonInteraction(trainAgainButton, () => ResetGame(true));
    }

    if (testButton != null)
    {
        testButton.gameObject.SetActive(true);
        SetupButtonInteraction(testButton, () => ResetGame(false));
    }
}

void SetupButtonInteraction(Button button, UnityEngine.Events.UnityAction action)
{
    button.onClick.RemoveAllListeners();
    button.onClick.AddListener(action);
    
    // For VR interaction
    var xrInteractable = button.GetComponent<XRSimpleInteractable>();
    if (xrInteractable == null)
    {
        xrInteractable = button.gameObject.AddComponent<XRSimpleInteractable>();
    }
    
    xrInteractable.selectEntered.RemoveAllListeners();
    xrInteractable.selectEntered.AddListener(_ => action.Invoke());
}

async void ResetGame(bool isTrainingMode)
{
    // Reset game state
    currentRound = 1;
    isRoundActive = false;
    flowerGrabbed = false;
    waitingForSmellReset = false;

    // Hide buttons and time display
    if (trainAgainButton != null) trainAgainButton.gameObject.SetActive(false);
    if (testButton != null) testButton.gameObject.SetActive(false);
    HideScore();
    HideDecisionTime();

    // Reset flowers
    lavenderFlower.SetActive(false);
    redFlower.SetActive(false);

    if (isTrainingMode)
    {
        // Reset to training mode
        await dbReference.Child("players/profile/player1_test/gameState/currentRound").SetValueAsync(1);
        await dbReference.Child("players/profile/player1_test/gameResults").SetValueAsync(CreateEmptyRounds());
        await dbReference.Child("players/profile/isTrainingMode").SetValueAsync(true);        
        FindObjectOfType<ModeTransitionManager>()?.ResetToTrainingMode();
    }
    else
    {
        // Start another test session
        await dbReference.Child("players/profile/player1_test/gameState/currentRound").SetValueAsync(1);
        await dbReference.Child("players/profile/player1_test/gameResults").SetValueAsync(CreateEmptyRounds());
        // No need to change training mode as we're staying in test mode
    }
}

void ShowDecisionTime(float time)
{
    if (timePanel != null && timeText != null)
    {
        timePanel.SetActive(true);
        timeText.text = $"Decision Time: {time.ToString("F2")}s";
    }
}

void HideDecisionTime()
{
    if (timePanel != null)
    {
        timePanel.SetActive(false);
    }
}
}