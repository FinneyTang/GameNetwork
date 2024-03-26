using Network.UDP;
using System.Collections.Generic;
using SyncInput;
using UnityEngine;

public class SyncInputSampleClient : MonoBehaviour
{
    public Transform SimObjectTF;
    public Transform PreObjectTF;

    private UDPClient m_ClientSession = new UDPClient();
    private readonly Queue<byte[]> m_ClientRecvedData = new Queue<byte[]>();
    
    private int m_CurFrameCount = 0;
    
    //object in the presentation layer
    private class PresObject
    {
        public Transform ObjectTF; 
        
        public Vector3 TargetPos = Vector3.zero; 
        public Quaternion TargetRot = Quaternion.identity;
        public Vector3 StartPos; 
        public Quaternion StartRot; 
        
        public float SimTime; 
        public float TotalTime;
    }
    private readonly Dictionary<string, PresObject> m_PresObjects = new Dictionary<string, PresObject>();
    
    //object in the logic layer
    private const int LOGIC_FRAME_TIME = 66; //15fps, ms
    private class LogicObject
    {
        public FixedVector3 LogicPos = FixedVector3.zero;
        public FixedVector3 LogicFwd = new FixedVector3(0, 0, 1000);   
    }
    private readonly Dictionary<string, LogicObject> m_LogicObjects = new Dictionary<string, LogicObject>();

    private void Start()
    {
        Application.targetFrameRate = 60;
        
        PreObjectTF.gameObject.SetActive(false);
        
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }

    private void Update()
    {
        SendInput();
        ClientLogicUpdate();
        ClientPresentationUpdate();
    }
    
    private void ClientLogicUpdate()
    {
        if (m_ClientSession.GetRecvedData(m_ClientRecvedData))
        {
            while (m_ClientRecvedData.Count != 0)
            {
                var msg = new FrameClientInputsMsg();
                msg.Unserialize(m_ClientRecvedData.Dequeue());
                if (m_CurFrameCount >= msg.FrameCount)
                {
                    continue;
                }
                m_CurFrameCount = msg.FrameCount;
                UpdateByServer(msg);
            }
            //for debug only
            if (m_LogicObjects.TryGetValue(m_ClientSession.ClientKey, out var localClient))
            {
                SimObjectTF.position = localClient.LogicPos.ToVector3();
                SimObjectTF.forward = localClient.LogicFwd.ToVector3();
            }
        }
    }

    private void ClientPresentationUpdate()
    {
        foreach (var pair in m_PresObjects)
        {
            var presObject = pair.Value;
            if (presObject.TotalTime < Mathf.Epsilon)
            {
                continue;
            }
            presObject.SimTime += Time.deltaTime;
            var ratio = Mathf.Clamp01(presObject.SimTime / presObject.TotalTime);
            presObject.ObjectTF.position = Vector3.Lerp(presObject.StartPos, presObject.TargetPos, ratio);
            presObject.ObjectTF.rotation = Quaternion.Slerp(presObject.StartRot, presObject.TargetRot, ratio);
        }
    }

    private void SendInput()
    {
        Vector3 dir = new Vector3();
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
        var inputMsg = new InputMsg();
        inputMsg.X = (int)(dir.x * 1000);
        inputMsg.Y = (int)(dir.z * 1000);
        m_ClientSession.Send(inputMsg.Serialize());
    }
    
    private void UpdateByServer(FrameClientInputsMsg msg)
    {
        foreach (var clientInput in msg.ClientInputs)
        {
            if(!m_LogicObjects.TryGetValue(clientInput.ClientKey, out var logicObject))
            {
                logicObject = new LogicObject();
                m_LogicObjects.Add(clientInput.ClientKey, logicObject);
            }
            //update logic
            var moveDir = new FixedVector3(clientInput.X, 0, clientInput.Y); //should normalize,
            var deltaPos = moveDir * (6000 * LOGIC_FRAME_TIME / 1000);
            logicObject.LogicPos += deltaPos;
            if (moveDir.x != 0 && moveDir.z != 0)
            {
                logicObject.LogicFwd = moveDir;   
            }
            //Debug.Log(logicObject.LogicPos + ", " + logicObject.LogicFwd);
            
            //notify pres object
            if(!m_PresObjects.TryGetValue(clientInput.ClientKey, out var presObject))
            {
                presObject = new PresObject();
                presObject.ObjectTF = Instantiate(PreObjectTF, PreObjectTF.parent);
                presObject.ObjectTF.gameObject.SetActive(true);
                presObject.ObjectTF.position = logicObject.LogicPos.ToVector3();
                presObject.ObjectTF.forward = logicObject.LogicFwd.ToVector3();
                m_PresObjects.Add(clientInput.ClientKey, presObject);
            }
            presObject.StartPos = presObject.ObjectTF.position;
            presObject.StartRot = presObject.ObjectTF.rotation;
            presObject.TargetPos = logicObject.LogicPos.ToVector3();
            presObject.TargetRot = Quaternion.LookRotation(logicObject.LogicFwd.ToVector3());
            presObject.TotalTime = LOGIC_FRAME_TIME / 1000f;
            presObject.SimTime = 0;
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
