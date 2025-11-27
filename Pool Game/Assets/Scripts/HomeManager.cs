using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    OldGameManager gameManagerScript;

    public GameObject blackBG, _2_PlayerPanel, loginPanel, environmentBG;

    public Animator homeButtonsAnimator, IDAnimator, settingsButtonAnimator, msgAnimator, msgAnimatorInLogin;
    Animator _2_PlayerPanelAnimator, loginPanelAnimator;

    public TMP_Text timeWarningText, playerMainDisplayText;
    public TMP_InputField player1Input, player2Input, timeInputField, playerMainInput, playerMainInputLoginPanel, AgeInput;

    public string player1Name, player2Name, playerMainName;
    public int inputTime, totalPocketedBallsCount, totalRacksCount;
    public TMP_Text totalPocketedBallsText, totalRacksText, totalRacksTextWithID;

    public Slider fieldViewSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        gameManagerScript = FindFirstObjectByType<OldGameManager>();
        _2_PlayerPanelAnimator = _2_PlayerPanel.GetComponent<Animator>();
        loginPanelAnimator = loginPanel.GetComponent<Animator>();
        _2_PlayerPanel.SetActive(false);
        loginPanelAnimator = loginPanel.GetComponent<Animator>();
        _2_PlayerPanel.SetActive(false);
        loginPanel.SetActive(false);
        blackBG.SetActive(false);

        player1Input.onValueChanged.AddListener(HandleInputChange1);
        player2Input.onValueChanged.AddListener(HandleInputChange2);
        playerMainInput.onValueChanged.AddListener(HandleInputChangeMain);
        playerMainInputLoginPanel.onValueChanged.AddListener(HandleInputChangeMainLoginPanel);

        Clear_2_PlayerInputs();
        LoadMainPlayerInputs();
        LoadRacksAndBallCount();
        LoadCameraFieldAndLineBool();
        gameManagerScript.ResetGenderToMale();
        StartCoroutine(ShowLoginPanel());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SettingsPanelIn()
    {
        homeButtonsAnimator.SetBool("GoBack", true);
        IDAnimator.SetBool("GoBack", true);
        settingsButtonAnimator.SetBool("GoBack", true);
        blackBG.SetActive(true);
        gameManagerScript.LoadMainPlayerInputs(gameManagerScript.playerMainDisplayText, playerMainInput, null);
        gameManagerScript.LoadCurrentProfile();

        yield return new WaitForSeconds(0.3f);

        gameManagerScript.OnSettingButtonClicked();
    }

    public void Clear_2_PlayerInputs()
    {
        player1Input.text = "";
        player2Input.text = "";
        timeInputField.text = "";

        PlayerPrefs.DeleteKey("Player1Name");
        PlayerPrefs.DeleteKey("Player2Name");
        PlayerPrefs.DeleteKey("InputTotalTime");

        Debug.Log("Player input and time cleared");
    }

    void HandleInputChange1(string input)
    {
        player1Input.text = input.ToUpper();
    }

    void HandleInputChange2(string input)
    {
        player2Input.text = input.ToUpper();
    }

    void HandleInputChangeMain(string input)
    {
        playerMainInput.text = input.ToUpper();
    }

    void HandleInputChangeMainLoginPanel(string input)
    {
        playerMainInputLoginPanel.text = input.ToUpper();
    }

    public void SaveMainPlayerInput()
    {
        ClearMainPlayerInputs();

        playerMainName = playerMainInput.text;
        SaveLoginData();
        playerMainDisplayText.text = playerMainName;

        PlayerPrefs.SetString("PlayerMainName", playerMainName);
        PlayerPrefs.SetInt("ProfileNumber", gameManagerScript.currentProfileNum);
        PlayerPrefs.Save();

        gameManagerScript.LoadCurrentProfile();
        Debug.Log("Saved Main: " + playerMainName);

        msgAnimator.SetTrigger("ShowTrigger");
    }

    public void LoadMainPlayerInputs()
    {
        if(PlayerPrefs.HasKey("PlayerMainName"))
        {
            playerMainName = PlayerPrefs.GetString("PlayerMainName");
        }

        playerMainDisplayText.text = playerMainName;
    }

    public void ClearMainPlayerInputs()
    {
        playerMainName = "";
        playerMainDisplayText.text = playerMainName;
        PlayerPrefs.DeleteKey("PlayerMainName");
        Debug.Log("Player input cleared");
    }

    void SaveLoginData()
    {
        if (PlayerPrefs.HasKey("AllowLogin")) return;

        gameManagerScript.AgeNum = int.Parse(AgeInput.text);
        playerMainName = playerMainInputLoginPanel.text;
        PlayerPrefs.SetInt("Age", gameManagerScript.AgeNum);
        PlayerPrefs.Save();
        msgAnimatorInLogin.SetTrigger("ShowTrigger");

        gameManagerScript.backButton.SetActive(true);
    }

    public void CameraFieldChange()
    {
        PlayerPrefs.SetFloat("CameraFieldSliderValue", fieldViewSlider.value);
        PlayerPrefs.Save();
    }

    public void ResetCameraField()
    {
        fieldViewSlider.value = 0.33647f;
        CameraFieldChange();


        gameManagerScript.lineTurnOn = true;
        PlayerPrefs.SetInt("LineBool", gameManagerScript.lineTurnOn ? 1 : 0);

        gameManagerScript.lockCameraView = false;
        PlayerPrefs.SetInt("LockCameraField", gameManagerScript.lockCameraView ? 1 : 0);

        PlayerPrefs.Save();
        gameManagerScript.LoadLineBool();
        gameManagerScript.LoadLockCameraFieldBool();
    }

    public void LoadRacksAndBallCount()
    {
        totalPocketedBallsCount = PlayerPrefs.GetInt("PocketedBallSaved", 0);
        totalRacksCount = PlayerPrefs.GetInt("RacksCountSaved", 0);

        totalPocketedBallsText.text = totalPocketedBallsCount.ToString();
        totalRacksText.text = totalRacksCount.ToString();
        totalRacksTextWithID.text = "Racks: " + totalRacksCount.ToString();
    }

    public void Save_2_PlayerInputs()
    {
        player1Name = player1Input.text;
        player2Name = player2Input.text;

        inputTime = int.Parse(timeInputField.text);

        Debug.Log("Time Input: " + inputTime);
        Debug.Log("Player 1 Name: " + player1Name);
        Debug.Log("Player 2 Name: " + player2Name);

        PlayerPrefs.SetString("Player1Name", player1Name);
        PlayerPrefs.SetString("Player2Name", player2Name);
        PlayerPrefs.SetInt("InputTotalTime", inputTime);
        PlayerPrefs.Save();
    }

    public void Load_2_PlayerInputs()
    {
        if(PlayerPrefs.HasKey("Player1Name") && PlayerPrefs.HasKey("Player2Name") && PlayerPrefs.HasKey("InputTotalTime"))
        {
            player1Name = PlayerPrefs.GetString("Player1Name");
            player2Name = PlayerPrefs.GetString("Player2Name");
            inputTime = PlayerPrefs.GetInt("InputTotalTime");

            player1Input.text = player1Name;
            player2Input.text = player2Name;
            timeInputField.text = inputTime.ToString();

            Debug.Log("Loaded Player 1 Name: " + player1Name);
            Debug.Log("Loaded Player 2 Name: " + player2Name);
            Debug.Log("Loaded Time Input: " + inputTime);
        }
    }

    IEnumerator ShowLoginPanel()
    {
        if(PlayerPrefs.HasKey("AllowLogin"))
        {
            yield break;
        }
        else
        {
            homeButtonsAnimator.SetBool("GoBack", true);
            IDAnimator.SetBool("GoBack", true);
            settingsButtonAnimator.SetBool("GoBack", true);
            blackBG.SetActive(true);

            yield return new WaitForSeconds(0.3f);
            loginPanel.SetActive(true);
        }
    }

    IEnumerator Back()
    {
        gameManagerScript.OnBackButtonClicked();
        StartCoroutine(BackLoginPanel());
        blackBG.SetActive(false);

        if(_2_PlayerPanel && _2_PlayerPanel.activeSelf)
        {
            _2_PlayerPanelAnimator.SetBool("GoBack", true);
        }

        yield return new WaitForSeconds(0.3f);
        gameManagerScript.LoadCurrentProfile();

        homeButtonsAnimator.SetBool("GoBack", false);
        IDAnimator.SetBool("GoBack", false);
        settingsButtonAnimator.SetBool("GoBack", false);

        if(_2_PlayerPanel && _2_PlayerPanel.activeSelf)
        {
            _2_PlayerPanel.SetActive(false);
        }
    }

    IEnumerator IDPanelSetActive()
    {
        homeButtonsAnimator.SetBool("GoBack", true);
        IDAnimator.SetBool("GoBack", true);
        settingsButtonAnimator.SetBool("GoBack", true);
        blackBG.SetActive(true);
        gameManagerScript.LoadMainPlayerInputs(gameManagerScript.playerMainDisplayText, playerMainInput, null);
        gameManagerScript.LoadCurrentProfile();

        yield return new WaitForSeconds(0.3f);

        gameManagerScript.settingsPanel.SetActive(true);
        foreach(Animator selected in gameManagerScript.selectShapeAnimators)
        {
            selected.gameObject.SetActive(true);
            selected.SetBool("SelectOut", true);
        }

        if(!gameManagerScript.backButton.activeSelf)
        {
            gameManagerScript.backButton.SetActive(true);
        }

        gameManagerScript.selectShapeAnimators[1].SetBool("SelectOut", false);
        yield return new WaitForSecondsRealtime(0.15f);
        gameManagerScript.infoPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(0.3f);
        gameManagerScript.previousOptionPanel = gameManagerScript.infoPanel;
    }

    IEnumerator _2_PlayerPanelSetActive()
    {
        homeButtonsAnimator.SetBool("GoBack", true);
        IDAnimator.SetBool("GoBack", true);
        settingsButtonAnimator.SetBool("GoBack", true);
        blackBG.SetActive(true);
        gameManagerScript.NoProfileSet_2_players();

        yield return new WaitForSeconds(0.3f);

        _2_PlayerPanel.SetActive(true);
        gameManagerScript.backButton.SetActive(true);
    }

    public void Start2PlayerGame()
    {
        if(string.IsNullOrWhiteSpace(player1Input.text) || string.IsNullOrWhiteSpace(player2Input.text))
        {
            timeWarningText.text = "Please enter Player names. They cannot be empty!";
            timeWarningText.GetComponent<Animator>().SetTrigger("ShowTrigger");
            return;
        }

        if(int.TryParse(timeInputField.text, out inputTime))
        {
            if(inputTime >= 5 && inputTime <= 60)
            {
                Save_2_PlayerInputs();
                gameManagerScript.Save_2_PlayerSelectedProfile();
                gameManagerScript.LoadScene("Stadium Scene");
            }
            else
            {
                timeWarningText.text = "Oops! The time must be within 5 to 60 seconds.";
                timeWarningText.GetComponent<Animator>().SetTrigger("ShowTrigger");
            }
        }
    }

    IEnumerator BackLoginPanel()
    {
        if(PlayerPrefs.HasKey("AllowLogin"))
        {
            yield break;
        }
        else
        {
            loginPanelAnimator.SetBool("GoBack", true);

            yield return new WaitForSeconds(0.3f);
            homeButtonsAnimator.SetBool("GoBack", false);
            IDAnimator.SetBool("GoBack", false);
            settingsButtonAnimator.SetBool("GoBack", false);
            blackBG.SetActive(false);

            loginPanel.SetActive(false);
            gameManagerScript.backButton.SetActive(false);

            gameManagerScript.AllowLogin = false;
            PlayerPrefs.SetInt("AllowLogin", gameManagerScript.AllowLogin ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public void LoadCameraFieldAndLineBool()
    {
        if(PlayerPrefs.HasKey("CameraFieldSliderValue"))
        {
            fieldViewSlider.value = PlayerPrefs.GetFloat("CameraFieldSliderValue");
        }
        else
        {
            fieldViewSlider.value = 0.33647f;
        }
        gameManagerScript.LoadLineBool();
        gameManagerScript.LoadLockCameraFieldBool();
        CameraFieldChange();
    }

    public void OnSettingsButtonCLicked() { StartCoroutine(SettingsPanelIn()); }

    public void OnInfoButtonClicked() { gameManagerScript.OnInfoButtonClicked();  }
    public void OnAudioButtonClicked() { gameManagerScript.OnAudioButtonClicked();  }
    public void OnDisplayButtonClicked() { gameManagerScript.OnDisplayButtonClicked(); }

    public void OnBackButtonClicked() { StartCoroutine(Back()); }
    public void OnLoginBackButtonClicked() { StartCoroutine(BackLoginPanel()); }

    public void OnIDButtonCLicked() { StartCoroutine(IDPanelSetActive()); }

    public void On_2_PlayerPanelClicked() { StartCoroutine(_2_PlayerPanelSetActive()); }
}
