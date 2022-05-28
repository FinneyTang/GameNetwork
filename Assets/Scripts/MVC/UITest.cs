using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    public Button Button;
    public Text Label;

    private int m_Value = 0;

    private void Start()
    {
        Label.text = string.Empty;
        Button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        m_Value++;
        Label.text = m_Value.ToString();
    }
}
