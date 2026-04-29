using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuHandler : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void SettingsBtn()
    {
        SceneManager.LoadScene("Settings");
    }

    public void InstructionsBtn()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void CreditsBtn()
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
