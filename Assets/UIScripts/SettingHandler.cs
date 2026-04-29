using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingHandler : MonoBehaviour
{
   public Button onButton;
    public Button offButton;

    Color redColor  = new Color(0.545f, 0f, 0f);         // #8B0000
    Color darkColor = new Color(0.067f, 0.067f, 0.067f); // #111111

    void Start()
    {
        // Check if game is already fullscreen when scene opens
        if (Screen.fullScreen)
        {
            HighlightButton(onButton, offButton);
        }
        else
        {
            HighlightButton(offButton, onButton);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            // Go fullscreen with the users monitor size
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            HighlightButton(onButton, offButton);
        }
        else
        {
            // Go windowed at 1280x720
            Screen.SetResolution(1280, 720, false);
            HighlightButton(offButton, onButton);
        }
    }

    // Changes image color of selected button to red and deselected to dark
    void HighlightButton(Button selected, Button deselected)
    {
        selected.GetComponent<Image>().color   = redColor;
        deselected.GetComponent<Image>().color = darkColor;
    }

    public void BackBtn()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

