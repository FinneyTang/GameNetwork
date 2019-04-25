using Network.UDP;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncInputSampleClient : MonoBehaviour
{
    public Transform SimObjectTF;
    public Transform PreObjectTF;

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
        ClientSimUpdate();
        ClientPresentationUpdate();
    }
    private bool m_HasClientConnected = false;
    private Vector3 m_TargetPos = Vector3.zero;
    private Quaternion m_TargetRot = Quaternion.identity;
    private Vector3 m_StartPos;
    private Quaternion m_StartRot;
    private float m_SimTime;
    private float m_TotalTime;
    void ClientSimUpdate()
    {
        if (m_HasClientConnected)
        {
            SendInput();
        }
        if (m_ClientSession.GetRecvedData(m_ClientRecvedData))
        {
            int frameCount = 0;
            while (m_ClientRecvedData.Count != 0)
            {
                InputMsg msg = new InputMsg();
                msg.Unserialize(m_ClientRecvedData.Dequeue());
                UpdateByServer(msg);
                frameCount++;
            }
            SimObjectTF.position = m_SimPos;
            SimObjectTF.rotation = m_SimRot;

            m_TargetPos = m_SimPos;
            m_TargetRot = m_SimRot;
            m_StartPos = PreObjectTF.position;
            m_StartRot = PreObjectTF.rotation;
            m_TotalTime = Time.fixedDeltaTime * frameCount;
            m_SimTime = 0;
        }
    }
    public void ClientPresentationUpdate()
    {
        if (m_TotalTime < Mathf.Epsilon)
        {
            return;
        }
        m_SimTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(m_SimTime / m_TotalTime);
        PreObjectTF.position = Vector3.Lerp(m_StartPos, m_TargetPos, ratio);
        PreObjectTF.rotation = Quaternion.Slerp(m_StartRot, m_TargetRot, ratio);
    }
    void SendInput()
    {
        InputMsg msg = new InputMsg();
        if (Input.GetKey(KeyCode.A))
        {
            msg.Dir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            msg.Dir.x = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            msg.Dir.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            msg.Dir.y = -1;
        }
        m_ClientSession.Send(msg.Serialize());
    }
    private Vector3 m_SimPos = Vector3.zero;
    private Quaternion m_SimRot = Quaternion.identity;
    void UpdateByServer(InputMsg msg)
    {
        Vector3 dir = new Vector3(msg.Dir.x, 0, msg.Dir.y);
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0;
        dir.Normalize();
        m_SimPos += 5 * Time.fixedDeltaTime * dir;
        if(dir.sqrMagnitude > Mathf.Epsilon)
        {
            m_SimRot = Quaternion.LookRotation(dir);
        }
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "Connect"))
        {
            m_ClientSession.Send(new byte[] { 0 });
            m_HasClientConnected = true;
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
