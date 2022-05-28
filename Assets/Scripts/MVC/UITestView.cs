using UnityEngine;
using UnityEngine.UI;

public class UITestView : MonoBehaviour
{
    public Button Button;
    public Text Label;

    public void Refresh(int v)
    {
        Label.text = v.ToString();
    }
}