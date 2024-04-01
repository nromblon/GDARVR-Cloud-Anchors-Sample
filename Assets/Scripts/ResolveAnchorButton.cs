using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ResolveAnchorButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    private string anchorId;


    public void OnButtonPressed()
    {
        PlaneAnchorPlacer.Instance.ResolveAnchor(anchorId);
    }

    public void SetAnchorId(string val)
    {
        anchorId = val;
        tmp.text = val;
    }

    public string GetAnchorId()
    {
        return this.anchorId;
    }
}
