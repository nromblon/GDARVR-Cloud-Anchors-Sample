using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerManager : MonoBehaviour
{
    private static WebServerManager instance;
    public static WebServerManager Instance
    {
        get { 
            return instance; 
        }
    }

    public const String SERVER_URL = "https://gdarvr-cloud-anchor-web-server.onrender.com";
    public static String groupName = "Test Group";
    public static String password = "passwordtest";

    public List<String> cloudAnchors;
    public bool isRequesting = false;

    private void Awake()
    {
        instance = this;
        cloudAnchors = new List<String>();
    }

    public void UploadHostedAnchor(string id)
    {
        // Load request body payload
        PostAnchorRequest requestBody = new PostAnchorRequest();
        requestBody.groupName = groupName;
        requestBody.password = password;
        requestBody.anchorId = id;

        StartCoroutine(UploadAnchor(requestBody));
    }

    public void RetrieveHostedAnchors()
    {
        isRequesting = true;
        StartCoroutine(RetrieveAnchors());
    }

    private IEnumerator UploadAnchor(PostAnchorRequest requestBody)
    {
        // Send POST request
        Debug.Log("Hosting anchor. Request Body:");
        Debug.Log(JsonUtility.ToJson(requestBody)); 
        using (UnityWebRequest www = new UnityWebRequest(SERVER_URL + "/anchors", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody));
            www.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("anchor upload complete!");
            }
        }
    }

    private IEnumerator RetrieveAnchors()
    {
        cloudAnchors.Clear();
        Debug.Log("Retrieving anchors...");
        // Send GET request
        // GET requests cannot have a payload, so parameters are passed as part of the "query parameters", which is added after the `?` character
        using (UnityWebRequest www = UnityWebRequest.Get(SERVER_URL + $"/anchors?groupName={Uri.EscapeUriString(groupName)}"))
        {
            yield return www.SendWebRequest();

            Debug.Log(www.result);
            switch (www.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + www.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + www.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Received: " + www.downloadHandler.text);

                    RetrieveAnchorResult res = JsonUtility.FromJson<RetrieveAnchorResult>(www.downloadHandler.text);
                    Debug.Log("res:");
                    Debug.Log(res);
                    Debug.Log(res.anchorIds.Length);
                    cloudAnchors.AddRange(res.anchorIds);
                    break;
            }
        }

        isRequesting = false;
    }
}

[System.Serializable]
public class PostAnchorRequest
{
    public string groupName;
    public string password;
    public string anchorId;
}

[System.Serializable]
public class RetrieveAnchorResult
{
    public string[] anchorIds;
}