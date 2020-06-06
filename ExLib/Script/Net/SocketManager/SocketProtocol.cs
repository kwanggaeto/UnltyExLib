using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.Net
{
    public enum ClientSocketState
    {
        DISCONNECTED,
        CONNECTED,
    }

    [System.Serializable]
    public enum SocketProtocol
    {
        TCP,
        UDP,
    }
}
