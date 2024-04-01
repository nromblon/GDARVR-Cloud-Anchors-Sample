using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class PlaneAnchorPlacer : MonoBehaviour
{
    private static PlaneAnchorPlacer sharedInstance;
    public static PlaneAnchorPlacer Instance
    {
        get { return sharedInstance; }
    }

    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Vector3 contentOffset = new Vector3(0, .05f, 0);
    [SerializeField] private bool onlyPlaceOnce = false;
    private ARRaycastManager raycastManager;
    private ARAnchorManager anchorManager;
    private List<ARRaycastHit> hits;

    private GameObject placedAnchor;

    private void Awake()
    {
        sharedInstance = this;
    }

    private void Start()
    {
        anchorManager = GetComponent<ARAnchorManager>();
        raycastManager = GetComponent<ARRaycastManager>();
        hits = new List<ARRaycastHit>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(r.origin, r.direction, Color.red, 1.5f);

            // Physics Raycast first to check if tapping on an existing virtual object
            RaycastHit hitInfo;
            if (Physics.Raycast(r, out hitInfo))
            {
                if (hitInfo.collider.gameObject.CompareTag("Removable"))
                {
                    Destroy(hitInfo.collider.gameObject.GetComponentInParent<ARAnchor>().gameObject);
                    return; // exit out of update
                }

            }
            // Raycast only checks against plane within polygon trackables
            if (raycastManager.Raycast(r, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Debug.Log($"Hit Count: {hits.Count}");
                foreach (ARRaycastHit hit in hits)
                {
                    // Check if plane hit is aligned horizontally (up)
                    if (hit.trackable is ARPlane plane && plane.alignment == UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalUp)
                    {
                        AnchorContent(hit.pose.position);
                        break;  // break, so only checks up until the first valid plane
                    }
                }
            }
        }
    }

    /**
     * Code Taken from: https://discussions.unity.com/t/detect-if-pointer-is-over-any-ui-element/138619
     */
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void AnchorContent(Vector3 worldPos)
    {
        if (onlyPlaceOnce)
        {
            if (placedAnchor != null)
                Destroy(placedAnchor.gameObject);
        }

        GameObject newAnchor = new GameObject("Anchor");
        newAnchor.transform.parent = null;
        newAnchor.transform.position = worldPos;
        newAnchor.AddComponent<ARAnchor>();

        GameObject content = Instantiate(contentPrefab, newAnchor.transform);
        content.transform.localPosition = contentOffset;

        placedAnchor = newAnchor;

        HostCloudAnchorPromise promise = anchorManager.HostCloudAnchorAsync(placedAnchor.GetComponent<ARAnchor>(), 1);
        StartCoroutine(WaitForHostPromise(promise));
    }

    private IEnumerator WaitForHostPromise(HostCloudAnchorPromise promise)
    {
        // Wait until async operation finishes (i.e., uploading of cloud anchor data)
        yield return promise;
        // Lines after are when the promise has been resolved / rejected.
        if (promise.State == PromiseState.Cancelled) yield break;
        
        Debug.Log(promise.Result.CloudAnchorState);

        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            // Upload Cloud Anchor Id to server
            Debug.Log($"Cloud Anchor Id: {promise.Result.CloudAnchorId}");
            WebServerManager.Instance.UploadHostedAnchor(promise.Result.CloudAnchorId);
        }
    }

    public void ResolveAnchor(string cloudAnchorId)
    {
        Debug.Log("Resolving cloud anchor: " + cloudAnchorId);
        ResolveCloudAnchorPromise promise = anchorManager.ResolveCloudAnchorAsync(cloudAnchorId);
        StartCoroutine(WaitForResolvePromise(promise));
    }

    private IEnumerator WaitForResolvePromise(ResolveCloudAnchorPromise promise)
    {
        // Wait until async operation finishes (i.e., resolving of cloud anchor data)
        yield return promise;

        // Lines after are when the promise has been resolved / rejected.
        if (promise.State == PromiseState.Cancelled)
        {

            Debug.Log("cancelled");
            yield break;
        }

        Debug.Log(promise.Result.CloudAnchorState);

        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            // Upload Cloud Anchor Id to server
            Debug.Log($"resolved cloud anchor trackable Id: {promise.Result.Anchor.trackableId}");
            Debug.Log($"resolved gameobject name: {promise.Result.Anchor.gameObject.name}");
            ARCloudAnchor anchor = promise.Result.Anchor;

            // attach content to cloud anchor
            GameObject content = Instantiate(contentPrefab, anchor.transform);
            content.transform.localPosition = contentOffset;
        }
    }

}
