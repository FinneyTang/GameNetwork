using Network.UDP;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncStateSample : MonoBehaviour
{
    public Transform ClientObjectTF;
    public Transform ServerObjectTF;

    private UDPListener m_ServerSession = new UDPListener();
    private Queue<byte[]> m_ServerRecvedData = new Queue<byte[]>();
    private UDPUser m_ClientSession = new UDPUser();
    private Queue<byte[]> m_ClientRecvedData = new Queue<byte[]>();
    void Start()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
        m_ClientSession.Send(new byte[] { 0 });
    }
    void Update()
    {
        ServerUpdate();
        ClientUpdate();
    }
    //----------------------------------------------------------
    //Msg
    class StateMsg
    {
        public Vector3 TargetPosition;
        public Quaternion TargetOrientation;
        public float TimeStamp;
        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(TargetPosition.x);
            writer.Write(TargetPosition.y);
            writer.Write(TargetPosition.z);
            writer.Write(TargetOrientation.x);
            writer.Write(TargetOrientation.y);
            writer.Write(TargetOrientation.z);
            writer.Write(TargetOrientation.w);
            writer.Write(TimeStamp);
            return stream.ToArray();
        }
        public void Unserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            TargetPosition.x = reader.ReadSingle();
            TargetPosition.y = reader.ReadSingle();
            TargetPosition.z = reader.ReadSingle();
            TargetOrientation.x = reader.ReadSingle();
            TargetOrientation.y = reader.ReadSingle();
            TargetOrientation.z = reader.ReadSingle();
            TargetOrientation.w = reader.ReadSingle();
            TimeStamp = reader.ReadSingle();
        }
    }
    //----------------------------------------------------------
    //Server logic
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

    //----------------------------------------------------------
    //Client logic
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
                if(msg.TimeStamp > m_LastTimeStamp)
                {
                    m_TargetPos = msg.TargetPosition;
                    m_TargetRot = msg.TargetOrientation;
                    m_StartPos = ClientObjectTF.position;
                    m_StartRot = ClientObjectTF.rotation;
                    m_TotalTime = (m_TotalTime - m_SimTime) + msg.TimeStamp - m_LastTimeStamp;
                    if(m_TotalTime > 1f)
                    {
                        m_TotalTime = msg.TimeStamp - m_LastTimeStamp;
                    }
                    m_SimTime = 0;
                    m_LastTimeStamp = msg.TimeStamp;
                }
            }
        }
        if(m_TotalTime < Mathf.Epsilon)
        {
            return;
        }
        m_SimTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(m_SimTime / m_TotalTime);
        ClientObjectTF.position = Vector3.Lerp(m_StartPos, m_TargetPos, ratio);
        ClientObjectTF.rotation = Quaternion.Slerp(m_StartRot, m_TargetRot, ratio);
    }
    //----------------------------------------------------------
    void OnApplicationQuit()
    {
        if (m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
