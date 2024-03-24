using UnityEngine;

public class UIUserInfoController : MonoBehaviour
{
    private UIUserInfoView m_View;
    private void Start()
    {
        m_View = GetComponent<UIUserInfoView>();
        var userModel = UIUserModel.instance;
        m_View.Refresh(userModel.Name, userModel.Coin);
        userModel.AddObserver(OnCoinChanged);
    }
    private void OnDestroy()
    {
        UIUserModel.instance.RemoveObserver(OnCoinChanged);
    }
    private void OnCoinChanged(UIUserModel.EPropChangeType type)
    {
        if (type == UIUserModel.EPropChangeType.Coin)
        {
            var userModel = UIUserModel.instance;
            m_View.Refresh(userModel.Name, userModel.Coin);
        }
    }
}