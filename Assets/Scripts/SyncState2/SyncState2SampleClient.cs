using System.Collections.Generic;
using Network.UDP;
using SyncState2;
using UnityEngine;

public class SyncState2SampleClient : MonoBehaviour
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
    private readonly Dictionary<string, ClientObject> m_OtherObjects = new Dictionary<string, ClientObject>();

    private void Start()
    {
        Application.targetFrameRate = 60;
        
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }

    private void Update()
    {
        ClientUpdateLocal();
        ClientUpdateOthers();
    }
    
    private float m_NextUploadTime;

    private void ClientUpdateLocal()
    {
        //update simulation
        var dir = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            dir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            dir.x = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            dir.z = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dir.z = -1;
        }
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0;
        dir.Normalize();
        var pos = ClientObjectTF.position;
        pos += 6 * Time.deltaTime * dir;
        ClientObjectTF.position = pos;
        if (dir.sqrMagnitude > Mathf.Epsilon)
        {
            ClientObjectTF.rotation = Quaternion.LookRotation(dir);
        }
        //send state msg
        if (Time.time > m_NextUploadTime)
        {
            var msg = new UploadStateMsg
            {
                TargetPosition = ClientObjectTF.position,
                TargetForward = ClientObjectTF.forward,
                TimeStamp = Time.time
            };
            m_ClientSession.Send(msg.Serialize());
            m_NextUploadTime = Time.time + 0.1f;
        }
    }

    private void ClientUpdateOthers()
    {
        if (m_ClientSession.GetRecvedData(m_ClientRecvedData))
        {
            while (m_ClientRecvedData.Count != 0)
            {
                var data = m_ClientRecvedData.Dequeue();
                var msg = new StateMsg();
                msg.Unserialize(data);
                if (msg.ClientKey == m_ClientSession.ClientKey)
                {
                    //show server pos for debug only
                    ServerObjectTF.position = msg.TargetPosition;
                    ServerObjectTF.rotation = Quaternion.LookRotation(msg.TargetForward);
                    continue; //skip local player
                }
                if (!m_OtherObjects.TryGetValue(msg.ClientKey, out var obj))
                {
                    obj = new ClientObject();
                    obj.ObjectTF = Instantiate(ClientObjectTF, ClientObjectTF.parent);
                    obj.ObjectTF.gameObject.SetActive(true);
                    obj.ObjectTF.position = msg.TargetPosition;
                    obj.ObjectTF.rotation = Quaternion.LookRotation(msg.TargetForward);
                    obj.LastTimeStamp = msg.TimeStamp;
                    m_OtherObjects[msg.ClientKey] = obj;
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
                }
            }
        }

        foreach (var pair in m_OtherObjects)
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
        }
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
