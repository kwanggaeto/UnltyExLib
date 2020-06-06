using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class OnceTrigger
{
    private bool _activate = true;
    private static OnceTrigger _instance;
    private static OnceTrigger instance { get { if (_instance == null) new OnceTrigger(); return _instance; } }

    public OnceTrigger() { _instance = this; }

    public void Fire(UnityAction action)
    {
        if (!_activate)
            return;
        _activate = false;
        action.Invoke();
    }

    public void Fire<T>(UnityAction<T> action, T param)
    {
        if (!_activate)
            return;
        
        _activate = false;
        action.Invoke(param);
    }

    public void Fire<T1, T2>(UnityAction<T1, T2> action, T1 param1, T2 param2)
    {
        if (!_activate)
            return;
        _activate = false;
        action.Invoke(param1, param2);
    }

    public void Fire<T1, T2, T3>(UnityAction<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        if (!_activate)
            return;
        _activate = false;
        action.Invoke(param1, param2, param3);
    }

    public void Refresh()
    {
        _activate = true;
    }


    public static void Trigger(UnityAction action)
    {
        instance.Fire(action);
    }

    public static void Trigger<T>(UnityAction<T> action, T param)
    {
        instance.Fire(action, param);
    }

    public static void Trigger<T1, T2>(UnityAction<T1, T2> action, T1 param1, T2 param2)
    {
        instance.Fire(action, param1, param2);
    }

    public static void Trigger<T1, T2, T3>(UnityAction<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        instance.Fire(action, param1, param2, param3);
    }

    public static void Reset()
    {
        instance.Refresh();
    }
}
