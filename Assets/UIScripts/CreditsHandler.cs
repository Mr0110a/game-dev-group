using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  public void Backbtn()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
