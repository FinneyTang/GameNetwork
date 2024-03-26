using Network.UDP;
using System.Collections.Generic;
using SyncState;
using UnityEngine;

public class SyncStateSampleClient : MonoBehaviour
{
    public Transform ClientObjectTF;
    public Transform ServerObjectTF;

    private UDPClient m_ClientSession = new UDPClient();
    private readonly Queue<byte[]> m_ClientRecvedData = new Queue<byte[]>();

    private class ClientObject
    {
        public Transform ObjectTF; 
        
        public Vector3 TargetPos = Vector3.zero; 
        public Quaternion TargetRot = Quaternion.identity;
        public Vector3 StartPos; 
        public Quaternion StartRot; 
        
        public float LastTimeStamp; 
        public float SimTime; 
        public float TotalTime;
    }
    private readonly Dictionary<string, ClientObject> m_Objects = new Dictionary<string, ClientObject>();

    private void Start()
    {
        Application.targetFrameRate = 60;
        
        ClientObjectTF.gameObject.SetActive(false);
        ServerObjectTF.gameObject.SetActive(false);
        
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }

    private void Update()
    {
        SendInput();
        ClientUpdate();
    }

    private void ClientUpdate()
    {
        if (m_ClientSession.GetRecvedData(m_ClientRecvedData))
        {
            while (m_ClientRecvedData.Count != 0)
            {
                var data = m_ClientRecvedData.Dequeue();
                var msg = new SyncState.StateMsg();
                msg.Unserialize(data);
                if (!m_Objects.TryGetValue(msg.ClientKey, out var obj))
                {
                    obj = new ClientObject();
                    obj.ObjectTF = Instantiate(ClientObjectTF, ClientObjectTF.parent);
                    obj.ObjectTF.gameObject.SetActive(true);
                    obj.ObjectTF.position = msg.TargetPosition;
                    obj.ObjectTF.rotation = Quaternion.LookRotation(msg.TargetForward);
                    obj.LastTimeStamp = msg.TimeStamp;
                    m_Objects[msg.ClientKey] = obj;
                    if (msg.ClientKey == m_ClientSession.ClientKey)
                    {
                        ServerObjectTF.gameObject.SetActive(true);
                    }
                }
                if (msg.TimeStamp > obj.LastTimeStamp)
                {
                    obj.TargetPos = msg.TargetPosition;
                    obj.TargetRot = Quaternion.LookRotation(msg.TargetForward);
                    obj.StartPos = obj.ObjectTF.position;
                    obj.StartRot = obj.ObjectTF.rotation;
                    obj.TotalTime = Mathf.Max(obj.TotalTime - obj.SimTime, 0f) + msg.TimeStamp - obj.LastTimeStamp;
                    if (obj.TotalTime > 1f)
                    {
                        obj.TotalTime = msg.TimeStamp - obj.LastTimeStamp;
                    }
                    obj.SimTime = 0;
                    obj.LastTimeStamp = msg.TimeStamp;

                    if (msg.ClientKey == m_ClientSession.ClientKey)
                    {
                        ServerObjectTF.position = obj.TargetPos;
                        ServerObjectTF.rotation = obj.TargetRot;   
                    }
                }
            }
        }

        foreach (var pair in m_Objects)
        {
            var obj = pair.Value;
            if (obj.TotalTime < Mathf.Epsilon)
            {
                continue;
            }
            obj.SimTime += Time.deltaTime;
            var ratio = Mathf.Clamp01(obj.SimTime / obj.TotalTime);
            obj.ObjectTF.position = Vector3.Lerp(obj.StartPos, obj.TargetPos, ratio);
            obj.ObjectTF.rotation = Quaternion.Slerp(obj.StartRot, obj.TargetRot, ratio);

            //Debug.Log(Time.deltaTime + ", " + obj.SimTime + ", " + obj.TotalTime + ", " + obj.LastTimeStamp + ", " + ratio);
        }
    }

    private void SendInput()
    {
        var msg = new SyncState.InputMsg();
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
            msg.Dir.z = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            msg.Dir.z = -1;
        }
        msg.Dir = Camera.main.transform.TransformDirection(msg.Dir);
        msg.Dir.y = 0;
        msg.Dir.Normalize();
        m_ClientSession.Send(msg.Serialize());
    }

    private void OnApplicationQuit()
    {
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
