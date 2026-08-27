using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This will need to be an interactable thing later on. For now, it should be fine
/// </summary>
public class BaseDoorController : NetworkBehaviour
{
    /// <summary>
    /// A vector2 that stores how open the door currently is.<br></br>
    /// X axis is left door and y axis is right door.
    /// <br></br>Doors should always be assigned based on their "side."
    /// </summary>
    public NetworkVariable<Vector2> doorOpenAmount = new();

    public BaseDoor leftDoor, rightDoor;

    public float doorOpenSpeed;

}
