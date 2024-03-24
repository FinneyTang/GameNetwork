using System;
using System.Collections.Generic;

public class UITestModel
{
    private static UITestModel m_Inst;

    public static UITestModel Instance()
    {
        if(m_Inst == null)
        {
            m_Inst = new UITestModel();
        }

        return m_Inst;
    }

    private readonly List<Action> m_Observers = new List<Action>();

    public void AddObserver(Action cb)
    {
        m_Observers.Add(cb);
    }

    public void RemoveObserver(Action cb)
    {
        m_Observers.Remove(cb);
    }
        
    private void Notify()
    {
        foreach (var ob in m_Observers)
        {
            ob.Invoke();
        }
    }
    public int Value { get; private set; }

    public void RequestIncValue()
    {
        Value++;
        Notify();
    }
}