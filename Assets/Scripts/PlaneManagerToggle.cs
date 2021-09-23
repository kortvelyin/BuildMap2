using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneManagerToggle : MonoBehaviour
{
    public ARPlaneManager planeManager;
    // Start is called before the first frame update
    void Start()
    {

        //if (isLocalPlayer)
            planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            Debug.Log("Plane manager found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchOffPlaneManager()
    {

        GetComponent<ARPlaneManager>().enabled = !GetComponent<ARPlaneManager>().enabled;
    }
}
