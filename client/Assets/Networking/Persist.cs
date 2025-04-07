using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persist : MonoBehaviour
{
    public GameObject ARCamera;
    public GameObject PassThru;
    public GameObject TextDisplay;

    /* 
     * This could be done efficiently but i want it to be a very deliberate
     * design decision what we're making persistent and not because it 
     * shouldn't be done to everything.
     */
    void Start()
    {
        if (ARCamera)
            DontDestroyOnLoad(ARCamera);
        if (PassThru)
            DontDestroyOnLoad(TextDisplay);
        if (TextDisplay)
            DontDestroyOnLoad(PassThru);
        DontDestroyOnLoad(this);
    }
}
