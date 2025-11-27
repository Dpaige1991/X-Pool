using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwoPlayerPocket : MonoBehaviour
{
    CueStickController cueStickControllerScript;
    OldGameManager gameManagerScript;
    Aim aimScript;
    AudioManager audioManagerScript;

    public TMP_Text assignBallDisplayText_1, assignBallDisplayText_2, foulText, selectedGroupText, winPlayerText, bottomMessageText;
    public TMP_Text targetBallID_1, targetBallID_2;
    public GameObject NoGroupImage_1, NoGroupImage_2, StripesImage_1, StripesImage_2, SolidsImage_1, SolidsImage_2;

    public Animator bottomMessageAnimator;

    public Image playerID_1, playerID_2;
    public Sprite activeSprite, simpleSprite;

    int currentPlayer;
    string player1Group = "", player2Group = "";
    public bool groupAssigned = false, correctBallPotted = false;
    public bool gameEnd = false;

    public float totalTimeInput, timeRemaining;

    List<Collider> player1PottedBalls = new List<Collider>();
    List<Collider> player2PottedBalls = new List<Collider>();

    public string player1Name, player2Name;
    public TMP_Text player1NameText, player2NameText, totalTimeText;
    public Slider highlightSlider_1, highlightSlider_2;

    public int totalPottedBallsCount, totalRacksCount;
    public TMP_Text currentPottedText1, currentPottedText2, player1NameInWinPanel, player2NameInWinPanel, totalPottedBallsText, totalRacksText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cueStickControllerScript = FindFirstObjectByType<CueStickController>();
        gameManagerScript = FindFirstObjectByType<OldGameManager>();
        aimScript = FindFirstObjectByType<Aim>();
        audioManagerScript = FindFirstObjectByType<AudioManager>();

        bottomMessageText = bottomMessageAnimator.gameObject.GetComponentInChildren<TMP_Text>();

        NoGroupImage_1.SetActive(true);
        SolidsImage_1.SetActive(false);
        StripesImage_1.SetActive(false);

        NoGroupImage_2.SetActive(true);
        SolidsImage_2.SetActive(false);
        StripesImage_2.SetActive(false);

        bottomMessageAnimator.gameObject.SetActive(true);
        foulText.gameObject.SetActive(true);
        selectedGroupText.gameObject.SetActive(true);
        winPlayerText.gameObject.SetActive(true);

        LoadPlayersInfoData();
        ResetGame();
        LoadRacksAndBallsCount();
        gameManagerScript.Load_2_PlayerSelectedProfile(playerID_1, playerID_2);
    }

    private void Update()
    {
        if(currentPlayer == 1)
        {
            highlightSlider_1.gameObject.SetActive(true);
            highlightSlider_2.gameObject.SetActive(false);
        }
        else if(currentPlayer == 2)
        {
            highlightSlider_1.gameObject.SetActive(false);
            highlightSlider_2.gameObject.SetActive(true);
        }
        Debug.Log(currentPlayer);

        UpdateTimer();
    }

    void ResetGame()
    {
        groupAssigned = false;
        correctBallPotted = false;
        player1Group = "";
        player2Group = "";
        currentPlayer = 1;
        player1PottedBalls.Clear();
        player2PottedBalls.Clear();

        highlightSlider_1.gameObject.SetActive(true);
        highlightSlider_2.gameObject.SetActive(false);

        targetBallID_1.text = "Target Balls";
        targetBallID_2.text = "Target Balls";

        timeRemaining = totalTimeInput;
        cueStickControllerScript.stopTimer = false;
        gameManagerScript.LoadLineBool();
    }

    public void LoadPlayersInfoData()
    {
        if (PlayerPrefs.HasKey("Player1Name") && PlayerPrefs.HasKey("Player2Name") && PlayerPrefs.HasKey("InputTotalTime"))
        {
            player1Name = PlayerPrefs.GetString("Player1Name");
            player2Name = PlayerPrefs.GetString("Player2Name");
            totalTimeInput = PlayerPrefs.GetInt("InputTotalTime");
        }
        else
        {
            player1Name = "PLAYER 1";
            player2Name = "PLAYER 2";
            totalTimeInput = 60;
        }

        player1NameText.text = player1Name;
        player2NameText.text = player2Name;
        totalTimeText.text = totalTimeInput.ToString();
        player1NameInWinPanel.text = player1Name;
        player2NameInWinPanel.text = player2Name;
    }

    IEnumerator OnTriggerEnter(Collider ball)
    {
       string ballTag = ball.tag;

        audioManagerScript.PlaySoundMechanics(audioManagerScript.pocketedBall);
       
        if(ballTag == "CueBall")
        {
            ball.transform.position = new Vector3(2, 2.57469f, 0);
            ball.attachedRigidbody.angularVelocity = Vector3.zero;
            StartCoroutine(HandleCueBallPotted());
            yield break;
        }

        if(ballTag == "BlackBall")
        {
            StartCoroutine(HandleBlackBallPotted());
            HandlePottedBall(ball, (currentPlayer == 1) ? player1PottedBalls : player2PottedBalls);
            yield break;
        }

        if(!groupAssigned)
        {
            selectedGroupText.GetComponent<Animator>().SetTrigger("ShowTrigger");
            AssignGroups(ballTag);
        }

        if(groupAssigned)
        {
            if(currentPlayer == 1 && ballTag == player1Group + "Ball")
            {
                HandlePottedBall(ball, player1PottedBalls);
                correctBallPotted = true; ;
            }
            else if(currentPlayer == 2 && ballTag == player2Group + "Ball")
            {
                HandlePottedBall(ball, player2PottedBalls);
                correctBallPotted = true;
            }
            else
            {
                while (bottomMessageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bottom Message In") && bottomMessageAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                {
                    yield return null;
                }

                bottomMessageAnimator.SetTrigger("ShowTrigger");
                bottomMessageText.text = "You Potted Your Opponent's Ball!";

                HandlePottedBall(ball, (currentPlayer == 1) ? player2PottedBalls : player1PottedBalls);
                Debug.Log("You Potted Opponent's Ball");
            }

            assignBallDisplayText_1.text = player1PottedBalls.Count + " /7";
            assignBallDisplayText_2.text = player2PottedBalls.Count + " /7";
        }
    }

    public IEnumerator CannotMoveCueBall()
    {
        bottomMessageAnimator.SetTrigger("ShowTrigger");
        bottomMessageText.text = "The cue ball isn't ready to be moved";

        yield break; // Ends the coroutine immediately
    }

    void UpdateTimer()
    {
        if (cueStickControllerScript.stopTimer) return;

        if (currentPlayer == 1) highlightSlider_1.value = timeRemaining / totalTimeInput;
        if (currentPlayer == 2) highlightSlider_2.value = timeRemaining / totalTimeInput;

        if(timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
            SwitchTurn();
        }

        int seconds = Mathf.Clamp(Mathf.FloorToInt(timeRemaining), 0, 99);

        totalTimeText.text = string.Format("{0:00}", seconds);
    }

    private void SwitchTurn()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        Debug.Log(currentPlayer + " is Playing New");
        timeRemaining = totalTimeInput;
        highlightSlider_1.value = 1f;
        highlightSlider_2.value = 1f;

        audioManagerScript.PlaySound(audioManagerScript.nextPlayerSelect);
    }

    void HandlePottedBall(Collider ball, List<Collider> pottedBalls)
    {
        cueStickControllerScript.balls.Remove(ball.attachedRigidbody);
        aimScript.ballObjects.Remove(ball.gameObject);
        pottedBalls.Add(ball);
        Destroy(ball.gameObject);
    }

    void AssignGroups(string ballTag)
    {
        if(ballTag == "SolidBall" || ballTag == "StripedBall")
        {
            if(currentPlayer == 1)
            {
                player1Group = (ballTag == "SolidBall") ? "Solid" : "Striped";
                player2Group = (player1Group == "Solid") ? "Striped" : "Solid";

                if(player1Group == "Solid")
                {
                    SolidsImage_1.SetActive(true);
                    StripesImage_1.SetActive(false);
                    SolidsImage_2.SetActive(false);
                    StripesImage_2.SetActive(true);
                }
                else
                {
                    SolidsImage_1.SetActive(false);
                    StripesImage_1.SetActive(true);
                    SolidsImage_2.SetActive(true);
                    StripesImage_2.SetActive(false);
                }

                NoGroupImage_1.SetActive(false);
                NoGroupImage_2.SetActive(false);
            }
            else
            {
                player2Group = (ballTag == "SolidBall") ? "Solid" : "Striped";
                player1Group = (player2Group == "Solid") ? "Striped" : "Solid";

                if (player2Group == "Solid")
                {
                    SolidsImage_1.SetActive(true);
                    StripesImage_1.SetActive(false);
                    SolidsImage_2.SetActive(true);
                    StripesImage_2.SetActive(false);
                }
                else
                {
                    SolidsImage_1.SetActive(false);
                    StripesImage_1.SetActive(true);
                    SolidsImage_2.SetActive(false);
                    StripesImage_2.SetActive(true);
                }

                NoGroupImage_1.SetActive(false);
                NoGroupImage_2.SetActive(false);
            }

            groupAssigned = true;

            Debug.Log("Groups assigned. Player 1 is: " + player1Group + ", Player 2 is: " + player2Group);
        }
    }

    IEnumerator HandleCueBallPotted()
    {
        cueStickControllerScript.moveCueBallAllow = true;

        if(currentPlayer == 1)
        {
            Debug.Log("Player 1 Potted Cue Ball");
        }
        else
        {
            Debug.Log("Player 2 Potted Cue Ball");
        }
        yield return null;
    }

    IEnumerator HandleBlackBallPotted()
    {
        if (currentPlayer == 1)
        {
            if (player1PottedBalls.Count == 7)
            {
                Debug.Log("Player 1 Won The Match");
            }
            else
            {
                Debug.Log("Player 2 Won The Match");
            }
        }
        else if(currentPlayer == 2) 
        {
            if (player2PottedBalls.Count == 7)
            {
                Debug.Log("Player 2 Won The Match");
            }
            else
            {
                Debug.Log("Player 1 Won The Match");
            }
        }

        winPlayerText.gameObject.SetActive(false);
        gameEnd = true;
        cueStickControllerScript.stopTimer = true;
        gameManagerScript.cameraButtonAnimator.SetBool("GoBack", true);
        gameManagerScript.ButtonPauseAnimator.SetBool("GoBack", true);
        cueStickControllerScript.powerSliderAnimator.SetBool("GoBack", true);
        audioManagerScript.PlaySound(audioManagerScript.crowdCheering);

        yield return new WaitForSecondsRealtime(3.3f);
        gameManagerScript.ShowWinPanel();
        currentPottedText1.text = player1PottedBalls.Count.ToString();
        currentPottedText2.text = player2PottedBalls.Count.ToString();

        totalPottedBallsCount += player1PottedBalls.Count + player2PottedBalls.Count;
        totalRacksCount++;
        PlayerPrefs.SetInt("PocktedBallsSaved", totalPottedBallsCount);
        PlayerPrefs.SetInt("RacksCountSaved", totalRacksCount);
    }

    void LoadRacksAndBallsCount()
    {
        totalPottedBallsCount = PlayerPrefs.GetInt("PocketedBallSaved", 0);
        totalRacksCount = PlayerPrefs.GetInt("RacksCountSaved", 0);

        totalPottedBallsText.text = totalPottedBallsCount.ToString();
        totalRacksText.text = totalRacksCount.ToString();
    }

    public void SavePocketBalls()
    {
        totalPottedBallsCount += player1PottedBalls.Count + player2PottedBalls.Count;
        PlayerPrefs.SetInt("PocketedBallsSaved", totalPottedBallsCount);
        LoadRacksAndBallsCount();
    }

    public IEnumerator HitMissedOrNot()
    {
        if(!correctBallPotted)
        {
            SwitchTurn();
        }

        correctBallPotted = false;
        yield break;
    }

    public void CheckLineColor(GameObject closetBall)
    {
        aimScript.closestBallTag = closetBall.tag.Replace("Ball", "");

        if(groupAssigned)
        {
            if(currentPlayer == 1)
            {
                string currentPlayerGroup = (player1Group == "Solid") ? "Solid" : "Striped";

                if(aimScript.closestBallTag == currentPlayerGroup)
                {
                    aimScript.ChangeLineColor(Color.white, Color.white, Color.white, aimScript.whiteEmission);
                }
                else if(aimScript.closestBallTag == "Black")
                {
                    if(player1PottedBalls.Count == 7)
                    {
                        aimScript.ChangeLineColor(Color.white, Color.white, Color.white, aimScript.whiteEmission);
                    }
                    else
                    {
                        aimScript.ChangeLineColor(Color.red, Color.red, Color.red, aimScript.redEmission);
                    }
                }
                else
                {
                    aimScript.ChangeLineColor(Color.red, Color.red, Color.red, aimScript.redEmission);
                }
            }
            else if(currentPlayer == 2)
            {
                string currentPlayerGroup = (player2Group == "Solid") ? "Solid" : "Striped";

                if (aimScript.closestBallTag == currentPlayerGroup)
                {
                    aimScript.ChangeLineColor(Color.white, Color.white, Color.white, aimScript.whiteEmission);
                }
                else if (aimScript.closestBallTag == "Black")
                {
                    if (player1PottedBalls.Count == 7)
                    {
                        aimScript.ChangeLineColor(Color.white, Color.white, Color.white, aimScript.whiteEmission);
                    }
                    else
                    {
                        aimScript.ChangeLineColor(Color.red, Color.red, Color.red, aimScript.redEmission);
                    }
                }
                else
                {
                    aimScript.ChangeLineColor(Color.red, Color.red, Color.red, aimScript.redEmission);
                }
            }
        }
    }
}
