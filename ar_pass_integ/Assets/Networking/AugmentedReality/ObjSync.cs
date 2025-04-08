using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSync : MonoBehaviour
{
    public Transform MirrorTarget;
    private Transform MirrorSync;
    public Quaternion rot_snapshot;
    private Quaternion R_track2pelv = Quaternion.AngleAxis(-62.02f, Vector3.up);

    [Header("Functionality")]
    public bool copyTranslation = true;
    public bool copyRotation = false;

    private void Start()
    {
        MirrorSync = GetComponent<Transform>();
    }

    void Update()
    {
        if (MirrorTarget != null) {
            if (copyTranslation)
                MirrorSync.position = MirrorTarget.position;
            if (copyRotation)
                MirrorSync.rotation = rot_snapshot * MirrorTarget.rotation * R_track2pelv;
        }
    }
}
