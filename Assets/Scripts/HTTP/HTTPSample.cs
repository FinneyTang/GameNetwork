using Network.Core;
using Network.HTTP;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class HTTPSample : MonoBehaviour
{
    private List<HTTPRequest> m_PendingRequests = new List<HTTPRequest>();
    private void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            TestGet(i);
        }
        TestPost();
    }
    private void Update()
    {
        for(int i = m_PendingRequests.Count - 1; i >= 0; i--)
        {
            m_PendingRequests[i].CheckPendingRequest();
        }
    }
    private void TestGet(int index)
    {
        var request = new HTTPRequest();
        request.Get("http://httpbin.org/", "get", (result, data) =>
        {
            if (result == true)
            {
                ColoredLogger.Log(Encoding.ASCII.GetString(data), ColoredLogger.LogColor.Green);
            }
            else
            {
                ColoredLogger.Log("Request failed: " + request.LastError, ColoredLogger.LogColor.Red);
            }
            m_PendingRequests.Remove(request);
        }, "name", "Reqeust" + index, "index", index);
        //add to pending list
        m_PendingRequests.Add(request);
    }
    private void TestPost()
    {

    }
}
