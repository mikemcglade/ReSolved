using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{

    public AudioSource backgroundMusic;
    public GameObject instructionsPanel;
    public Button instructionsButton;
    public Button startButton;
    public Button closeButton;

    //public Animation startScreenAnimation;


    void Start()
    {
        // Play background music
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }

        // Play start screen animation
        // if (startScreenAnimation != null)
        // {
        //     startScreenAnimation.Play();
        // }

        // Add listener to start button
        if (startButton != null)
        {
           startButton.onClick.AddListener(StartGame);
        }
        
        if (instructionsButton != null)
        {
            instructionsButton.onClick.AddListener(ShowInstructions);
        }

        // Add listener to close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideInstructions);
        }

        // Hide instructions panel initially
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }
}