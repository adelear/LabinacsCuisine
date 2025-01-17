using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] AudioManager asm;  
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip pauseSound;
    [SerializeField] AudioClip gameMusic;
    [SerializeField] AudioClip titleMusic;
    [SerializeField] AudioClip buttonSound; 

    [SerializeField] AudioMixer audioMixer;

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button backButton;
    [SerializeField] Button backButton2; 
    [SerializeField] Button quitButton;
    [SerializeField] Button returnToMenuButton;
    [SerializeField] Button resumeGame;
    [SerializeField] Button creditsButton; 

    [Header("Menus")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject creditsMenu; 

    [Header("Sliders")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    void Start()
    {
        asm = GetComponent<AudioManager>();
        if (startButton)
        {
            startButton.onClick.AddListener(StartGame);
            EventTrigger startButtonTrigger = startButton.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(startButtonTrigger, PlayButtonSound); 
        }

        if (settingsButton)
        {
            EventTrigger settingsButtonTrigger = settingsButton.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(settingsButtonTrigger, PlayButtonSound);
            settingsButton.onClick.AddListener(ShowSettingsMenu);
        }

        if (creditsButton)
        {
            EventTrigger creditsButtoNTrigger = creditsButton.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(creditsButtoNTrigger, PlayButtonSound);
            creditsButton.onClick.AddListener(ShowCreditsPage);
        }

        if (backButton)
        {
            backButton.onClick.AddListener(ShowMainMenu);
            EventTrigger backButtonTrigger = backButton.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(backButtonTrigger, PlayButtonSound);
        }

        if (backButton2)
        {
            backButton2.onClick.AddListener(ShowMainMenu);
            EventTrigger backButtonTrigger = backButton2.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(backButtonTrigger, PlayButtonSound);
        }

        if (quitButton)
        {
            quitButton.onClick.AddListener(Quit);
            EventTrigger quitButtonTrigger = quitButton.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(quitButtonTrigger, PlayButtonSound); 
        }

        if (masterSlider)
        {
            masterSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(value, "MasterVol"));
        }

        if (musicSlider)
        {
            musicSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(value, "MusicVol"));
        }

        if (sfxSlider)
        {
            sfxSlider.onValueChanged.AddListener((value) => OnSliderValueChanged(value, "SFXVol")); 
        }

        if (resumeGame)
        {
            EventTrigger resumeGameTrigger = resumeGame.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(resumeGameTrigger, PlayButtonSound); 
            resumeGame.onClick.AddListener(UnpauseGame);
        }

        if (returnToMenuButton)
        {
            EventTrigger returnToMenuTrigger = returnToMenuButton.gameObject.AddComponent<EventTrigger>();
            AddPointerEnterEvent(returnToMenuTrigger, PlayButtonSound);
            returnToMenuButton.onClick.AddListener(LoadTitle);
        }
    }

    void LoadTitle()
    {
        SceneTransitionManager.Instance.LoadScene("MainMenu");
        Time.timeScale = 1.0f; 
    }
    void UnpauseGame()
    {
        Time.timeScale = 1.0f;
        GameManager.Instance.SwitchState(GameManager.GameState.GAME);
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.GetGameState() == GameManager.GameState.DEFEAT) return; 
        if (!pauseMenu) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);


            if (pauseMenu.activeSelf)
            {
                Time.timeScale = 0f; 
                GameManager.Instance.SwitchState(GameManager.GameState.PAUSE);   
                asm.PlayOneShot(pauseSound, false);
                pauseMenu.SetActive(true);
            }

            else
            {
                UnpauseGame();
            }

        }
    }
    void ShowSettingsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        if (masterSlider)
        {
            float value;
            audioMixer.GetFloat("MasterVol", out value);
            masterSlider.value = value + 80;
        }
        else
        {
            Debug.LogError("Audio Mixer is not assigned.");
        }

        if (musicSlider)
        {
            float value;
            audioMixer.GetFloat("MusicVol", out value);
            musicSlider.value = value + 80; 
        }

        if (sfxSlider)
        {
            float value;
            audioMixer.GetFloat("SFXVol", out value);
            sfxSlider.value = value + 80;
        }
    }

    void ShowCreditsPage()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false); 
        creditsMenu.SetActive(true);
    }
    void ShowMainMenu()
    {
        settingsMenu.SetActive(false);
        if (creditsMenu) creditsMenu.SetActive(false); 
        mainMenu.SetActive(true);
    }
    void StartGame()
    {
        SceneTransitionManager.Instance.LoadScene("Level"); ;
        Time.timeScale = 1.0f;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = gameMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Audio Source is not assigned.");
        }
    }

    void OnSliderValueChanged(float value, string volume)
    {
        audioMixer.SetFloat(volume, value - 80);
    }

    private void AddPointerEnterEvent(EventTrigger trigger, UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => action.Invoke());
        trigger.triggers.Add(entry);
    } 

    public void PlayButtonSound()
    {
        asm.PlayOneShot(buttonSound, false);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}