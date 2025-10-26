using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("House"); 
    }

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");

        Application.Quit();
    }
}