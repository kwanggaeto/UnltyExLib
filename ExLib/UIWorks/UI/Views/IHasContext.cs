using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public delegate void SetContextDelegate<T>(T data);

    public interface IHasContext<T>
    {
        T ContextData { get; }

        bool HasContext();
        void SetContext(T data);
    }
}
