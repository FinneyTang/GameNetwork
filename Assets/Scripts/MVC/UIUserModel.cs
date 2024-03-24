using System;
using System.Collections.Generic;
using Network.Core;

public class UIUserModel
{
    private static UIUserModel m_Inst;

    public static UIUserModel instance
    {
        get
        {
            if(m_Inst == null)
            {
                m_Inst = new UIUserModel();
            }
            return m_Inst;   
        }
    }

    public enum EPropChangeType
    {
        Coin,
    }

    private readonly List<Action<EPropChangeType>> m_Observers = new List<Action<EPropChangeType>>();

    public void AddObserver(Action<EPropChangeType> cb)
    {
        m_Observers.Add(cb);
    }

    public void RemoveObserver(Action<EPropChangeType> cb)
    {
        m_Observers.Remove(cb);
    }
        
    private void Notify(EPropChangeType type)
    {
        foreach (var ob in m_Observers)
        {
            ob.Invoke(type);
        }
    }

    public string Name { get; private set; } = "Finney";
    public int Coin { get; private set; } = 100;

    public void RequestBuyItem(int price)
    {
        if (Coin < price)
        {
            return;
        }
        ColoredLogger.Log("Waiting for RequestBuyItem from server");
        ServiceManager.instance.SendRequest(ERequestType.Buy, price, (result) =>
        {
            ColoredLogger.Log($"RequestBuyItem result: {result}");
            if (result == false && Coin < price)
            {
                return;
            }
            //simulate buying item, in real project, you should update the Coin from server
            //and get coin from server response
            Coin -= price;
            Notify(EPropChangeType.Coin);
        });
    }
}