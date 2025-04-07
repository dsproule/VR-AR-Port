using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

[System.Serializable]
public class ClientNetworkTransform : NetworkTransform
{
    private Vector3 AR_pos_offset;
    private Quaternion AR_rot_offset;
    private Vector3 AR_headset_pos_offset;
    private Quaternion AR_headset_rot_offset;

    private bool runningOnHeadset;

    private void Start()
    {
        if (NetworkManager.Singleton.GetComponent<NetworkManagerExtras>().isServer == true)
            runningOnHeadset = true;
    }

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

    protected override void Update()
    {
        // Call the base class's Update method
        base.Update();
        Vector3 r_track_in_headset = new Vector3(0, 0, 0);

        (AR_pos_offset, AR_rot_offset, AR_headset_pos_offset, AR_headset_rot_offset) = PollForOffset();

        Quaternion R_vr2track = AR_rot_offset; // orientation of tracker in VR frame
        Quaternion R_ar2headset = AR_headset_rot_offset; // orientation of headset in AR frame 
        Quaternion R_headset2track = Quaternion.identity; // change based on stand dimensions
        Quaternion R_ar2track = R_ar2headset * R_headset2track;
        Quaternion R_vr2ar = R_vr2track * Quaternion.Inverse(R_ar2track);

        Vector3 r_track_in_vr = AR_pos_offset;
        Vector3 r_headset_in_ar = AR_headset_pos_offset;
        if (runningOnHeadset)
            r_track_in_headset = new Vector3(0f, -0.0172f, 0.08f); // change based on stand dimensions

        Vector3 r_ar2track_in_vr = r_headset_in_ar + r_track_in_headset;
        Vector3 r_ar_in_vr = r_track_in_vr - r_ar2track_in_vr;


        Quaternion R_vr2obj = GetComponent<Transform>().rotation;
        Vector3 r_obj_in_vr = GetComponent<Transform>().position;

        Vector3 r_ar2obj_in_ar = r_obj_in_vr - r_ar_in_vr;

        // send tranform with respect to AR frame
        GetComponent<Transform>().position = r_ar2obj_in_ar;

    }
    private (Vector3, Quaternion, Vector3, Quaternion) PollForOffset()
    {
        return (NetworkManager.Singleton.GetComponent<NetworkManagerExtras>().AR_pos_offset,
            NetworkManager.Singleton.GetComponent<NetworkManagerExtras>().AR_rot_offset,
            NetworkManager.Singleton.GetComponent<NetworkManagerExtras>().AR_headset_pos_offset,
            NetworkManager.Singleton.GetComponent<NetworkManagerExtras>().AR_headset_rot_offset);
    }
}
