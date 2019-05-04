using Network.UDP;
using UnityEngine;

public class SyncState2SampleClient : MonoBehaviour
{
    public Transform SimObjectTF;

    private UDPClient m_ClientSession = new UDPClient();
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
    }
    private bool m_HasClientConnected = false;
    private float m_NextUploadTime;
    void ClientSimUpdate()
    {
        if (m_HasClientConnected)
        {
            //update simulation
            Vector3 dir = Vector3.zero;
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
            Vector3 pos = SimObjectTF.position;
            pos += 6 * Time.deltaTime * dir;
            SimObjectTF.position = pos;
            if (dir.sqrMagnitude > Mathf.Epsilon)
            {
                SimObjectTF.rotation = Quaternion.LookRotation(dir);
            }
            //send state msg
            if (Time.time > m_NextUploadTime)
            {
                UploadStateMsg msg = new UploadStateMsg();
                msg.TargetPosition = SimObjectTF.position;
                msg.TargetOrientation = SimObjectTF.rotation;
                msg.TimeStamp = Time.time;

                m_ClientSession.Send(msg.Serialize());
                m_NextUploadTime = Time.time + 0.1f;
            }
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
