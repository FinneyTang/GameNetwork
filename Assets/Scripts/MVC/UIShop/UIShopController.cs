using UnityEngine;

public class UIShopController : MonoBehaviour
{
    private const int ItemPrice = 50;
    private UIShopView m_View;
    
    private void Start()
    {
        m_View = GetComponent<UIShopView>();
        m_View.Refresh(ItemPrice, UIUserModel.instance.Coin);
        m_View.BtnBuy.onClick.AddListener(OnBtnBuyClick);
        UIUserModel.instance.AddObserver(OnCoinChanged);
    }
    private void OnDestroy()
    {
        UIUserModel.instance.RemoveObserver(OnCoinChanged);
    }
    private void OnCoinChanged(UIUserModel.EPropChangeType type)
    {
        if (type == UIUserModel.EPropChangeType.Coin)
        {
            m_View.Refresh(ItemPrice,UIUserModel.instance.Coin);
        }
    }
    private void OnBtnBuyClick()
    {
        UIUserModel.instance.RequestBuyItem(ItemPrice);
    }
}