using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ExLib.Native.Android
{
    public class Dialog
    {
        public delegate void DialogAction(bool ok);

        private AndroidJavaClass _unityPlayer;
        private AndroidJavaObject _activity;

        public event DialogAction onResult;

        public Dialog()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
#endif
        }

        ~Dialog()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_unityPlayer != null)
                _unityPlayer.Dispose();

            _unityPlayer = null;
#endif
        }

        private class PositiveButtonListner : AndroidJavaProxy
        {
            private Dialog dialog;

            public PositiveButtonListner(Dialog d)
             : base("android.content.DialogInterface$OnClickListener")
            {
                dialog = d;
            }

            public void onClick(AndroidJavaObject obj, int value)
            {
                if (dialog.onResult != null)
                    dialog.onResult.Invoke(true);
            }
        }


        private class NegativeButtonListner : AndroidJavaProxy
        {
            private Dialog dialog;

            public NegativeButtonListner(Dialog d)
            : base("android.content.DialogInterface$OnClickListener")
            {
                dialog = d;
            }

            public void onClick(AndroidJavaObject obj, int value)
            {
                if (dialog.onResult != null)
                    dialog.onResult.Invoke(false);
            }
        }

        public void Show(string message, bool cancelable, string okLabel="YES", string cancelLabel="NO")
        {
#if UNITY_ANDROID
#if UNITY_EDITOR
            if (!cancelable)
            {
                WindowsAPI.Dialog.ReturnValue res = WindowsAPI.Dialog.Show("Operation Dialog", message, ExLib.Native.WindowsAPI.Dialog.Flags.MB_OK);
                if (onResult != null)
                    onResult.Invoke(true);
            }
            else
            {
                WindowsAPI.Dialog.ReturnValue res = WindowsAPI.Dialog.Show("Operation Dialog", message, ExLib.Native.WindowsAPI.Dialog.Flags.MB_YESNO);
                if (res == WindowsAPI.Dialog.ReturnValue.IDOK || res == WindowsAPI.Dialog.ReturnValue.IDYES)
                {
                    if (onResult != null)
                        onResult.Invoke(true);
                }
                else if (res == WindowsAPI.Dialog.ReturnValue.IDCANCEL || res == WindowsAPI.Dialog.ReturnValue.IDNO)
                {
                    if (onResult != null)
                        onResult.Invoke(false);
                }
            }
#else
            _activity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                AndroidJavaObject alertDialogBuilder = new AndroidJavaObject("android/app/AlertDialog$Builder", _activity);

                alertDialogBuilder.Call<AndroidJavaObject>("setMessage", message);
                alertDialogBuilder.Call<AndroidJavaObject>("setCancelable", cancelable);
                alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", okLabel);
                if (cancelable)
                {
                    alertDialogBuilder.Call<AndroidJavaObject>("setNegativeButton", cancelable);
                }
            }));
#endif
#endif
        }
    }
}
