using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Level2Scene");
    }
}