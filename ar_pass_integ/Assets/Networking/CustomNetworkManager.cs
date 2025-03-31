using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CustomNetworkManager : NetworkManager
{
    public RpcHandler rpcHandle;
    private NetworkManagerExtras extraConfig;
    public bool connectionMade;
    
    void Start()
    {
        OnConnectionEvent += ClientJoin;
        OnClientDisconnectCallback += EndExperiment;

        /* This next line needs to be commented out if not deploying on the headset */
        extraConfig = GetComponent<NetworkManagerExtras>();

        if (extraConfig.isServer == true)
            NetworkManager.Singleton.StartServer();
    }

    void EndExperiment(ulong clientId)
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void OnDestroy()
    {
        OnConnectionEvent -= ClientJoin;
    }

    private void ClientJoin(NetworkManager networkManager, ConnectionEventData eventData)
    {
        if (!IsServer)
            return;

        if (rpcHandle == null)
            CreateRpcHandler();

        var networkPrefabs = networkManager.NetworkConfig.Prefabs.Prefabs;
        Debug.Log("Client has joined!");

        if (extraConfig.isTesting)
            return;
        /* Spawn the hands */
        foreach (var networkPrefab in networkPrefabs)
        {
            if (networkPrefab.Prefab.name == "StreamObjectMirror")
            {
                for (int i = 0; i < 2; i++)
                {
                    var instance = Instantiate(networkPrefab.Prefab);
                    if (i == 0)
                        instance.GetComponent<MeshRenderer>().material = extraConfig.leftHandMaterial;
                    else
                        instance.GetComponent<MeshRenderer>().material = extraConfig.rightHandMaterial;
                    instance.name = "Hand" + i;
                    DontDestroyOnLoad(instance);
                    var instanceNetworkObject = instance.GetComponent<NetworkObject>();
                    instanceNetworkObject.SpawnWithOwnership(eventData.ClientId);
                    rpcHandle.LinkHandController(instanceNetworkObject.NetworkObjectId, (uint)i);
                }
            }
        }
    }

    private void CreateRpcHandler()
    {
        var networkPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

        foreach (var networkPrefab in networkPrefabs)
        {
            if (networkPrefab.Prefab.name == "RpcHandler")
            {
                var instance = Instantiate(networkPrefab.Prefab);
                DontDestroyOnLoad(instance);
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
                instanceNetworkObject.Spawn();
                rpcHandle = instance.GetComponent<RpcHandler>();
                rpcHandle.RunInit();
                break;
            }
        }
    }
}
