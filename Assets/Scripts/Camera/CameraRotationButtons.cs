using UnityEngine;

public class CameraRotationButtons : MonoBehaviour
{
    public Transform CameraYawPivot;
    public float RotateSpeed = 60f;

    bool rotatingLeft;
    bool rotatingRight;

    void Update()
    {
        if (rotatingLeft)
            CameraYawPivot.Rotate(0, -RotateSpeed * Time.deltaTime, 0);

        if (rotatingRight)
            CameraYawPivot.Rotate(0, RotateSpeed * Time.deltaTime, 0);
    }

    public void StartRotateLeft() => rotatingLeft = true;
    public void StopRotateLeft() => rotatingLeft = false;

    public void StartRotateRight() => rotatingRight = true;
    public void StopRotateRight() => rotatingRight = false;
}