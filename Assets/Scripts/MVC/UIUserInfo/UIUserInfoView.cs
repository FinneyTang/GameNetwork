using UnityEngine;
using UnityEngine.UI;

public class UIUserInfoView : MonoBehaviour
{
    public Text Label;
    
    public void Refresh(string userName, int wallet)
    {
        Label.text = $"{userName}: {wallet}";
    }
}