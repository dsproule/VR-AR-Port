using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
//using UnityEditor.UIElements;

public class RpcHandler : NetworkBehaviour
{
    private CustomNetworkManager localNetMan;
    private Dictionary<string, ulong> registeredObjs = new Dictionary<string, ulong>();
    GameObject localUpdateText;


    private void Init()
    {
        localNetMan = FindObjectOfType<CustomNetworkManager>();
        if (localNetMan == null)
            Debug.Log("WARNING: Nework Manager could not be found");
        localNetMan.connectionMade = true;
        localUpdateText = localNetMan.GetComponent<NetworkManagerExtras>().textUpdatesHandle;

    }

    #region ServerRpc
    [ServerRpc(RequireOwnership = false)]
    private void SpawnRequestServerRpc(string PrefabName, Vector3 pos, ulong ClientId, string objLabel = null, string tag = null, bool grabbable = false)
    {
        var networkPrefabs = localNetMan.NetworkConfig.Prefabs.Prefabs;
        ulong netId;

        foreach (var networkPrefab in networkPrefabs)
        {
            if (networkPrefab.Prefab.name == PrefabName)
            {
                var instance = Instantiate(networkPrefab.Prefab);

                if (objLabel == "Pivot" || objLabel == "Compass")
                    DontDestroyOnLoad(instance);
                instance.transform.position = pos;

                NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
                instanceNetworkObject.SpawnWithOwnership(ClientId);
                netId = instanceNetworkObject.NetworkObjectId;

                if (objLabel != null)
                    RegisterObjCallback(netId, objLabel);
                if (tag != null)
                    SetTagClientRpc(netId, tag);
                if (grabbable != false)
                    AttachGrabClientRpc(netId);
                return;
            }
        }
        Debug.Log("ERROR: Prefab does not exist in list");
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateAROffsetServerRpc(Vector3 pos_offset, Quaternion rot_offset)
    {
        localNetMan.GetComponent<NetworkManagerExtras>().AR_pos_offset = pos_offset;
        localNetMan.GetComponent<NetworkManagerExtras>().AR_rot_offset = rot_offset;

        Transform headset = GameObject.Find("CenterEyeAnchor").GetComponent<Transform>();
        localNetMan.GetComponent<NetworkManagerExtras>().AR_headset_pos_offset = headset.position;
        localNetMan.GetComponent<NetworkManagerExtras>().AR_headset_rot_offset = headset.rotation;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnRequestServerRpc(ulong netId)
    {
        GameObject gameObject;
        if ((gameObject = GetObjFromId(netId)) == null)
            return;

        if (registeredObjs.ContainsKey(gameObject.name))
            registeredObjs.Remove(gameObject.name);
        NetworkObject instanceNetworkObject = gameObject.GetComponent<NetworkObject>();

        instanceNetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActiveSetServerRpc(ulong netId, bool setting)
    {
        GameObject targetObj = GetObjFromId(netId);

        targetObj.SetActive(setting);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TextUpdatesStatusOnServerRpc(bool status)
    {
        GameObject textUpdates = localNetMan.GetComponent<NetworkManagerExtras>().textUpdatesHandle;
        if (textUpdates != null)
        {
            textUpdates.SetActive(status);
        }
        else
            Debug.Log("Text Update does not exist");
    }

    [ServerRpc(RequireOwnership = false)]
    private void TextUpdatesServerRpc(string content)
    {
        GameObject textUpdates = localNetMan.GetComponent<NetworkManagerExtras>().textUpdatesHandle;
        if (textUpdates.activeSelf == true)
        {
            TMP_Text text = textUpdates.GetComponent<TMP_Text>();
            text.text = content;
        }
        else
            Debug.Log("Text Update is not active");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchSceneServerRpc(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        /* Maintains the tracking for hands */
        for (int i = 0; i < 2; i++)
        {
            var instance = GameObject.Find("Hand" + i);
            var instanceNetworkObject = instance.GetComponent<NetworkObject>();
            LinkHandController(instanceNetworkObject.NetworkObjectId, (uint)i);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void vfColorUpdateServerRpc(ulong netId, Color color)
    {
        GameObject vfMirror = GetObjFromId(netId);
        vfMirror.GetComponent<MeshRenderer>().material.color = color;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReparentObjServerRpc(string childRegisName, string parentRegisName)
    {
        GameObject child = RetrRegObj(childRegisName);
        GameObject parent = RetrRegObj(parentRegisName);

        child.transform.parent = parent.transform;

        /* Because this is called on chestcursor and compass. Creates persistence. */
        DontDestroyOnLoad(child);
        DontDestroyOnLoad(parent);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetLinePosServerRpc(ulong netId, int i, Vector3 pos)
    {
        GameObject lineObj = GetObjFromId(netId);
        if (lineObj == null)
        {
            Debug.Log("Could not find line obj");
            return;
        }
        LineRenderer line = lineObj.GetComponent<LineRenderer>();
        if (line != null)
            line.SetPosition(i, pos);
        else
            Debug.Log($"Could not find line renderer for obj with netId {netId}");
    }
    #endregion

    #region ClientRpc
    [ClientRpc]
    private void LinkHandControllerClientRpc(ulong netId, uint handCount)
    {
        return;
        GameObject mirrorObj;
        ObjSync mirrorSync;
        GameObject hand;

        hand = GameObject.Find("RightHand");
        mirrorObj = GetObjFromId(netId);
        mirrorSync = mirrorObj.GetComponent<ObjSync>();

        mirrorSync.MirrorTarget = hand.transform;
        mirrorSync.tag = "Hand";

    }

    [ClientRpc]
    private void AttachGrabClientRpc(ulong netId)
    {
        GameObject obj = GetObjFromId(netId);
        //obj.AddComponent<GrabbableObj>();
    }

    [ClientRpc]
    private void SetTagClientRpc(ulong netId, string tag)
    {
        GameObject obj = GetObjFromId(netId);
        obj.tag = tag;
    }

    [ClientRpc]
    private void InitClientRpc()
    {
        Init();
    }

    [ClientRpc]
    private void RegisterObjCallbackClientRpc(ulong netId, string objLabel)
    {
        registeredObjs[objLabel] = netId;
    }
    #endregion

    #region Callback
    /*
     * Rpc to request objects spawning. Will send over the name of the object in the 
     * spawnable objects list in the net manager as well as a 3d position vector.
     * Acts as an intermediate buffer so every object can use this call instead of
     * creating a new function signature every time from different scripts. Called by 
     * client.
     * 
     * Utilizes just Prefab name and position of where we want this to spawn
     */
    public void SpawnRequest(string NetPrefabName, Vector3 pos, string targetObj = null, string tag = null, bool grabbable = false)
    {
        SpawnRequestServerRpc(NetPrefabName, pos, NetworkManager.Singleton.LocalClientId, targetObj, tag, grabbable);
    }

    /*
     * By default the hand controller network object will spawn without performing
     * any tracking. This command sends the signal to the client to find them within 
     * its own tree and link it. Called by server.
     */
    public void LinkHandController(ulong netId, uint handCount)
    {
        RegisterObjCallback(netId, "h" + handCount);
        LinkHandControllerClientRpc(netId, handCount);
    }

    /* 
     * By default the RpcHandler isn't connected with the network manager when it is 
     * spawned in, this RPC is called upon initialization on both sides so that the 
     * handler is linked with the network manager. Called by server.
     */
    public void RunInit()
    {
        Init();
        InitClientRpc();
    }

    /*
     * GameObjects won't syncronize the active-status by default, this RPC sets it 
     * client-side and updates it on the server. Called by client.
     */
    public void ActiveSet(GameObject gameObject, bool setting)
    {
        var instanceNetworkObject = gameObject.GetComponent<NetworkObject>();
        ulong netId = instanceNetworkObject.NetworkObjectId;

        ActiveSetServerRpc(netId, setting);
    }

    /*
     * After a GameObject spawns we sometimes need values on the client to update
     * with reference to them so this acts as a callback register of all desired
     * network objects. Called by server.
     */
    private void RegisterObjCallback(ulong netId, string objLabel)
    {
        registeredObjs[objLabel] = netId;
        RegisterObjCallbackClientRpc(netId, objLabel);
    }

    public void ResetARSync(Vector3 pos_offset, Quaternion rot_offset)
    {
        UpdateAROffsetServerRpc(pos_offset, rot_offset);
    }

    /*
     * Similar to spawning, only the server can make despawn requests so this
     * allows the client to make such requests. Called by client.
     */
    public void DespawnRequest(GameObject gameObject)
    {
        if (!gameObject)
        {
            Debug.Log("GameObject does not exist! Value is null");
            return;
        }

        NetworkObject netObj = gameObject.GetComponent<NetworkObject>();

        if (registeredObjs.ContainsKey(gameObject.name))
            registeredObjs.Remove(gameObject.name);
        if (netObj != null)
            DespawnRequestServerRpc(netObj.NetworkObjectId);
    }

    /*
     * Makes the text disappear on the AR side. Called by the client.
     */
    public void TextUpdatesStatusOn(bool status)
    {
        if (localUpdateText)
            localUpdateText.SetActive(status);
        TextUpdatesStatusOnServerRpc(status);
    }

    /* Line VF */
    public void SetLinePos(GameObject lineObj, int i,  Vector3 pos)
    {
        if (lineObj.GetComponent<LineRenderer>() == null)
            return;
        lineObj.GetComponent<LineRenderer>().SetPosition(i, pos);
        var instanceNetworkObject = lineObj.GetComponent<NetworkObject>();
        ulong netId = instanceNetworkObject.NetworkObjectId;
        SetLinePosServerRpc(netId, i, pos);
    }

    /*
     * Updates the text being displayed in the headset. Called by client.
     */
    public void TextUpdates(string content)
    {
        if (localUpdateText)
            localUpdateText.GetComponent<TMP_Text>().text = content;
        TextUpdatesServerRpc(content);
    }

    /*
     * Request for the server to synchronously change the scenes for both
     * machines. Called by client.
     */
    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        SwitchSceneServerRpc(sceneName);
    }

    /* Used to assign the transform of an object to a parent object. Needed for 
     * network related objects because client is stripped of privileges.
     * Called by client */
    public void ReparentObj(string childRegisName, string parentRegisName)
    {
        ReparentObjServerRpc(childRegisName, parentRegisName);
    }
    #endregion

    /*  ===================== Utility Functions =====================   */

    /* Allows access to registered objects */
    public GameObject RetrRegObj(string objLabel)
    {
        if (!registeredObjs.ContainsKey(objLabel))
            return null;
        ulong netId = registeredObjs[objLabel];
        return GetObjFromId(netId);
    }

    public void vfColorUpdate(ulong netId, Color color)
    {
        vfColorUpdateServerRpc(netId, color);
    }

    #region Stats
    /* I have looked extensively and cannot find how the stats are passed between Start scene and Reach. 
     * As far as I'm concerned, it is voodoo being passed in the air. This facilitates it for the Pickup
     * scene.
     */
    //private CharacterStats stats;
    
    //public CharacterStats StatsCacheRetr()
    //{
    //    return stats;
    //}

    
    //public void StatsCacheWrite(JsonPlayerStats jsonStats)
    //{
    //    stats.userName = jsonStats.userName;
    //    stats.chestHeight = jsonStats.chestHeight;
    //    stats.pelvicBraceHeight = jsonStats.pelvicBraceHeight;
    //    stats.armsLength = jsonStats.armsLength;
    //    stats.maxReachE = jsonStats.maxReachE;
    //    stats.maxReachN = jsonStats.maxReachN;
    //    stats.maxReachNE = jsonStats.maxReachNE;
    //    stats.maxReachNW = jsonStats.maxReachNW;
    //    stats.startPos = jsonStats.startPos;
    //    Debug.Log("Saved Stats to rpcHandler");
 
    //}
    #endregion

    /*  ===================== Helper Functions =====================   */
    private GameObject GetObjFromId(ulong netId)
    {
        if (localNetMan == null)
            return null;

        localNetMan.SpawnManager.SpawnedObjects.TryGetValue(netId, out NetworkObject networkObject);
        if (networkObject == null)
            return null;
        return networkObject.gameObject;
    }
}