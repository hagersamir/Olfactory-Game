
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser User { get; private set; }
    public bool IsInitialized { get; private set; }

    [Header("Login UI")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text loginStatusText;

    [Header("Register UI")]
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordField;
    public TMP_Text registerStatusText;

    private DatabaseReference dbReference;

    async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializeFirebase()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                auth.StateChanged += AuthStateChanged;
                IsInitialized = true;
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve dependencies: {dependencyStatus}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase init failed: {e.Message}");
        }
    }

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != User)
        {
            User = auth.CurrentUser;
            if (User != null) Debug.Log($"User signed in: {User.Email}");
        }
    }

    public async void Login()
    {
        if (!IsInitialized) 
        {
            loginStatusText.text = "System not ready";
            return;
        }

        if (string.IsNullOrEmpty(emailLoginField.text) || string.IsNullOrEmpty(passwordLoginField.text))
        {
            loginStatusText.text = "Please fill all fields";
            return;
        }

        try
        {
            loginStatusText.text = "Signing in...";
            
            // 1. Authenticate user
            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(
                emailLoginField.text, 
                passwordLoginField.text);
            
            User = result.User;
            loginStatusText.text = "Login successful!";

            // 2. Save user data to Firebase
            await SaveUserData();

            // 3. Load game scene
            TransitionManager.LoadGameScene();
        }
        catch (Exception e)
        {
            loginStatusText.text = ParseAuthError(e);
            Debug.LogError($"Login failed: {e.Message}");
        }
    }

    private async Task SaveUserData()
    {
        try
        {
            // Create the complete user data structure
            var userData = new Dictionary<string, object>
            {
                ["profile"] = new Dictionary<string, object>
                {
                    ["email"] = User.Email,
                    ["displayName"] = User.DisplayName ?? User.Email.Split('@')[0]
                },
                ["sessionData"] = new Dictionary<string, object>
                {
                    ["currentZone"] = "none",
                    ["lastLogin"] = DateTime.UtcNow.ToString("o")
                },
                ["gameData"] = new Dictionary<string, object>
                {
                    ["mode2"] = new Dictionary<string, object>
                    {
                        ["gameState"] = new Dictionary<string, object>
                        {
                            ["currentRound"] = 0,
                            ["currentSmell"] = new Dictionary<string, object>
                            {
                                ["lavender"] = false,
                                ["red"] = false
                            }
                        },
                        ["gameResults"] = new Dictionary<string, object>()
                    }
                }
            };

            // Save to Firebase
            await dbReference.Child("players").Child(User.UserId).UpdateChildrenAsync(userData);
            Debug.Log("User data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save user data: {e.Message}");
            throw; // Re-throw to be caught in Login()
        }
    }

    public async void Register()
    {
        if (!IsInitialized) return;

        if (passwordRegisterField.text != confirmPasswordField.text)
        {
            registerStatusText.text = "Passwords don't match!";
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(
                emailRegisterField.text, 
                passwordRegisterField.text);

            // Update user profile
            var profile = new UserProfile {
                DisplayName = emailRegisterField.text.Split('@')[0]
            };
            await result.User.UpdateUserProfileAsync(profile);

            registerStatusText.text = "Registration successful!";
            UIManager.Instance.ShowLoginScreen();
        }
        catch (Exception e)
        {
            registerStatusText.text = ParseAuthError(e);
        }
    }

    private string ParseAuthError(Exception e)
    {
        if (!(e.GetBaseException() is FirebaseException firebaseEx))
            return "Unknown error occurred";

        switch (firebaseEx.ErrorCode)
        {
            case 5: return "Password too weak (min 6 characters)";
            case 6: return "Email already in use";
            case 7: return "Invalid email format";
            case 8: return "Incorrect password";
            case 9: return "Account not found";
            default: return firebaseEx.Message;
        }
    }

    void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= AuthStateChanged;
    }
}