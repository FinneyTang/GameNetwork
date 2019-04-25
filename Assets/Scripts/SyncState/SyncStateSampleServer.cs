using Network.UDP;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncStateSampleServer : MonoBehaviour
{
    public Transform ServerObjectTF;

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
        ServerUpdate();
    }
    private Vector3 m_ServerTargetPos;
    private bool m_HasClientConnected = false;
    private Vector3 m_ServerPos = Vector3.zero;
    private Quaternion m_ServerRot = Quaternion.identity;
    private float m_NextSyncTime;
    void ServerUpdate()
    {
        if(m_HasClientConnected == false)
        {
            if (m_ServerSession.GetRecvedData(m_ServerRecvedData))
            {
                while (m_ServerRecvedData.Count != 0)
                {
                    m_HasClientConnected = true;
                    m_ServerRecvedData.Dequeue();
                }   
            }
        }
        else
        {
            Vector3 toTarget = m_ServerTargetPos - m_ServerPos;
            if (toTarget.sqrMagnitude < 0.5f)
            {
                m_ServerTargetPos = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            }
            else
            {
                toTarget.Normalize();
                m_ServerPos += 6 * Time.deltaTime * toTarget;
                m_ServerRot = Quaternion.LookRotation(toTarget);
            }
            if (Time.time > m_NextSyncTime)
            {
                StateMsg msg = new StateMsg();
                msg.TargetPosition = m_ServerPos;
                msg.TargetOrientation = m_ServerRot;
                msg.TimeStamp = Time.time;

                m_ServerSession.Send(msg.Serialize());
                m_NextSyncTime = Time.time + 0.2f;
            }
            ServerObjectTF.position = m_ServerPos;
            ServerObjectTF.rotation = m_ServerRot;
        }
    }
    //----------------------------------------------------------
    void OnApplicationQuit()
    {
        if (m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
    }
}
