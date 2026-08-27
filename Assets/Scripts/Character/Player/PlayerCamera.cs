using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public CinemachineCamera cineCam;
    float targetRoll;
    public float rollLerpSpeed;
    public void UpdateCam(float yaw = 0, float pitch = 0, float roll = 0)
    {
        //Do pitch and yaw world-space
        cineCam.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        //Then roll in local-space
        targetRoll = Mathf.Lerp(targetRoll, roll, 1- Mathf.Exp(-rollLerpSpeed * Time.deltaTime));
        cineCam.transform.localRotation *= Quaternion.Euler(0, 0, targetRoll);
    }
}
