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
        const string imgURL =
            "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/60593745-10ed-4768-a257-37f9bfbaa024/dfd12iu-c57e0d16-9624-4ee1-8d78-4e54f1bad35e.png/v1/fit/w_512,h_512,q_70,strp/seamless_brick_wall_texture_512_512px_by_thomasbaijot_dfd12iu-375w-2x.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9NTEyIiwicGF0aCI6IlwvZlwvNjA1OTM3NDUtMTBlZC00NzY4LWEyNTctMzdmOWJmYmFhMDI0XC9kZmQxMml1LWM1N2UwZDE2LTk2MjQtNGVlMS04ZDc4LTRlNTRmMWJhZDM1ZS5wbmciLCJ3aWR0aCI6Ijw9NTEyIn1dXSwiYXVkIjpbInVybjpzZXJ2aWNlOmltYWdlLm9wZXJhdGlvbnMiXX0.66m616YqWjL7SLJf37uicBqFc1PGcxzbWIYu8WS21cc";
        m_Request.Get(imgURL, "", (result, data) =>
        {
            if(result == true)
            {
                var tex = new Texture2D(1, 1);
                if(tex.LoadImage(data))
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

    private void Update ()
    {
        if(m_Request != null)
        {
            m_Request.CheckPendingRequest();
        }
    }

    private void OnGUI()
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
