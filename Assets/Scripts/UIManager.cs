using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject loginScreen;
    public GameObject registerScreen;
    public GameObject loadingScreen;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoginScreen()
    {
        SetAllScreens(false);
        loginScreen.SetActive(true);
    }

    public void ShowRegisterScreen()
    {
        SetAllScreens(false);
        registerScreen.SetActive(true);
    }

    public void ShowLoadingScreen(bool show = true)
    {
        SetAllScreens(false);
        loadingScreen.SetActive(show);
    }

    private void SetAllScreens(bool state)
    {
        loginScreen.SetActive(state);
        registerScreen.SetActive(state);
        loadingScreen.SetActive(state);
    }
}