using UnityEngine;

public class UITestController : MonoBehaviour
{
    private UITestView m_View;
    private void Start()
    {
        m_View = GetComponent<UITestView>();
        m_View.Label.text = string.Empty;
        m_View.Button.onClick.AddListener(OnClick);
        UITestModel.Instance().AddObserver(OnValueChanged);
    }
    private void OnDestroy()
    {
        UITestModel.Instance().RemoveObserver(OnValueChanged);
    }
    private void OnValueChanged()
    {
        m_View.Refresh(UITestModel.Instance().Value);
        //m_View.Label.text = UITestModel.Instance().Value.ToString();
    }
    private void OnClick()
    {
        UITestModel.Instance().RequestIncValue();
    }
}