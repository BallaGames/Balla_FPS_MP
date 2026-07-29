using Unity.Netcode;
using UnityEngine;

public class ConnectTest : MonoBehaviour
{
    public NetworkManager nm;

    private void OnValidate()
    {
        if (nm == null)
            nm = GetComponent<NetworkManager>();
    }

    private void OnGUI()
    {
        if (nm == null)
            return;
        //if we're not connected to anything
        if(!nm.IsServer && !nm.IsClient)
        {
            if (GUILayout.Button("Client"))
            {
                nm.StartClient();
            }
            if (GUILayout.Button("Server"))
            {
                nm.StartServer();
            }
            if (GUILayout.Button("Host"))
            {
                nm.StartHost();
            }
        }
        else if(!nm.ShutdownInProgress)
        {
            if (GUILayout.Button("Shutdown"))
            {
                nm.Shutdown();
            }
        }

    }
}
