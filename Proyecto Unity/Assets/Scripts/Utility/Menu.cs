using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadLevel(int templateIndex)
    {
        // Sets a map template index and loads the main scene
        PlayerPrefs.SetInt("templateIndex", templateIndex);

        SceneManager.LoadScene("MainScene");
    }

    public void ExitGame()
    {
        // Quits the game
        Application.Quit();
    }

    public void ARGalleryScene()
    {
        // Loads the AR gallery scene
        SceneManager.LoadScene("AR_Gallery");
    }
}
