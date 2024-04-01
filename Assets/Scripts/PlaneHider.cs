using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneHider : MonoBehaviour
{
    [SerializeField] private XROrigin xrOriginObject;
    private bool isVisible = true;

    void Update()
    {
        foreach (var visualizer in xrOriginObject.GetComponentsInChildren<ARPlaneMeshVisualizer>())
        {
            visualizer.enabled = isVisible;
        }
    }

    public void TogglePlaneVisibility()
    {
        if (xrOriginObject == null)
            return;

        isVisible = !isVisible;
        foreach (var visualizer in xrOriginObject.GetComponentsInChildren<ARPlaneMeshVisualizer>())
        {
            visualizer.enabled = isVisible;
        }

    }
}
