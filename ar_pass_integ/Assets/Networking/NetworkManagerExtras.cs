using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkManagerExtras : MonoBehaviour
{
    /* The custom network manager does not allow new public
     * vars to be added so this facilitates that
     */
    public Material leftHandMaterial;
    public Material rightHandMaterial;
    public GameObject textUpdatesHandle;
    public bool isTesting;
    public bool isServer;

    public Vector3 AR_pos_offset;
    public Quaternion AR_rot_offset;
    public Vector3 AR_headset_pos_offset;
    public Quaternion AR_headset_rot_offset;
}
