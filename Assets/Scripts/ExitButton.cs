using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    // Return to the title screen
    public void ExitGame()
    {
        SceneManager.LoadScene("StartScene");
    }
}