using Network.Core;
using Network.HTTP;
using UnityEngine;

public class HTTPImageSample : MonoBehaviour
{
    public MeshRenderer Renderer;

    private HTTPRequest m_Request;
    private bool m_IsLoaded = false;
    void Start ()
    {
        m_Request = new HTTPRequest();
        m_Request.Get("http://m.ps123.net/sucai/UploadFiles/201103/2011032500295577.jpg", "", (result, data) =>
        {
            if(result == true)
            {
                Texture2D tex = new Texture2D(512, 512);
                if(ImageConversion.LoadImage(tex, data))
                {
                    Renderer.material.mainTexture = tex;
                }
                m_IsLoaded = true;
            }
            else
            {
                ColoredLogger.Log("failed load image: " + m_Request.LastError, ColoredLogger.LogColor.Red);
            }
            m_Request = null;
        });
    }
	void Update ()
    {
        if(m_Request != null)
        {
            m_Request.CheckPendingRequest();
        }
    }
    void OnGUI()
    {
        float progress = m_IsLoaded ? 100 : 0;
        if(m_Request != null)
        {
            progress = m_Request.Progress * 100;
        }
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        GUI.Label(new Rect(margin, margin, Screen.width - 2 * margin, 20), progress.ToString("f1") + "%");
    }
}
