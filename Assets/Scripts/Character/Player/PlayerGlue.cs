using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerCamera))]
public class PlayerGlue : NetworkBehaviour
{
    PlayerMotor motor;
    PlayerCamera cam;
    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();
        if (cam == null)
            cam = GetComponent<PlayerCamera>();
    }
    protected override void OnNetworkPostSpawn()
    {
        Debug.Log($"Player {gameObject.name} owned by me: {IsOwner}");

        cam.cineCam.enabled = IsOwner;
        motor.Motor.enabled = IsOwner;

        if (IsOwner)
            cam.cineCam.Prioritize();
    }
}
