using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HasVisibleGUIHelper
{
    private static HashSet<string> _waitVisibleList = new HashSet<string>();
    private static HashSet<string> _visibleList = new HashSet<string>();

    public static void WaitVisible(string id)
    {
        _waitVisibleList.Add(id);
    }

    public static void CanVisible(string id)
    {
        _waitVisibleList.Remove(id);
    }

    public static bool GetVisible(string id)
    {
        if (_waitVisibleList.Contains(id))
            return false;

        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
        {
        }
        else
        {
            CanVisible(id);
        }

        return !_waitVisibleList.Contains(id);
    }
}
