using UnityEngine;

public class VSSceneManager : MonoBehaviour
{
    public LocalPlayFabDataLoader localLoader;
    public OpponentProfileLoader opponentLoader;

    private void Start()
    {
        if (localLoader == null)
            Debug.LogError("LocalPlayFabDataLoader is not assigned!");

        if (opponentLoader == null)
            Debug.LogError("OpponentDataLoader is not assigned!");
    }
}
