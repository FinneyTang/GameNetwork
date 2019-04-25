using Network.UDP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingSampleServer : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private Queue<byte[]> m_ServerRecvedData = new Queue<byte[]>();
    void Start()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
    }
    void Update()
    {
        if (m_ServerSession.GetRecvedData(m_ServerRecvedData))
        {
            while (m_ServerRecvedData.Count != 0)
            {
                var data = m_ServerRecvedData.Dequeue();
                StartCoroutine(DelaySend(data, UnityEngine.Random.Range(0.1f, 0.3f)));
            }
        }
    }
    IEnumerator DelaySend(byte[] data, float delay)
    {
        yield return new WaitForSeconds(delay);
        m_ServerSession.Send(data);
    }
    void OnApplicationQuit()
    {
        if (m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
    }
}
