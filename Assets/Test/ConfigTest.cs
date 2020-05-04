using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigTest : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(Settings.UNetSettings.Value.Address);
        Debug.Log(Settings.SocketSettings.Value.ClientSettings);
        Debug.Log(Settings.SMTPSettings.Value.Host);
    }
}
