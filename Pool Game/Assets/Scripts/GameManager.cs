using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OldGameManager : MonoBehaviour
{
    public GameObject backButton, pausePanel, BlackBG, settingsPanel, winPanel, audioPanel, infoPanel, displayPanel, gameOverPanel;
    [NonSerialized] public GameObject previousPanel, previousOptionPanel;

    public Animator[] selectShapeAnimators;
    public Volume BlurVolume;

    Animator pausePanelAnimator, displayPanelAnimator, BackButtonAnimator, settingsPanelAnimator, audioPanelAnimator, infoPanelAnimator, homeButtonsAnimator, IDAnimator, settingsButtonAnimator;
    public Animator upperUIAnimator, cameraButtonAnimator, ButtonPauseAnimator;

    public TMP_Text playerMainDisplayText, AgeTMP;
    public string playerMainName;

    int currentSceneIndex;
    public bool UpperUIShift = true, lineTurnOn = true, MechanicSoundAllow = true, lockCameraView = false;

    CueStickController cueStickControllerScript;
    TwoPlayerPocket twoPlayerPocketScript;
    AudioManager audioManagerScript;

    public Image lineDisplayImage;
    public Sprite lineTurnOnSprite, lineTurnOffSprite;

    public Image mainIDImageInfoPanel, mainIDImageHomeMenu, player1Image, player2Image, mainIDImageInLoginPanel;
    public List<Sprite> profileSprites;
    public List<Image> profileImages;
    public int currentProfileNum, currentProfilePlayer_1_Num, currentProfilePlayer_2_Num, AgeNum;
    public bool maleGenderIsActive, AllowLogin = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cueStickControllerScript = GetComponent<CueStickController>();

        pausePanelAnimator = pausePanel.GetComponent<Animator>();
        BackButtonAnimator = backButton.GetComponent<Animator>();
        settingsPanelAnimator = settingsPanel.GetComponent<Animator>();
        audioPanelAnimator = audioPanel.GetComponent<Animator>();
        infoPanelAnimator = infoPanel.GetComponent<Animator>();
        displayPanelAnimator = displayPanel.GetComponent<Animator>();

        pausePanel.SetActive(false);
        BlackBG.SetActive(false);
        backButton.SetActive(false);
        settingsPanel.SetActive(false);
        audioPanel.SetActive(false);
        infoPanel.SetActive(false);
        displayPanel.SetActive(false);

        upperUIAnimator.SetBool("IdlePlace", true);
        upperUIAnimator.SetBool("GoBack", true);
    }

    public void OnPauseButtonClicked()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        BlackBG.SetActive(true);
        backButton.SetActive(true);
        cameraButtonAnimator.SetBool("GoBack", true);
        ButtonPauseAnimator.SetBool("GoBack", true);
        cueStickControllerScript.powerSliderAnimator.SetBool("GoBack", true);
    }

    IEnumerator SettingsPanelSetActive()
    {
        if(pausePanel && pausePanel.activeSelf)
        {
            pausePanelAnimator.SetBool("GoBack", true);
            StartCoroutine(DeactivatePanels(0.35f));
            previousPanel = pausePanel;
            yield return new WaitForSecondsRealtime(0.3f);
        }

        settingsPanel.SetActive(true);
        foreach(Animator selected in selectShapeAnimators)
        {
            selected.gameObject.SetActive(true);
            selected.SetBool("SelectOut", true);
        }

        if(!backButton.activeSelf)
        {
            backButton.SetActive(true);
        }

        selectShapeAnimators[0].SetBool("SelectOut", false);
        audioPanel.SetActive(true);
        previousOptionPanel = audioPanel;
    }

    IEnumerator InfoPanelSetActive()
    {
        if(previousOptionPanel == audioPanel)
        {
            audioPanelAnimator.SetBool("GoBack", true);
            selectShapeAnimators[0].SetBool("SelectOut", true);
        }
        if(previousOptionPanel == displayPanel)
        {
            displayPanelAnimator.SetBool("GoBack", true);
            selectShapeAnimators[2].SetBool("SelectOut", true);
        }

        StartCoroutine(DeactivateOptionPanel(0.3f));
        selectShapeAnimators[1].SetBool("SelectOut", false);
        yield return new WaitForSecondsRealtime(0.15f);
        infoPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(0.3f);
        previousOptionPanel = infoPanel;
    }

    IEnumerator AudioPanelSetActive()
    {
        if (previousOptionPanel == infoPanel)
        {
            infoPanelAnimator.SetBool("GoBack", true);
            selectShapeAnimators[1].SetBool("SelectOut", true);
        }
        if (previousOptionPanel == displayPanel)
        {
            displayPanelAnimator.SetBool("GoBack", true);
            selectShapeAnimators[2].SetBool("SelectOut", true);
        }

        StartCoroutine(DeactivateOptionPanel(0.3f));
        selectShapeAnimators[0].SetBool("SelectOut", false);
        yield return new WaitForSecondsRealtime(0.15f);
        audioPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(0.3f);
        previousOptionPanel = audioPanel;
    }

    IEnumerator DisplayPanelSetActive()
    {
        if (previousOptionPanel == infoPanel)
        {
            infoPanelAnimator.SetBool("GoBack", true);
            selectShapeAnimators[1].SetBool("SelectOut", true);
        }
        if (previousOptionPanel == audioPanel)
        {
            audioPanelAnimator.SetBool("GoBack", true);
            selectShapeAnimators[0].SetBool("SelectOut", true);
        }

        StartCoroutine(DeactivateOptionPanel(0.3f));
        selectShapeAnimators[2].SetBool("SelectOut", false);
        yield return new WaitForSecondsRealtime(0.15f);
        displayPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(0.3f);
        previousOptionPanel = displayPanel;
    }

    IEnumerator BackLoginPanel()
    {
        yield return null;
    }

    IEnumerator IDPanelSetActive()
    {
        homeButtonsAnimator.SetBool("GoBack", true);
        IDAnimator.SetBool("GoBack", true);
        settingsButtonAnimator.SetBool("GoBack", true);
        BlackBG.SetActive(true);
        yield return null;
    }

    IEnumerator BackPanelSetActive()
    {
        if (settingsPanel.activeSelf)
        {
            settingsPanelAnimator.SetBool("GoBack", true);
            StartCoroutine(DeactivateOptionPanel(0.3f));

            if (previousOptionPanel == audioPanel)
            {
                audioPanelAnimator.SetBool("GoBack", true);
                selectShapeAnimators[0].SetBool("SelectOut", true);
            }
            if (previousOptionPanel == infoPanel)
            {
                infoPanelAnimator.SetBool("GoBack", true);
                selectShapeAnimators[1].SetBool("SelectOut", true);
            }
            if (previousOptionPanel == displayPanel)
            {
                displayPanelAnimator.SetBool("GoBack", true);
                selectShapeAnimators[2].SetBool("SelectOut", true);
            }

            StartCoroutine(DeactivateOptionPanel(0.3f));
        }

        if(pausePanel && pausePanel.activeSelf)
        {
            ResumeGame();
        }

        yield return new WaitForSecondsRealtime(0.3f);

        if(pausePanel && previousOptionPanel == pausePanel)
        {
            pausePanel.SetActive(true);

            if(!backButton.activeSelf)
            {
                backButton.SetActive(true);
            }
        }
    }

    IEnumerator GameOverPanel()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        BlurVolume.enabled = true;
        yield return null;
    }

    public void LoadMainPlayerInputs(TMP_Text displayText, TMP_InputField inputField, object optionalParam = null)
    {
        if (displayText != null && inputField != null)
            displayText.text = inputField.text;  // take the player’s input and display it

        if (optionalParam != null)
            Debug.Log("Optional param: " + optionalParam);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanelAnimator.SetBool("GoBack", true);
        BlackBG.SetActive(false);
        BackButtonAnimator.SetBool("GoBack", true);

        if(cameraButtonAnimator && ButtonPauseAnimator)
        {
            cameraButtonAnimator.SetBool("GoBack", false);
            ButtonPauseAnimator.SetBool("GoBack", false);
            cueStickControllerScript.powerSliderAnimator.SetBool("GoBack", false);
        }

        StartCoroutine(DeactivatePanels(0.4f));
    }

    IEnumerator DeactivatePanels(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        pausePanel.SetActive(false);
        backButton.SetActive(false);
    }

    IEnumerator DeactivateSettingsPanel(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        settingsPanel.SetActive(false);
    }

    IEnumerator DeactivateOptionPanel(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (previousOptionPanel == audioPanel)
        {
            audioPanel.SetActive(false);
        }
        if (previousOptionPanel == infoPanel)
        {
            infoPanel.SetActive(false);
        }
        if (previousOptionPanel == displayPanel)
        {
            displayPanel.SetActive(false);
        }
    }

    IEnumerator HomeConfirmationPanel()
    {
        yield return null;
    }

    IEnumerator RestartConfirmationPanel()
    {
        yield return null;
    }

    IEnumerator WinPanel()
    {
        Time.timeScale = 0f;
        winPanel.SetActive(true);
        BlackBG.SetActive(true);
        yield return null;
    }

    public void OnUpperUIButtonClicked()
    {
        if(!cueStickControllerScript.isOnTopCameraActive)
        {
            UpperUIShift = !UpperUIShift;
            upperUIAnimator.SetBool("GoBack", !UpperUIShift);
        }
        else
        {
            upperUIAnimator.SetBool("GoBack", false);
        }
    }

    public void LoadLineBool()
    {
        if(PlayerPrefs.HasKey("LineBool"))
        {
            lineTurnOn = PlayerPrefs.GetInt("LineBool", 0) == 1;
        }
        else
        {
            lineTurnOn = true;
            PlayerPrefs.SetInt("LineBool", lineTurnOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        if(lineTurnOn)
        {
            lineDisplayImage.sprite = lineTurnOnSprite;
        }
        else
        {
            lineDisplayImage.sprite = lineTurnOffSprite;
        }

        SwitchLineAnimation();
    }

    public void SwitchLineTurnBool()
    {
        lineTurnOn = !lineTurnOn;

        if(lineTurnOn)
        {
            lineDisplayImage.sprite = lineTurnOnSprite;
        }
        else
        {
            lineDisplayImage.sprite = lineTurnOffSprite;
        }

        SwitchLineAnimation();
        PlayerPrefs.SetInt("LineBool", lineTurnOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SwitchLineAnimation()
    {
        if (displayPanelAnimator && displayPanelAnimator.gameObject.activeSelf)
        {
            displayPanelAnimator.SetBool("SwitchOff", !lineTurnOn);
        }
    }

    public void LoadCurrentProfile()
    {

    }

    void LoadVolumeSettingsMechanics()
    {
        if(PlayerPrefs.HasKey("MechanicsSound"))
        {
            MechanicSoundAllow = PlayerPrefs.GetInt("MechanicsSound", 0) == 1;
        }
        else
        {
            MechanicSoundAllow = true;
            PlayerPrefs.SetInt("MechanicsSound", MechanicSoundAllow ? 1 : 0);
            PlayerPrefs.Save();
        }

        SwitchMechanicsVolumeAnimation();
    }

    public void SwitchMechanicsVolumeBool()
    {
        MechanicSoundAllow = !MechanicSoundAllow;
        PlayerPrefs.SetInt("MechanicsSound", MechanicSoundAllow ? 1 : 0);
        PlayerPrefs.Save();

        SwitchMechanicsVolumeAnimation();
    }

    public void SwitchMechanicsVolumeAnimation()
    {
        if(audioPanel)
        {
            audioPanelAnimator.SetBool("SwitchOFF", !MechanicSoundAllow);
        }
    }

    public void LoadLockCameraFieldBool()
    {
        if(PlayerPrefs.HasKey("LockCameraField"))
        {
            lockCameraView = PlayerPrefs.GetInt("LockCameraField", 0) == 1;
        }
        else
        {
            lockCameraView = false;
            PlayerPrefs.SetInt("LockCameraField", lockCameraView ? 1 : 0);
            PlayerPrefs.Save();
        }

        SwitchLockViewAnimation();
    }

    public void SwitchLockCameraFieldTurnBool()
    {
        lockCameraView = !lockCameraView;

        SwitchLockViewAnimation();
        PlayerPrefs.SetInt("LockCameraField", lockCameraView ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SwitchLockViewAnimation()
    {
        if(displayPanelAnimator && displayPanelAnimator.gameObject.activeSelf)
        {
            displayPanelAnimator.SetBool("SwitchOffLock", !lockCameraView);
        }
    }

    public void OnSettingButtonClicked() { StartCoroutine(SettingsPanelSetActive()); }

    public void OnInfoButtonClicked() { StartCoroutine(InfoPanelSetActive()); }
    public void OnAudioButtonClicked() { StartCoroutine(AudioPanelSetActive()); }
    public void OnDisplayButtonClicked() { StartCoroutine(DisplayPanelSetActive()); }

    public void OnBackButtonClicked() { StartCoroutine(BackPanelSetActive()); }

    public void ShowGameOverPanel() { StartCoroutine(GameOverPanel());  }
    public void ShowWinPanel() { StartCoroutine(WinPanel()); }

    public void OnHomeConfirmationClicked() { StartCoroutine(HomeConfirmationPanel()); }
    public void OnRestartConfirmationClicked() { StartCoroutine(RestartConfirmationPanel()); }

    public void ChangeGender()
    {
        if(maleGenderIsActive)
        {
            for(int i = 0; i < 5; i++)
            {
                profileImages[i].gameObject.SetActive(true);
            }
            for (int i = 5; i < 10; i++)
            {
                profileImages[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 5; i < 10; i++)
            {
                profileImages[i].gameObject.SetActive(true);
            }
            for (int i = 0; i < 5; i++)
            {
                profileImages[i].gameObject.SetActive(false);
            }
        }

        maleGenderIsActive = !maleGenderIsActive;
    }

    public void ResetGenderToMale()
    {
        maleGenderIsActive = true;
        ChangeGender();
    }

    public void changePlayer_1Profile()
    {
        currentProfilePlayer_1_Num++;
        if (currentProfilePlayer_1_Num <= 9)
        {
            player1Image.sprite = profileSprites[currentProfilePlayer_1_Num];
        }
        else
        {
            currentProfilePlayer_1_Num = 0;
            player1Image.sprite = profileSprites[currentProfilePlayer_1_Num];
        }
    }

    public void changePlayer_2Profile()
    {
        currentProfilePlayer_2_Num++;
        if(currentProfilePlayer_2_Num <= 9)
        {
            player2Image.sprite = profileSprites[currentProfilePlayer_2_Num];
        }
        else
        {
            currentProfilePlayer_2_Num = 0;
            player2Image.sprite = profileSprites[currentProfilePlayer_2_Num];
        }
    }

    public void NoProfileSet_2_players()
    {
        currentProfilePlayer_1_Num = 9;
        currentProfilePlayer_2_Num = 9;
        player1Image.sprite = profileSprites[currentProfilePlayer_1_Num];
        player2Image.sprite = profileSprites[currentProfilePlayer_2_Num];
    }

    public void Save_2_PlayerSelectedProfile()
    {
        PlayerPrefs.SetInt("SelectedIDPlayers-1-Num", currentProfilePlayer_1_Num);
        PlayerPrefs.SetInt("SelectedIDPlayers-2-Num", currentProfilePlayer_2_Num);
        PlayerPrefs.Save();
    }

    public void Load_2_PlayerSelectedProfile(Image player1Image, Image player2Image)
    {
        currentProfilePlayer_1_Num = PlayerPrefs.GetInt("SelectedIDPlayers-1-Num");
        currentProfilePlayer_2_Num = PlayerPrefs.GetInt("SelectedIDPlayers-2-Num");

        player1Image.sprite = profileSprites[currentProfilePlayer_1_Num];
        player2Image.sprite = profileSprites[currentProfilePlayer_2_Num];
    }
}
