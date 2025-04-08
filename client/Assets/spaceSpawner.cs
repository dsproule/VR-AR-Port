using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spaceSpawner : MonoBehaviour
{
    public GameObject prefab;
    public bool useNetwork = false;
    public Vector3 spawnLoc = new Vector3(0, 0, 0);

    public GameObject netBall = null;
    private bool spawnAttempted = false;    
    private RpcHandler rpcHandle;

    void Update()
    {
        // if the rpcHandler has the object registered, gets a refea
        if (rpcHandle != null)
            netBall = rpcHandle.RetrRegObj("netBall");

        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (useNetwork)
            {
                // first confirm connection to RpcHandler
                LocateRpcHandler();

                // if rpcHandle.* errors out, rpcHandle most likely did not connect
                
                if (!spawnAttempted)
                {
                    rpcHandle.SpawnRequest("networkedBall", Vector3.zero, "netBall");
                    spawnAttempted = true;
                } else
                {
                    rpcHandle.DespawnRequest(netBall);
                    spawnAttempted = false;
                }

            } else
            {
                // Branch for regular VR spawn
                Debug.Log("Spacebar pressed!");
                Instantiate(prefab);
            }
        }
    }

    public void LocateRpcHandler()
    {
        if (rpcHandle == null)
            rpcHandle = FindObjectOfType<RpcHandler>();
    }
}
