using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [NonSerialized] public Rigidbody cueBallRigidBody;
    [SerializeField] private Transform cueStickPivot, stickTransform;

    public float amountOfForce = 10.5f;
    public int numberOfForces = 12;
    public bool switch1;
    public string name = "oldName";
    public Vector3 newVector, stickEndPosition;
    public Camera mainCamera;

    public List<GameObject> Balls;
    public List<string> familyMembers;
    public float[] digits = {0.2f, 0.5f, 5, 12.5f};

    public float minSpeedToPlaySound = 0.1f;
    public float maxSpeedForMaxVolume = 10f;
    private float soundCooldown = 1f;
    private float lastSoundTime;

    public AudioManager audioManagerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioManagerScript = FindFirstObjectByType<AudioManager>();
        lastSoundTime = -soundCooldown;

        numberOfForces = 20;
        StartCoroutine(AssignBallsToList());
        StartCoroutine(PreventInitialSound());
    }

    private void OnCollisionEnter(Collision collision)
    {
        float currentTime = Time.time;
        float relativeSpeed = collision.relativeVelocity.magnitude;

        if(currentTime - lastSoundTime >= soundCooldown)
        {
            float volume = Mathf.Clamp01((relativeSpeed - minSpeedToPlaySound) / (maxSpeedForMaxVolume - minSpeedToPlaySound));

            if(collision.gameObject.CompareTag("CueBall") || collision.gameObject.CompareTag("SolidBall") || collision.gameObject.CompareTag("StripedBall") || collision.gameObject.CompareTag("BlackBall"))
            {
                audioManagerScript.PlaySoundMechanicsVolume(audioManagerScript.ballCollide, volume);
            }
            else if(collision.gameObject.CompareTag("Table"))
            {
                audioManagerScript.PlaySoundMechanicsVolume(audioManagerScript.edgeCollide, 1f);
            }
        }
    }

    private IEnumerator AssignBallsToList()
    {
        GameObject[] ballsWithTags = GameObject.FindGameObjectsWithTag("Ball");

        foreach (GameObject gameObject in ballsWithTags)
        {
            Balls.Add(gameObject);
            Debug.Log(gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        float lerpSpeed = 0f, delay = 0.05f;
        while(lerpSpeed < 1f)
        {
            stickTransform.localPosition = Vector3.MoveTowards(stickTransform.localPosition, stickEndPosition, lerpSpeed);
            lerpSpeed += Time.deltaTime * delay;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        AddForeOnCueBall();
        amountOfForce = OurBoolMethod();
        MouseInputRay();
    }

    void AddForeOnCueBall()
    {
        if (switch1 == true && numberOfForces == 13)
        {
            Vector3 Forward = cueStickPivot.forward;
            cueBallRigidBody.AddForce(Forward * 10, ForceMode.Impulse);
        }
        else if(amountOfForce != 1.5f)
        {
            Debug.Log("else : The switch is off.");
            return;
        }
        else
        {
            Debug.Log("else : The amountOfForce is equal to 1.5");
        }
    }

    float OurBoolMethod()
    {
        if(numberOfForces == 15)
        {
            return 100.5f;
        }
        return 50.5f;
    }

    void MouseInputRay()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.blue);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                cueStickPivot.LookAt(hit.point);
            }
        }
    }

    IEnumerator PreventInitialSound()
    {
        audioManagerScript.volumeAudioSource.enabled = false;
        yield return new WaitForSeconds(0.5f);
        audioManagerScript.volumeAudioSource.enabled = true;
    }

    public void StartPoolRack()
    {
        audioManagerScript.PlaySound(audioManagerScript.startPoolRack);
    }
}
