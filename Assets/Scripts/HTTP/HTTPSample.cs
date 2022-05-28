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
        TestGet();
        TestPost();
        TestGetTimeout();
    }
    private void Update()
    {
        for(int i = m_PendingRequests.Count - 1; i >= 0; i--)
        {
            m_PendingRequests[i].CheckPendingRequest();
        }
    }
    private void TestGet()
    {
        //http://httpbin.org/get?name=Hello&age=1
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
        }, "name", "Hello", "age", 1);
        //add to pending list
        m_PendingRequests.Add(request);
    }
    class Person
    {
        public string name;
        public int age;
    }
    private void TestPost()
    {
        var jsonData = new Person()
        {
            name = "Hello",
            age = 1,
        };
        Dictionary<string, string> header = new Dictionary<string, string>();
        header["Content-Type"] = "application/json";
        var postData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonData));

        var request = new HTTPRequest();
        request.Post("http://httpbin.org/", "post", (result, data) =>
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
        }, postData, header);
        //add to pending list
        m_PendingRequests.Add(request);
    }
    private void TestGetTimeout()
    {
        var request = new HTTPRequest();
        request.Get("http://111.111.111.111/", "get", (result, data) =>
        {
            if (result == false)
            {
                ColoredLogger.Log("Request failed: " + request.LastError, ColoredLogger.LogColor.Red);
            }
            m_PendingRequests.Remove(request);
        });
        //add to pending list
        m_PendingRequests.Add(request);
    }
}
