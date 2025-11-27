using UnityEngine;
using System.Collections;

public class FunctionDelayCaller : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetScript; // The script to call functions from
    [SerializeField] private string firstFunctionName;   // Name of first function
    [SerializeField] private string secondFunctionName;  // Name of second function
    [SerializeField] private float delay = 2f;           // Delay in seconds

    void Start()
    {
        StartCoroutine(CallFunctionsWithDelay());
    }

    IEnumerator CallFunctionsWithDelay()
    {
        if (targetScript == null) yield break;

        // Call the first function immediately
        targetScript.Invoke(firstFunctionName, 0f);

        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Call the second function
        targetScript.Invoke(secondFunctionName, 0f);
    }
}
