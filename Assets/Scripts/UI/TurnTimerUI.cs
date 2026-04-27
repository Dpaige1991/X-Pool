using UnityEngine;
using UnityEngine.UI;

public class TurnTimerUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;

    [Header("UI Sliders")]
    [SerializeField] private Slider player1TimerSlider;
    [SerializeField] private Slider player2TimerSlider;

    [Header("Timer Settings")]
    [SerializeField] private float secondsPerTurn = 15f;

    private float _timeLeft;
    private bool _running;
    private PlayerId _activePlayer;

    private void Awake()
    {
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();

        SetupSlider(player1TimerSlider);
        SetupSlider(player2TimerSlider);
        ResetAllUI();
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnBeganUI += HandleTurnBegan;
            turnManager.OnTurnEndedUI += HandleTurnEnded;
            turnManager.OnMatchEndedUI += HandleMatchEnded;

            // Sync immediately to current state
            HandleTurnBegan(turnManager.CurrentPlayer);
        }
        else
        {
            Debug.LogError("[TurnTimerUI] TurnManager not found.");
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnBeganUI -= HandleTurnBegan;
            turnManager.OnTurnEndedUI -= HandleTurnEnded;
            turnManager.OnMatchEndedUI -= HandleMatchEnded;
        }
    }

    private void Update()
    {
        if (!_running) return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            UpdateActiveSliderFill();
            _running = false;

            if (turnManager != null)
                turnManager.ForceTurnTimeout();

            return;
        }

        UpdateActiveSliderFill();
    }

    private void HandleTurnBegan(PlayerId player)
    {
        _activePlayer = player;

        // Start immediately EVEN during BIH placement and ball rolling
        _timeLeft = secondsPerTurn;
        _running = true;

        // Optional: both full at turn start
        SetSlider(player1TimerSlider, 1f);
        SetSlider(player2TimerSlider, 1f);

        Debug.Log($"<color=orange>[Timer]</color> Started for {_activePlayer}. BIH={(turnManager != null && turnManager.BallInHand)}");
    }

    private void HandleTurnEnded(PlayerId player)
    {
        _running = false;
        Debug.Log($"<color=orange>[Timer]</color> Turn ended for {player}");
    }

    private void HandleMatchEnded()
    {
        _running = false;
        ResetAllUI();
        Debug.Log("<color=orange>[Timer]</color> Match ended. UI reset.");
    }

    private void UpdateActiveSliderFill()
    {
        float t01 = (secondsPerTurn <= 0f) ? 0f : Mathf.Clamp01(_timeLeft / secondsPerTurn);

        if (_activePlayer == PlayerId.Player1)
        {
            SetSlider(player1TimerSlider, t01);
            SetSlider(player2TimerSlider, 1f);
        }
        else
        {
            SetSlider(player2TimerSlider, t01);
            SetSlider(player1TimerSlider, 1f);
        }
    }

    private void ResetAllUI()
    {
        SetSlider(player1TimerSlider, 1f);
        SetSlider(player2TimerSlider, 1f);
    }

    private void SetupSlider(Slider s)
    {
        if (s == null) return;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.value = 1f;
        s.wholeNumbers = false;
    }

    private void SetSlider(Slider s, float value01)
    {
        if (s == null) return;
        s.value = Mathf.Clamp01(value01);
    }
}