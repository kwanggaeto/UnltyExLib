using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.UI
{
    public abstract class ThrobberBase :MonoBehaviour
    {
        public bool IsGenerated { get; protected set; }
        public abstract void Play();
        public abstract void Stop();
    }
}
