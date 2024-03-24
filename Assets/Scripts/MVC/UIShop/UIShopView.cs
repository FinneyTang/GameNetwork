using UnityEngine;
using UnityEngine.UI;

public class UIShopView : MonoBehaviour
{
    public Button BtnBuy;
    public Text Label;

    public void Refresh(int price, int wallet)
    {
        Label.color = price > wallet ? Color.red : Color.green;
        Label.text = $"{price}/{wallet}";
    }
}