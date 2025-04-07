using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spaceSpawner : MonoBehaviour
{
    public GameObject prefab;
    public bool useNetwork = false;
    public Vector3 spawnLoc = new Vector3(0, 0, 0);

    private RpcHandler rpcHandle;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (useNetwork)
            {
                // first confirm connection to RpcHandler
                LocateRpcHandler();

                // if rpcHandle.* errors out, rpcHandle most likely did not connect

                rpcHandle.SpawnRequest("networkedBall", Vector3.zero);
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
