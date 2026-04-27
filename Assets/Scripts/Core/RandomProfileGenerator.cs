using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomProfileGenerator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text nameResultText;
    [SerializeField] private TMP_Text numberResultText;
    [SerializeField] private Image profileImage;
    [SerializeField] private Image matchResultImage;

    [Header("Sprites")]
    [SerializeField] private Sprite[] availableSprites;

    [Header("Number Settings")]
    [SerializeField] private int minNumber = 1;
    [SerializeField] private int maxNumber = 9999;

    private string[] firstNames =
    {
        "ALEX", "JORDAN", "TAYLOR", "MORGAN", "CHRIS",
        "DOMINIQUE", "RILEY", "CAMERON", "JESSE", "AVERY"
    };

    private void Start()
    {
        GenerateRandomProfile();
    }

    public void GenerateRandomProfile()
    {
        GenerateRandomName();
        GenerateRandomNumber();
        GenerateRandomSprite();
    }

    private void GenerateRandomName()
    {
        string first = firstNames[Random.Range(0, firstNames.Length)];

        nameText.text = first;
        nameResultText.text = first;
    }

    private void GenerateRandomNumber()
    {
        int randomNumber = Random.Range(minNumber, maxNumber + 1);
        numberText.text = randomNumber.ToString();
        numberResultText.text = randomNumber.ToString();
    }

    private void GenerateRandomSprite()
    {
        if (availableSprites == null || availableSprites.Length == 0)
            return;

        Sprite randomSprite = availableSprites[Random.Range(0, availableSprites.Length)];
        profileImage.sprite = randomSprite;
        matchResultImage.sprite = randomSprite;
    }
}