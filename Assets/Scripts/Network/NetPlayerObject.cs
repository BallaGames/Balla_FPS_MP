using Unity.Netcode;
using UnityEngine;

public class NetPlayerObject : NetworkBehaviour
{
    public GameObject[] playerPrefabs;
    int characterIndex;

    bool triedSpawn;
    public GameObject playerSpawned;

    private void OnGUI()
    {
        if (!IsOwner)
            return;

        GUILayout.BeginVertical();
        GUILayout.Space(200);
        GUILayout.Label("Character Index:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            characterIndex = Mathf.Max(0, characterIndex - 1);
        }
        GUILayout.Label(characterIndex.ToString());
        if (GUILayout.Button(">"))
        {
            characterIndex = Mathf.Min(playerPrefabs.Length - 1, characterIndex + 1);
        }
        GUILayout.EndHorizontal();
        if (playerSpawned == null)
            triedSpawn = false;
        else
        {
            if(GUILayout.Button("Despawn player"))
            {
                AskDespawnPlayer_RPC();
            }
        }
        if (!triedSpawn && GUILayout.Button("Spawn Character!"))
        {
            triedSpawn = true;
            RequestCharacter_RPC(characterIndex);
        }
        GUILayout.EndVertical();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void RequestCharacter_RPC(int index)
    {

        Debug.Log($"Received spawn request from {OwnerClientId} for character {index}");

        characterIndex = index;
        var go = Instantiate(playerPrefabs[index]);
        var no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(OwnerClientId);
        playerSpawned = go;
        ReturnRequest_RPC(no);
    }
    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
    public void ReturnRequest_RPC(NetworkObjectReference objectReference)
    {
        if(objectReference.TryGet(out var nob))
        { 
            playerSpawned = objectReference;
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void AskDespawnPlayer_RPC()
    {
        playerSpawned.GetComponent<NetworkObject>().Despawn();
    }
}
