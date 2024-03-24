using System;
using System.Collections.Generic;
using Network.HTTP;
using UnityEngine;

public enum ERequestType
{
    Buy
}

public class ServiceManager : MonoBehaviour
{
    public static ServiceManager instance { get; private set; }

    private readonly List<HTTPRequest> m_PendingRequests = new List<HTTPRequest>();
    private void Awake()
    {
        instance = this;
    }
    public void SendRequest(ERequestType type, object value, Action<bool> callback)
    {
        var request = new HTTPRequest();
        request.Get("http://httpbin.org/", "get", (result, data) =>
        {
            callback?.Invoke(result);
            m_PendingRequests.Remove(request);
        }, "type", type, "value", value);
        m_PendingRequests.Add(request);
    }
    
    private void Update()
    {
        for(var i = m_PendingRequests.Count - 1; i >= 0; i--)
        {
            m_PendingRequests[i].CheckPendingRequest();
        }
    }
}