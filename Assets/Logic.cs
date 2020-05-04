using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if NET_4_6
using System.Threading.Tasks;
#endif

using ExLib.Utils;

public class Logic : MonoBehaviour
{
    [SerializeField]
    private ExLib.Control.UIKeyboard.Keyboard _keyboard;

    private Texture2D _tex;

    void Awake()
    {
        _keyboard.gameObject.SetActive(false);
    }


	// Use this for initialization
	IEnumerator Start ()
    {
        yield return new WaitForSeconds(3f);
#if NET_4_6
        Test();
#endif


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _keyboard.gameObject.SetActive(true);
        }
    }

    private void OnGUI()
    {
        /*if (ExLib.Net.SocketManager.Instance.HasServer)
        {
            if (ExLib.Net.SocketManager.Instance.Server != null)
            {
                foreach (System.Net.EndPoint rp in ExLib.Net.SocketManager.Instance.Server.Clients.Keys)
                {
                    if (GUILayout.Button("Send "+ rp.ToString(), GUILayout.Width(200), GUILayout.Height(100)))
                    {
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes("Test!!!");
                        ExLib.Net.SocketManager.Instance.ServerSend(rp, msg);
                    }
                }
            }
        }*/
    }

    private void Callback()
    {
        _tex = new Texture2D(1024, 1024);
        Debug.Log(_tex.width);
    }

#if NET_4_6
    private async void Test()
    {
        _tex = new Texture2D(1024, 1024);
        int w = _tex.width;
        int h = _tex.height;
        float start = Time.realtimeSinceStartup;
        int result = await Task<int>.Run<int>(()=>
        {
            int sum = 0;
            for(int i=0; i<w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    sum++;
                }
            }
            return sum;
        });

        Debug.Log(result);
        Debug.Log(Time.realtimeSinceStartup - start);
        Callback();
    }
#endif
}
