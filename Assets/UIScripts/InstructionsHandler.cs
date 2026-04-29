using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsHandler : MonoBehaviour
{
public void backbtn()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
