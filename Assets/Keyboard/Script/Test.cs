using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGD.GuestBook
{
    public class Test : MonoBehaviour
    {
        ExLib.Native.WindowsAPI.KeyboardEventHooker _keyHooker;
        private void Awake()
        {
            _keyHooker = new ExLib.Native.WindowsAPI.KeyboardEventHooker(false);
        }

        private void OnEnable()
        {
            _keyHooker.Hook();
        }

        private void OnDisable()
        {
            _keyHooker.UnHook();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                        Debug.Log("KeyCode down: " + kcode);
                }
            }
        }
    }
}