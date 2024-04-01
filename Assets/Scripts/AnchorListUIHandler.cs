using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnchorListUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject viewport;
    [SerializeField] private GameObject anchorUIItemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRequestAnchorsPressed()
    {
        WebServerManager.Instance.RetrieveHostedAnchors();
        StartCoroutine(WaitUntilAnchorsReceived());
    }

    IEnumerator WaitUntilAnchorsReceived()
    {
        yield return new WaitUntil(() => !WebServerManager.Instance.isRequesting);
        Debug.Log("anchors received! ");
        // Destroys all children objects
        foreach (Transform transform in viewport.transform)
        {
            Destroy(transform.gameObject);
        }
        // Repopulate
        foreach (string cloudanchorID in WebServerManager.Instance.cloudAnchors)
        {
            GameObject go = Instantiate(anchorUIItemPrefab);
            go.name = cloudanchorID;
            go.transform.parent = viewport.transform;
            ResolveAnchorButton btn = go.GetComponent<ResolveAnchorButton>();
            btn.SetAnchorId(cloudanchorID);
        }
        Debug.Log("Done");
    }
}
