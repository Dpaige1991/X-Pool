using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class CueStickController : MonoBehaviour
{
    Transform cueStickPivot, stickTransform;
    public Transform targetTransform;
    public Camera mainCamera;
    public Rigidbody cueBall;
    public List<Rigidbody> balls;

    public CinemachineCamera cameraOnTop, cameraOnStick;

    float mouseSensitivity = 1, hitForceAmount = 20, stickHitSpeed = 5f, stickLeavingSpeed = 0.5f;
    float topRotationSensitivity = 0.8f, CamStickRotationSensitivity = 5f;

    Vector3 lastMousePosition, stickPullBack;
    Vector3 stickOriginalPosition = new Vector3(0, 0.6352608f, -2.349853f), 
        stickFarPosition = new Vector3(0, 3, -12), 
        stickHitPosition = new Vector3(0, 0.605f, -2.235f);

    Vector3 tableMinBounds = new Vector3(-3.5f, 0f, -1.7f),
        tableMaxBounds = new Vector3(3.5f, 0f, 1.7f);

    

    bool AllowRotateStickWhileRider, hitPeriod;
    [NonSerialized] public bool isDraggingStick = false, isDraggingCueBall = false, isOnTopCameraActive = false, moveCueBallAllow = true, initialMoveCueBall = false, stopTimer = false;
    
    public Slider powerSlider, fieldViewSlider;
    public Animator powerSliderAnimator;
    public Image sliderImage;

    float sliderHitForce, sliderColorValue;

    TwoPlayerPocket twoPlayerPocketScript;
    OldGameManager gameManagerScript;
    AudioManager audioManagerScript;
    private float targetObjectRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        twoPlayerPocketScript = FindFirstObjectByType<TwoPlayerPocket>();
        gameManagerScript = FindFirstObjectByType<OldGameManager>();
        audioManagerScript = FindFirstObjectByType<AudioManager>();
        cueStickPivot = GetComponent<Transform>();
        stickTransform = cueStickPivot.transform.GetChild(0);
        powerSliderAnimator = powerSlider.GetComponent<Animator>();

        AllowRotateStickWhileRider = true;

        cameraOnTop.Priority = 10;
        cameraOnStick.Priority = 20;

        initialMoveCueBall = true;
        moveCueBallAllow = true;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(HandleMouseInput());
        AdJustStickPivotToCueBalls();
        CheckingBallPosition();
        Debug.Log(AllowRotateStickWhileRider);
    }

    IEnumerator HandleMouseInput()
    {
        yield return new WaitForSeconds(0.1f);

        Camera activeCamera = mainCamera;

        Plane plane = new Plane(Vector3.up, cueStickPivot.position);

        int roughLayerMask = 1 << LayerMask.NameToLayer("RoughLayer");
        int ignoreRoughLayer = ~roughLayerMask;

        if(Input.GetMouseButtonDown(0) && AllowRotateStickWhileRider)
        {
            Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ignoreRoughLayer))
                {
                    if (hit.collider.CompareTag("CueBall"))
                    {
                        lastMousePosition = hitPoint;
                        isDraggingCueBall = true;
                    }
                    else
                    {
                        lastMousePosition = isOnTopCameraActive ? GetMouseWorldPosition() : Input.mousePosition;
                        isDraggingStick = true;
                    }
                }
            }
        }

        if(Input.GetMouseButton(0))
        {
            if(isDraggingCueBall)
            {
                Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
                if(plane.Raycast(ray, out float distance))
                {
                    if(moveCueBallAllow)
                    {
                        Vector3 hitPoint = ray.GetPoint(distance);

                        tableMinBounds.x = initialMoveCueBall ? 2f : tableMinBounds.x;

                        float clampedX = Mathf.Clamp(hitPoint.x, tableMinBounds.x, tableMaxBounds.x);
                        float clampedZ = Mathf.Clamp(hitPoint.z, tableMinBounds.z, tableMaxBounds.z);

                        cueBall.position = new Vector3(clampedX, cueBall.position.y, clampedZ);
                    }
                    else
                    {
                        if (twoPlayerPocketScript) StartCoroutine(twoPlayerPocketScript.CannotMoveCueBall());
                    }
                }
            }
            else if (isDraggingStick && AllowRotateStickWhileRider)
            {
                Vector3 currentMousePosition = isOnTopCameraActive ? GetMouseWorldPosition() : Input.mousePosition;

                if (isOnTopCameraActive)
                {
                    Vector3 lastDirection = lastMousePosition - cueStickPivot.position;
                    Vector3 currentDirection = currentMousePosition - cueStickPivot.position;

                    float angle = Vector3.SignedAngle(lastDirection, currentDirection, Vector3.up);
                    cueStickPivot.Rotate(Vector3.up, angle * topRotationSensitivity, Space.World);
                }
                else
                {
                    Vector3 mouseDelta = currentMousePosition - lastMousePosition;
                    cueStickPivot.Rotate(Vector3.up, mouseDelta.x * CamStickRotationSensitivity * Time.deltaTime, Space.Self);

                    if(!gameManagerScript.lockCameraView)
                    {
                        HandleStickCameraInput();
                    }
                }

                lastMousePosition = currentMousePosition;            
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            isDraggingStick = false;
            isDraggingCueBall = false;
        }
    }

    void HandleStickCameraInput()
    {
        float rotationSpeedMultiplier = 8f;
        float positionSpeedMultiplier = 0.1f;

        float mouseDeltaY = Input.GetAxis("Mouse Y");

        float minRotation = -80f, maxRotation = 0f;

        float currentRotationX = targetTransform.localEulerAngles.x > 180f
            ? targetTransform.localEulerAngles.x - 360f
            : targetTransform.localEulerAngles.x;

        targetObjectRotation = Mathf.Clamp(currentRotationX + mouseDeltaY * rotationSpeedMultiplier, minRotation, maxRotation);
        targetTransform.localEulerAngles = new Vector3(targetObjectRotation, 0f, 0f);

        float minPositionZ = 0f, maxPositionZ = 1f;
        float positionInput = -mouseDeltaY;

        float targetPositionZ = Mathf.Clamp(targetTransform.localPosition.z + positionInput * positionSpeedMultiplier, minPositionZ, maxPositionZ);
        targetTransform.localPosition = new Vector3(0f, 0f, targetPositionZ);
    }


    void AdJustStickPivotToCueBalls()
    {
        if (twoPlayerPocketScript && twoPlayerPocketScript.gameEnd) return;

        if (AreAllBallsStopped())
        {
            cueStickPivot.position = Vector3.MoveTowards(cueStickPivot.position, cueBall.position, Time.deltaTime * stickHitSpeed);
            if(hitPeriod)
            {
                targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, Vector3.zero, Time.deltaTime * 5f);
            }

            if (AllowRotateStickWhileRider)
            {
                stickTransform.localPosition = Vector3.MoveTowards(stickTransform.localPosition, stickOriginalPosition, Time.deltaTime * 3);
            }
        }
    }

    IEnumerator LineDisplayingAllow(float lineRendererDelay)
    {
        yield return new WaitForSeconds(lineRendererDelay);
        if (twoPlayerPocketScript && twoPlayerPocketScript.gameEnd) yield break;

        Aim.lineIsDisplaying = true;
        hitPeriod = false;
        stopTimer = false;
        powerSliderAnimator.SetBool("GoBack", true);

        if(twoPlayerPocketScript)
        {
            StartCoroutine(twoPlayerPocketScript.HitMissedOrNot());
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

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, cueStickPivot.position);

        if(plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    public void OnSliderValueChange()
    {
        if (hitPeriod) return;

        AllowRotateStickWhileRider = false;
        sliderHitForce = hitForceAmount * powerSlider.value;
        PullBackStick();
        Debug.Log(sliderHitForce);
    }

    public void OnSliderReleased()
    {
        if(sliderHitForce > 0f)
        {
            StartCoroutine(HitCueBall());
            StartCoroutine(ResetSlider());
            stopTimer = true;
        }
        else
        {
            AllowRotateStickWhileRider = true;
        }
    }

    IEnumerator ResetSlider()
    {
        float resetingSpeed = 0.5f;
        while(powerSlider.value > 0)
        {
            powerSlider.value = Mathf.MoveTowards(powerSlider.value, 0, Time.deltaTime * resetingSpeed);
            yield return null;
        }
    }

    IEnumerator HitCueBall()
    {
        float elapsedTime = 0f;
        while(elapsedTime < 1f)
        {
            stickTransform.localPosition = Vector3.Lerp(stickPullBack, stickOriginalPosition, Time.deltaTime * stickHitSpeed);
            elapsedTime += Time.deltaTime * stickHitSpeed;
            yield return null;
        }

        audioManagerScript.PlaySoundMechanicsVolume(audioManagerScript.StickHit, sliderHitForce);

        Vector3 hitDirection = cueStickPivot.forward;
        cueBall.AddForce(hitDirection * sliderHitForce, ForceMode.Impulse);

        Aim.lineIsDisplaying = false;

        hitPeriod = true;
        AllowRotateStickWhileRider = true;
        moveCueBallAllow = false;
        initialMoveCueBall = false;

        elapsedTime = 0f;
        while(elapsedTime > 1f)
        {
            stickTransform.localPosition = Vector3.Lerp(stickOriginalPosition, stickFarPosition, elapsedTime);
            elapsedTime += Time.deltaTime * stickLeavingSpeed;
            yield return null;
        }

        StartCoroutine(LineDisplayingAllow(2f));
        sliderHitForce = 0f;
    }

    void PullBackStick()
    {
        stickPullBack = stickOriginalPosition - stickTransform.localRotation * Vector3.forward * (sliderHitForce / hitForceAmount);
        if(sliderHitForce > 0)
        {
            stickTransform.localPosition = Vector3.MoveTowards(stickTransform.localPosition, stickPullBack, Time.deltaTime * stickHitSpeed);
        }
        else
        {
            stickTransform.localPosition = Vector3.MoveTowards(stickTransform.localPosition, stickOriginalPosition, Time.deltaTime * stickHitSpeed);
        }
    }

    public void CameraTransition()
    {
        isOnTopCameraActive = !isOnTopCameraActive;

        if(isOnTopCameraActive)
        {
            cameraOnTop.Priority = 10;
            cameraOnStick.Priority = 1;

            if (!gameManagerScript.upperUIAnimator) return;
            gameManagerScript.upperUIAnimator.SetBool("IdlePlace", false);
            gameManagerScript.upperUIAnimator.SetBool("GoBack", false);
        }
        else
        {
            cameraOnTop.Priority = 1;
            cameraOnStick.Priority = 10;

            if (!gameManagerScript.upperUIAnimator) return;
            gameManagerScript.upperUIAnimator.SetBool("IdlePlace", false);
            gameManagerScript.upperUIAnimator.SetBool("GoBack", false);
        }
    }

    public void CameraFieldChange()
    {
        float maxField = 50f;
        float minField = 25f;

        float sliderValue = fieldViewSlider.value;
        PlayerPrefs.SetFloat("CameraFieldSliderValue", fieldViewSlider.value);
        PlayerPrefs.Save();

        float newFieldOfView = Mathf.Lerp(minField, maxField, sliderValue);

        cameraOnStick.Lens.FieldOfView = newFieldOfView;
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

    bool AreAllBallsStopped()
    {
        foreach(Rigidbody ball in balls)
        {
            if(ball.angularVelocity.sqrMagnitude > 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    void CheckingBallPosition()
    {
        float distanceFromPivotToCue = Vector3.Distance(cueBall.transform.position, cueStickPivot.transform.position);

        if(distanceFromPivotToCue > 0.1f)
        {
            Aim.lineIsDisplaying = false;
        }
        else
        {
            if(!hitPeriod)
            {
                Aim.lineIsDisplaying = true;
            }
        }
    }
}
