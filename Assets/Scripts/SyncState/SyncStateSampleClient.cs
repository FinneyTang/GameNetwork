using Network.UDP;
using System.Collections.Generic;
using UnityEngine;

public class SyncStateSampleClient : MonoBehaviour
{
    public Transform ClientObjectTF;

    private UDPClient m_ClientSession = new UDPClient();
    private Queue<byte[]> m_ClientRecvedData = new Queue<byte[]>();
    void Start()
    {
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }
    void Update()
    {
        ClientUpdate();
    }
    private Vector3 m_TargetPos = Vector3.zero;
    private Quaternion m_TargetRot = Quaternion.identity;
    private float m_LastTimeStamp;
    private Vector3 m_StartPos;
    private Quaternion m_StartRot;
    private float m_SimTime;
    private float m_TotalTime;
    void ClientUpdate()
    {
        if (m_ClientSession.GetRecvedData(m_ClientRecvedData))
        {
            while (m_ClientRecvedData.Count != 0)
            {
                var data = m_ClientRecvedData.Dequeue();
                StateMsg msg = new StateMsg();
                msg.Unserialize(data);
                if (msg.TimeStamp > m_LastTimeStamp)
                {
                    m_TargetPos = msg.TargetPosition;
                    m_TargetRot = msg.TargetOrientation;
                    m_StartPos = ClientObjectTF.position;
                    m_StartRot = ClientObjectTF.rotation;
                    m_TotalTime = (m_TotalTime - m_SimTime) + msg.TimeStamp - m_LastTimeStamp;
                    if (m_TotalTime > 1f)
                    {
                        m_TotalTime = msg.TimeStamp - m_LastTimeStamp;
                    }
                    m_SimTime = 0;
                    m_LastTimeStamp = msg.TimeStamp;
                }
            }
        }
        if (m_TotalTime < Mathf.Epsilon)
        {
            return;
        }
        m_SimTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(m_SimTime / m_TotalTime);
        ClientObjectTF.position = Vector3.Lerp(m_StartPos, m_TargetPos, ratio);
        ClientObjectTF.rotation = Quaternion.Slerp(m_StartRot, m_TargetRot, ratio);
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "Connect"))
        {
            m_ClientSession.Send(new byte[] { 0 });
        }
    }
    void OnApplicationQuit()
    {
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
