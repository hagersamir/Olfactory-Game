using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static void LoadGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public static void LoadAuthScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("AuthScene");
    }
}