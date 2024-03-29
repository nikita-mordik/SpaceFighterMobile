﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
using System.Collections;

public class HUD : MonoBehaviour
{
    public AudioMixer audioMixer;

    //переменные для подсчёта времени:
    private float timeInSecondsP;
    public static int minutsP;
    public static int secondsP;
    [SerializeField] TMP_Text[] minutes = null;
    [SerializeField] TMP_Text[] seconds = null;

    public Slider m_MusicSlider;  //тут хранится слайдер регулировки громкости музыки

    public Slider m_SoundSlider;  //тут хранится слайдер регулировки громкости звуков

    public CanvasGroup levelLoseWindow;
    public CanvasGroup levelCompletedWindow;
    public CanvasGroup gameMenu;
    public CanvasGroup settingsMenu;

    //private static bool gameIsPaused = false;

    public GameObject pauseButton;
    
    //тут хранятся текстовые поля очков:
    [SerializeField] private TMP_Text[] scoreText = null;

    public Toggle joystickToggle;
    public GameObject joystickPanel = null;
    public GameObject joystickControl = null;

    public Toggle accelerationToggle;
    public GameObject accelerationPanel = null;
    public GameObject accelerationControl = null;

    int score = 0;  //переменная для подсчёта очков

    Resolution[] resolutions;  //массив в котором хранятся все возможные разрешения экранов

    private static HUD _instance;

    public static HUD Instance { get => _instance; set => _instance = value; }
    public int Score { get => score; set => score = value; }

    private void Start()
    {
        if (_instance == null)
            _instance = this;

        //SetResolutions();
        
        if (PlayerSpaceSript.AccelerationControl)
            accelerationPanel.SetActive(true);
        else if (PlayerSpaceSript.JoystickControl)
            joystickPanel.SetActive(true);

        if (GameController.Instance.loadSceneFirstTime == false)
        {
            //загружаю сохранённые значения громкости:
            m_SoundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
            m_MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }

        if (SceneManager.GetActiveScene().buildIndex == 2)
            StartCoroutine(ShowControl());
    }

    private void Update()
    {
        CountTime();
    }

    IEnumerator ShowControl()
    {
        GameController.Instance.State = GameState.Pause;

        if (PlayerSpaceSript.accelerationControl)
        {
            accelerationControl.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);  //делаю задержку в 2 секунды реального времени, это игнорирует time.timescale         
            accelerationControl.SetActive(false);
            GameController.Instance.State = GameState.Play;
        }
        else if (PlayerSpaceSript.joystickControl)
        {
            joystickControl.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            joystickControl.SetActive(false);
            GameController.Instance.State = GameState.Play;
        }
    }

    public void ControlJoystick(bool controlJoystick)
    {
        controlJoystick = joystickToggle.isOn;

        if (controlJoystick)
        {
            GameController.Instance.ControlManagement = ControlManagement.Joystick;
            
            accelerationToggle.isOn = false;

            if (joystickPanel.activeInHierarchy == false)
            {
                joystickPanel.SetActive(true);
                accelerationPanel.SetActive(false);
            }
        }
    }

    public void ControlAceleration(bool controlAceleration)
    {
        controlAceleration = accelerationToggle.isOn;

        if (controlAceleration)
        {
            GameController.Instance.ControlManagement = ControlManagement.Acceleration;

            joystickToggle.isOn = false;

            if (accelerationPanel.activeInHierarchy == false)
            {
                accelerationPanel.SetActive(true);
                joystickPanel.SetActive(false);
            }
        }
    }

    void SetResolutions()  //метод для определения разрешения экрана
    {
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
    }

    //метод в котором идёт подсчёт очков
    public void UpdateScore(int scoreAdd)
    {
        Score += scoreAdd;  //тут идёт подсчёт очков, т.е. колличество убитых противников
        for (int i = 0; i < scoreText.Length; i++)
        {
            scoreText[i].text = "Score: " + Score.ToString();  //тут обновляем интерфейс в игре, т.е. постоянно меняется score, колличество набраных очков
        }
    }

    public void ShowWindow(CanvasGroup window)
    {
        window.alpha = 1f;
        window.blocksRaycasts = true;
        window.interactable = true;

        pauseButton.SetActive(false);

        GameController.Instance.State = GameState.Pause;  //ставлю всё в игре на паузу когда открываю окно настроек
    }

    public void HideWindow(CanvasGroup window)
    {
        window.alpha = 0;
        window.blocksRaycasts = false;
        window.interactable = false;

        pauseButton.SetActive(true);

        GameController.Instance.State = GameState.Play;
    }

    public void ExitPressed()      //метод для выхода из сцены
    {
        Application.Quit();
    }

    public void MenuPressed()
    {
        GameController.Instance.loadSceneFirstTime = false;

        SceneManager.LoadScene("MainMenu");
    }

    public void ReloadPressed()
    {
        GameController.Instance.loadSceneFirstTime = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //метод настройки громкости музыки:
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("m_volume", volume);

        PlayerPrefs.SetFloat("MusicVolume", volume);  //сохраняю значение ползунка громкости для перехода между сценами

        GameController.Instance.loadSceneFirstTime = false;
    }

    //метод настройки громкости звуков в игре:
    public void SetSoundVolume(float volume)
    {
        audioMixer.SetFloat("s_volume", volume);

        PlayerPrefs.SetFloat("SoundVolume", volume);  //сохраняю значение ползунка громкости для перехода между сценами

        GameController.Instance.loadSceneFirstTime = false;
    }

    public void NextLevelPressed()
    {
        GameController.Instance.loadSceneFirstTime = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);  //загружаю следующую сцену
    }

    //метод в котором реализуется секундомер:
    public void CountTime()
    {
        timeInSecondsP += Time.deltaTime;  //эта строка отвечает за одну еденицу времени и её увеличение

        secondsP = (int)(timeInSecondsP % 60);  //тут обозначаю что переменная secondsP, это 60% от переменной timeInSecondsP к которой постоянно прибавляется время, тобишь выходят секунды
        minutsP = (int)(timeInSecondsP / 60);  //тут тоже самое, но тут выходят минуты
        //следующие условия добавлены для адекватного отображения течения времени в интерфейсе
        for(int i=0; i < minutes.Length; i++)
        {
            for(int j = 0; j < seconds.Length; j++)
            {
                if (minutsP < 10)  //если текст минут что выводится на экран след.: 07, 08, 09, тогда:
                {
                    minutes[i].text = 0 + minutsP.ToString();  //к этому тексту добавляются ноль плюс минуты = 01,02...
                    if (secondsP < 10)  //если текст секунд что выводится след.: 01,02..., тогда:
                    {
                        seconds[j].text = 0 + secondsP.ToString();
                    }
                    else
                    {
                        seconds[j].text = secondsP.ToString();
                    }
                }
                else
                {
                    minutes[i].text = 0 + minutsP.ToString();
                    if (secondsP < 10)
                    {
                        seconds[j].text = 0 + secondsP.ToString();
                    }
                    else
                    {
                        seconds[j].text = secondsP.ToString();
                    }
                }
            }
        }
    }
}