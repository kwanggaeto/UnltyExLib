using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using ExLib.Net;

namespace dstrict.editor
{
    [CustomEditor(typeof(SocketManager))]
    public class SocketManagerEditor : UnityEditor.Editor
    {
        private AnimBool _hasServerBool = new AnimBool();
        private static bool _serverFold;
        private static bool _clientFold;
        private GUIStyle _infoStyle;

        private const string _localhost = "127.0.0.1";

        private const string _notFoundIPv4Msg = "No network adapters with an IPv4 address in the system!";

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            SerializedProperty script = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();

            SerializedProperty servers = serializedObject.FindProperty("_servers");
            SerializedProperty clients = serializedObject.FindProperty("_clients");


            _infoStyle = new GUIStyle(EditorStyles.miniLabel);
            _infoStyle.normal.textColor = new Color { r = .6f, g = .6f, b = .6f, a = 1f };
            SocketManager sm = target as SocketManager;
            GUIStyle laregLabelStyle = new GUIStyle(EditorStyles.foldout);
            laregLabelStyle.font = EditorStyles.boldFont;

            string basicInfo = GetLocalIPAddress();

            EditorGUILayout.LabelField("Informations", EditorStyles.boldLabel);
            if (string.IsNullOrEmpty(basicInfo))
            {
                EditorGUILayout.HelpBox(_notFoundIPv4Msg, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(basicInfo, MessageType.Info);
            }

            if (servers.arraySize > 0)
            {
                EditorGUILayout.Space();
                _serverFold = EditorGUILayout.Foldout(_serverFold, servers.arraySize + " Servers", true, laregLabelStyle);
                EditorGUI.indentLevel++;
                if (_serverFold)
                {
                    for (int i = 0; i < servers.arraySize; i++)
                    {
                        SerializedProperty server = servers.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(server, true);
                    }
                }
                EditorGUI.indentLevel--;
            }

            if (clients.arraySize > 0)
            {
                EditorGUILayout.Space();

                _clientFold = EditorGUILayout.Foldout(_clientFold, clients.arraySize + " Clients", true, laregLabelStyle);
                EditorGUI.indentLevel++;
                if (_clientFold)
                {
                    for (int i = 0; i < clients.arraySize; i++)
                    {
                        SerializedProperty client = clients.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(client, true);
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Server"))
            {


                int port = GetHighestServerPort();
                if (port >= 0)
                {
                    sm.AddServer(_localhost, port, false);
                }
            }
            if (GUILayout.Button("Add Client"))
            {
                sm.AddClient();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private int GetHighestServerPort()
        {
            SocketManager sm = target as SocketManager;

            int port = -1;
            if (sm.Servers == null || sm.Servers.Count == 0)
            {
                port = 0;
                return port;
            }

            foreach (TCPSocketServer server in sm.Servers)
            {
                if (_localhost.Equals(server.Address))
                {
                    if (server.Port >= port)
                    {
                        if (server.Port + 1 <= System.UInt16.MaxValue)
                            port = server.Port + 1;
                    }
                }
            }
            return port;
        }

        public string GetLocalIPAddress()
        {
            string hostname = Dns.GetHostName();
            string txt = string.Empty;
            var host = Dns.GetHostEntry(hostname);
            List<string> ips = new List<string>();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }

            if (ips.Count>0)
            {
                txt = string.Format("Host Name : {0}", hostname);
                for (int i=0; i<ips.Count; i++)
                {
                    txt += string.Format("\nIP : {0}", ips[i]);
                }
            }

            return ips.Count == 0 ? null : txt;
        }
    }

    [CustomPropertyDrawer(typeof(TCPSocketClient))]
    public class TCPSocketClientDrawer : PropertyDrawer
    {
        private bool _socketConnected;
        private GUIStyle _infoStyle = new GUIStyle(EditorStyles.miniLabel);

        private const string _UNIQUE_FIELD_CONTROL_NAME = "ClientUniqueId";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SocketManagerResources.Load();

            SocketManager sm = property.serializedObject.targetObject as SocketManager;

            _infoStyle.normal.textColor = new Color { r = .6f, g = .6f, b = .6f, a = 1f };
            SerializedProperty connectAtStart = property.FindPropertyRelative("_connectAtStart");
            SerializedProperty address = property.FindPropertyRelative("_address");
            SerializedProperty port = property.FindPropertyRelative("_port");
            SerializedProperty reconn = property.FindPropertyRelative("reconnectable");
            SerializedProperty reconnInterval = property.FindPropertyRelative("reconnectInterval");
            SerializedProperty uniqueId = property.FindPropertyRelative("_uniqueId");

            EditorGUI.BeginProperty(position, label, property);
            Rect rectPos = EditorGUI.IndentedRect(position);
            float w = rectPos.width;
            rectPos.width = w;
            rectPos.height = GetPropertyHeight(property, label);
            EditorGUI.DrawRect(rectPos, new Color { r=.3f, g= .3f, b= .3f, a=1f });
            rectPos.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 5f;
            EditorGUI.DrawRect(rectPos, new Color { r = .4f, g = .4f, b = .4f, a = 1f });

            int index = GetArrayIndex(property);
            TCPSocketClient target = GetTarget(property);

            _socketConnected = false;
            if (target != null)
            {
                _socketConnected = target.IsConnected;
            }

            position.x += 7.5f;
            position.y += 5f;
            Rect activationRect = EditorGUI.IndentedRect(position);
            activationRect.x -= 6f;
            activationRect.y -= 3.5f;
            activationRect.height = EditorGUIUtility.singleLineHeight + 4f;
            activationRect.width = activationRect.height;
            Texture icon = _socketConnected ? SocketManagerResources.ConnectedIcon : SocketManagerResources.DisconnectedIcon;
            EditorGUI.DrawPreviewTexture(activationRect, icon, SocketManagerResources.TransparentMat, ScaleMode.ScaleToFit, 1f);
            float startX = position.x;

            position.y -= 2f;
            position.x += activationRect.width + 5f;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Client - " + index, EditorStyles.boldLabel);

            position.y += 2f;
            Rect toggleRect = GUILayoutUtility.GetRect(new GUIContent("Connect at Start"), EditorStyles.toggle);
            position.x = w - EditorGUIUtility.labelWidth + 7.5f;
            connectAtStart.boolValue = EditorGUI.Toggle(position, "Connect at Start", connectAtStart.boolValue);
            position.x = startX;
            position.y += 5f;
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            position.width = w;
            position.height = EditorGUIUtility.singleLineHeight;


            GUI.SetNextControlName(_UNIQUE_FIELD_CONTROL_NAME);
            EditorGUI.PropertyField(position, uniqueId);
            string ctrlName = GUI.GetNameOfFocusedControl();

            if (_UNIQUE_FIELD_CONTROL_NAME.Equals(ctrlName))
            {
                if (!sm.ValidateToAdd(sm.Clients, target, uniqueId.stringValue))
                {
                    uniqueId.stringValue = TCPSocketClient.GetDefaultUID();
                }
            }

            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;

            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, address);
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            port.intValue = Mathf.Clamp(port.intValue, 1, System.UInt16.MaxValue);
            EditorGUI.PropertyField(position, port);

            position.y += 10f;
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, reconn);

            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, reconnInterval);


            if (target != null)
            {
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.y += EditorGUIUtility.singleLineHeight+6f;

                EditorGUI.LabelField(position, "Connected : " + _socketConnected, _infoStyle);

                if (_socketConnected)
                {
                    position.y += EditorGUIUtility.singleLineHeight - 3f;
                    EditorGUI.LabelField(position, "Remote Info : " + target.KernelSocket.RemoteEndPoint, _infoStyle);
                    position.y += EditorGUIUtility.singleLineHeight - 3f;
                    EditorGUI.LabelField(position, "Local Info : " + target.KernelSocket.LocalEndPoint, _infoStyle);
                }
            }
            EditorUtility.SetDirty(property.serializedObject.targetObject);

            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            position.y += 5f;
            position = EditorGUI.IndentedRect(position);
            if (EditorApplication.isPlaying)
            {
                position.width *= .333333f;
                EditorGUI.BeginDisabledGroup(_socketConnected);
                if (GUI.Button(position, "Connect"))
                {
                    target.Reconnect();
                }
                position.x += position.width;
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!_socketConnected);
                if (GUI.Button(position, "Disconnect"))
                {
                    target.Dispose();
                }
                EditorGUI.EndDisabledGroup();
                position.x += position.width;
                if (GUI.Button(position, "Remove"))
                {
                    sm.RemoveClient(index);
                }
            }
            else
            {
                if (GUI.Button(position, "Remove"))
                {
                    sm.RemoveClient(index);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TCPSocketClient target = GetTarget(property);

            bool connection = target == null ? false : target.IsConnected;

            float offsetLine = connection ? EditorApplication.isPlaying ? 2f : 1f : 0f;
            float offset = connection ? 0f: (EditorGUIUtility.standardVerticalSpacing * 2f + 6f);
            return (EditorGUIUtility.singleLineHeight+ EditorGUIUtility.standardVerticalSpacing) * (8f + offsetLine) + 20f + offset;
        }

        private int GetArrayIndex(SerializedProperty property)
        {
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(property.propertyPath, @"\[(.*?)\]");
            if (match.Groups.Count == 2)
            {
                int index = int.Parse(match.Groups[1].ToString());
                return index;
            }
            return -1;
        }

        private TCPSocketClient[] GetTargetArray(SerializedProperty property)
        {
            System.Reflection.FieldInfo fi = fieldInfo;
            System.Reflection.PropertyInfo pi = fi.ReflectedType.GetProperty("IsConnected");
            System.Reflection.PropertyInfo[] fieldPropInfos = fi.ReflectedType.GetProperties();
            TCPSocketClient[] target = null;
            foreach (System.Reflection.PropertyInfo info in fieldPropInfos)
            {
                if (info.PropertyType.IsArray && info.PropertyType.GetElementType().Equals(typeof(TCPSocketClient)))
                {
                    target = (TCPSocketClient[])info.GetValue(property.serializedObject.targetObject, null);
                    break;
                }
            }

            return target;
        }

        private TCPSocketClient GetTarget(SerializedProperty property)
        {
            TCPSocketClient[] target = GetTargetArray(property);

            if (target == null)
            {

            }
            else
            {
                int index = GetArrayIndex(property);
                if (index >= 0)
                {
                    if (index >= target.Length)
                        return null;

                    return target[index];  
                }
            }

            return null;
        }
    }

    [CustomPropertyDrawer(typeof(TCPSocketServer))]
    public class TCPSocketSeverDrawer : PropertyDrawer
    {
        private bool _socketListen;
        private GUIStyle _infoStyle = new GUIStyle(EditorStyles.miniLabel);

        private const string _UNIQUE_FIELD_CONTROL_NAME = "ServerUniqueId";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SocketManagerResources.Load();

            SocketManager sm = property.serializedObject.targetObject as SocketManager;

            _infoStyle.normal.textColor = new Color { r = .6f, g = .6f, b = .6f, a = 1f };
            SerializedProperty listenAtStart = property.FindPropertyRelative("_listenAtStart");
            SerializedProperty address = property.FindPropertyRelative("_address");
            SerializedProperty port = property.FindPropertyRelative("_port");
            SerializedProperty maxConn = property.FindPropertyRelative("_maxConnections");
            SerializedProperty uniqueId = property.FindPropertyRelative("_uniqueId");

            EditorGUI.BeginProperty(position, label, property);
            Rect rectPos = EditorGUI.IndentedRect(position);
            float w = rectPos.width;
            rectPos.width = w;
            rectPos.height = GetPropertyHeight(property, label);
            EditorGUI.DrawRect(rectPos, new Color { r = .3f, g = .3f, b = .3f, a = 1f });
            rectPos.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 5f;
            EditorGUI.DrawRect(rectPos, new Color { r = .4f, g = .4f, b = .4f, a = 1f });

            int index = GetArrayIndex(property);
            TCPSocketServer target = GetTarget(property);

            _socketListen = false;
            if (target != null)
            {
                _socketListen = target.IsListen;
            }

            position.x += 7.5f;
            position.y += 5f;

            Rect activationRect = EditorGUI.IndentedRect(position);
            activationRect.x -= 6f;
            activationRect.y -= 3.5f;
            activationRect.height = EditorGUIUtility.singleLineHeight + 4f;
            activationRect.width = activationRect.height;
            Texture icon = _socketListen ? SocketManagerResources.ConnectedIcon : SocketManagerResources.DisconnectedIcon;
            EditorGUI.DrawPreviewTexture(activationRect, icon, SocketManagerResources.TransparentMat, ScaleMode.ScaleToFit, 1f);
            float startX = position.x;

            position.y -= 2f;
            position.x += activationRect.width + 5f;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Server - " + index, EditorStyles.boldLabel);

            Rect toggleRect = GUILayoutUtility.GetRect(new GUIContent("Listen at Start"), EditorStyles.toggle);
            position.x = w - EditorGUIUtility.labelWidth + 7.5f;
            listenAtStart.boolValue = EditorGUI.Toggle(position, "Listen at Start", listenAtStart.boolValue);
            position.x = startX;
            position.y += 5f;
            position.y += 2f;
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            position.width = w;
            position.height = EditorGUIUtility.singleLineHeight;
            GUI.SetNextControlName(_UNIQUE_FIELD_CONTROL_NAME);
            EditorGUI.PropertyField(position, uniqueId);
            string ctrlName = GUI.GetNameOfFocusedControl();

            if (_UNIQUE_FIELD_CONTROL_NAME.Equals(ctrlName))
            {
                if (!sm.ValidateToAdd(sm.Servers, target, uniqueId.stringValue))
                {
                    uniqueId.stringValue = TCPSocketServer.GetDefaultUID();
                }
            }

            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;

            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, address);
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            port.intValue = Mathf.Clamp(port.intValue, 1, System.UInt16.MaxValue);
            EditorGUI.PropertyField(position, port);

            position.y += 10f;
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, maxConn);

            if (target != null)
            {
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.y += EditorGUIUtility.singleLineHeight + 6f;

                EditorGUI.LabelField(position, "Listen : " + _socketListen, _infoStyle);

                if (_socketListen && target.KernelSocket != null)
                {
                    position.y += EditorGUIUtility.singleLineHeight - 3f;
                    EditorGUI.LabelField(position, "Local Info : " + target.KernelSocket.LocalEndPoint, _infoStyle);
                }

                if (target.IsAnyClientConnected)
                {
                    position.x += 5f;
                    foreach (System.Net.EndPoint end in target.Clients.Keys)
                    {
                        position.y += EditorGUIUtility.singleLineHeight - 3f;
                        EditorGUI.LabelField(position, "- Client : " + end, _infoStyle);
                    }

                    position.x -= 5f;
                }
            }
            EditorUtility.SetDirty(property.serializedObject.targetObject);


            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.singleLineHeight;
            position.y += 5f;
            position = EditorGUI.IndentedRect(position);
            position.height = EditorGUIUtility.singleLineHeight;
            if (EditorApplication.isPlaying)
            {
                position.width *= .333333f;
                EditorGUI.BeginDisabledGroup(_socketListen);
                if (GUI.Button(position, "Bind"))
                {
                    if (target.KernelSocket == null)
                        target.Reconnect();
                    else
                        target.Connect();
                }
                position.x += position.width;
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!_socketListen);
                if (GUI.Button(position, "Close"))
                {
                    target.Dispose();
                }
                EditorGUI.EndDisabledGroup();
                position.x += position.width;
                if (GUI.Button(position, "Remove"))
                {
                    sm.RemoveServer(index);
                }
            }
            else
            {
                if (GUI.Button(position, "Remove"))
                {
                    sm.RemoveServer(index);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TCPSocketServer target = GetTarget(property);
            bool connection = target == null ? false : target.IsListen;
            float clientsOffset = (target == null ? 0f : target.Clients==null?0f:target.Clients.Count) * (EditorGUIUtility.singleLineHeight - 3f);
            float connectionOffset = connection ? EditorGUIUtility.singleLineHeight - 3f : 0f;
            float extraOffset = 15f + (EditorGUIUtility.singleLineHeight + 10f) + (EditorGUIUtility.standardVerticalSpacing * 3f);
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 6f + extraOffset + clientsOffset + connectionOffset;
        }

        private int GetArrayIndex(SerializedProperty property)
        {
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(property.propertyPath, @"\[(.*?)\]");
            if (match.Groups.Count == 2)
            {
                int index = int.Parse(match.Groups[1].ToString());
                return index;
            }
            return -1;
        }

        private TCPSocketServer[] GetTargetArray(SerializedProperty property)
        {
            System.Reflection.FieldInfo fi = base.fieldInfo;
            System.Reflection.PropertyInfo pi = fi.ReflectedType.GetProperty("IsConnected");
            System.Reflection.PropertyInfo[] fieldPropInfos = fi.ReflectedType.GetProperties();
            TCPSocketServer[] target = null;
            foreach (System.Reflection.PropertyInfo info in fieldPropInfos)
            {
                if (info.PropertyType.IsArray && info.PropertyType.GetElementType().Equals(typeof(TCPSocketServer)))
                {
                    target = (TCPSocketServer[])info.GetValue(property.serializedObject.targetObject, null);
                    break;
                }
            }

            return target;
        }

        private TCPSocketServer GetTarget(SerializedProperty property)
        {
            TCPSocketServer[] target = GetTargetArray(property);

            if (target == null)
            {

            }
            else
            {
                int index = GetArrayIndex(property);
                if (index >= 0)
                {
                    if (index >= target.Length)
                        return null;

                    return target[index];
                }
            }

            return null;
        }
    }

    public static class SocketManagerResources
    {
        public static Texture SocketManagerIcon { get; private set; }
        public static Texture ConnectedIcon { get; private set; }
        public static Texture DisconnectedIcon { get; private set; }
        public static Material TransparentMat { get; private set; }

        public static void Load()
        {
            if (SocketManagerIcon == null)
                SocketManagerIcon = Resources.Load<Texture>("Textures/network");
            if (ConnectedIcon == null)
                ConnectedIcon = Resources.Load<Texture>("Textures/connected");
            if (DisconnectedIcon == null)
                DisconnectedIcon = Resources.Load<Texture>("Textures/disconnected");

            if (TransparentMat == null)
            {
                TransparentMat = new Material(Shader.Find("Unlit/Transparent"));
            }
        }
    }
}
