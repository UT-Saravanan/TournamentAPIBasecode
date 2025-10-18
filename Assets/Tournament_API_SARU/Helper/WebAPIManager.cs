using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// WebApiManager is a singleton class that manages network requests to the server.
/// </summary>
public class WebApiManager : MonoBehaviour
{
    #region SINGLETON
    public static WebApiManager Instance;
    #endregion

    #region CALLBACKS

    /// <summary>
    /// The call back after every request is made
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    /// <param name="body"></param>
    public delegate void ReqCallback(bool isSuccess, string error, string body);

    #endregion

    #region UNITY_FUNCTIONS
    /// <summary>
    ///  Runs on Awake of this Application
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    #endregion

    #region NETWORK

    /// <summary>
    ///  All Post Network Calls Made here
    /// </summary>
    /// <param name="callType"></param>
    /// <param name="uri"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    /// <param name="timeout"></param>
    public void GetNetWorkCallPostMethodUsingJson(string uri, string bodyJsonString, ReqCallback callback, int timeout = 5, bool check = false)
    {
      
        GetNetWorkCall(NetworkCallType.POST_METHOD_USING_JSONDATA, uri, bodyJsonString, null, callback, timeout, check);
    }

    public void GetNetWorkCall(NetworkCallType callType, string uri, List<KeyValuePojo> parameters, ReqCallback callback, int timeout = 5, bool check = false)
    {
        string bodyJsonString = string.Empty;
        if (callType == NetworkCallType.POST_METHOD_USING_JSONDATA)
            bodyJsonString = getEncodedParams(parameters);
        GetNetWorkCall(callType, uri, bodyJsonString, parameters, callback, timeout, check);
    }

    /// <summary>
    ///  Type of Network call done here
    /// </summary>
    /// <param name="callType"></param>
    /// <param name="uri"></param>
    /// <param name="bodyJsonString"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    /// <param name="timeout"></param>
    private void GetNetWorkCall(NetworkCallType callType, string uri, string bodyJsonString, List<KeyValuePojo> parameters, ReqCallback callback, int timeout = 5, bool check = false)
    {
        switch (callType)
        {
            case NetworkCallType.GET_METHOD:
                StartCoroutine(RequestGetMethod(uri, parameters, callback, timeout));
                break;
            case NetworkCallType.POST_METHOD_USING_FORMDATA:
                StartCoroutine(PostRequestUsingForm(uri, parameters, callback, timeout));
                break;
            case NetworkCallType.POST_METHOD_USING_JSONDATA:
                StartCoroutine(PostRequestUsingJson(uri, bodyJsonString, callback, timeout, check));
                break;
        }
    }

    /// <summary>
    /// Get method Request function
    /// </summary>
    /// <param name="url"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    private IEnumerator RequestGetMethod(string url, List<KeyValuePojo> parameters, ReqCallback callback, int timeout = 5)
    {
        yield return null;
        if (!parameters.Exists(x => x.keyId == "DateTime"))
            parameters.Add(new KeyValuePojo { keyId = "DateTime", value = "Date___" + DateTime.UtcNow });
        string getParameters = getEncodedParams(parameters);
        Debug.LogWarning(url + getParameters);
#if !UNITY_WEBGL || UNITY_EDITOR
        using (UnityWebRequest www = UnityWebRequest.Get(url + getParameters))
        {
            www.timeout = timeout;
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            while (!www.isDone)
                yield return www;
            callback(www.result == UnityWebRequest.Result.Success, www.error, www.downloadHandler.text);
            yield break;
        }

#endif

    }

    /// <summary>
    /// Post method Request function using FORM
    /// </summary>
    /// <param name="url"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    private IEnumerator PostRequestUsingForm(string url, List<KeyValuePojo> parameters, ReqCallback callback, int timeout = 5)
    {
        if (!parameters.Exists(x => x.keyId == "DateTime"))
            parameters.Add(new KeyValuePojo { keyId = "DateTime", value = "Date___" + DateTime.UtcNow });
        WWWForm bodyFormData = new WWWForm();
        foreach (KeyValuePojo items in parameters)
        {
            bodyFormData.AddField(items.keyId, items.value);
            Debug.LogWarning(items.keyId + "::" + items.value);
        }

        using (UnityWebRequest www = UnityWebRequest.Post(url, bodyFormData))
        {
            www.timeout = timeout;
            yield return www.SendWebRequest();


            while (!www.isDone)
                yield return www;

            callback(www.result == UnityWebRequest.Result.Success, www.error, www.downloadHandler.text);
        }
    }

    /// <summary>
    /// Post method Request function using Json
    /// </summary>
    /// <param name="url"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    /// <param name="timeout"></param14>
    /// <returns></returns>
    private IEnumerator PostRequestUsingJson(string url, string body, ReqCallback callback, int timeout = 5, bool check = false)
    {
        string jsonData = body;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = timeout;

            yield return request.SendWebRequest();
            Debug.Log($"<color=yellow>API Request Response ::: {jsonData}\n{url}</color>\nresponse is : {request.downloadHandler.text}");

            callback(request.result == UnityWebRequest.Result.Success, request.error, request.downloadHandler.text);

        } // Automatically disposes of the request here

    }

    /// <summary>
    /// Get Encoded value
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public string getEncodedParams(List<KeyValuePojo> parameters)
    {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePojo items in parameters)
        {
            string value = UnityWebRequest.EscapeURL(items.value);

            if (sb.Length > 0)
            {
                sb.Append("&");
            }
            sb.Append(items.keyId + "=" + value);
        }
        if (sb.Length > 0)
        {
            sb.Insert(0, "?");
        }
        return sb.ToString();
    }

    #endregion
}
public enum NetworkCallType
{
    GET_METHOD,
    POST_METHOD_USING_JSONDATA,
    POST_METHOD_USING_FORMDATA
}

[Serializable]
public class KeyValuePojo
{
    public string keyId;
    public string value;

    public KeyValuePojo() { }

    public KeyValuePojo(string keyId, string value)
    {
        this.keyId = keyId;
        this.value = value;
    }
}
