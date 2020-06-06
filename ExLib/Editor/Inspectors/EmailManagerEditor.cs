using UnityEngine;
using UnityEditor;
using System.Collections;
using ExLib.Net;

[CanEditMultipleObjects]
[CustomEditor(typeof(EmailManager))]
public class EmailManagerEditor : Editor {
    

    public override void OnInspectorGUI()
    {        
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.BeginVertical();
        
        EmailManager em = (EmailManager)target;

        SerializedProperty host = serializedObject.FindProperty("_smtpHost");
        SerializedProperty port = serializedObject.FindProperty("_smtpPort");
        SerializedProperty ssl = serializedObject.FindProperty("_SSL");
        SerializedProperty timeout = serializedObject.FindProperty("_timeout");
        SerializedProperty sender = serializedObject.FindProperty("_sender");
        SerializedProperty pw = serializedObject.FindProperty("_senderPassword");
        SerializedProperty senderName = serializedObject.FindProperty("_senderName");
        SerializedProperty d_receiver = serializedObject.FindProperty("_defaultReceiver");
        SerializedProperty d_cc = serializedObject.FindProperty("_defaultCC");
        SerializedProperty subject = serializedObject.FindProperty("_defaultSubject");
        SerializedProperty html = serializedObject.FindProperty("_isBodyHTML");
        SerializedProperty d_body = serializedObject.FindProperty("_defaultBody");

        host.stringValue = EditorGUILayout.TextField("SMTP Host : ", host.stringValue);

        port.intValue = EditorGUILayout.IntField("SMTP Port : ", port.intValue);

        ssl.boolValue = EditorGUILayout.Toggle("Use SSL : ", ssl.boolValue);

        timeout.intValue = EditorGUILayout.IntField("Timeout : ", timeout.intValue);

        sender.stringValue = EditorGUILayout.TextField("SMTP User Name : ", sender.stringValue);

        if (em.showSenderPassword)
        {
            pw.stringValue = EditorGUILayout.TextField("SMTP User Password : ", pw.stringValue);
        }
        else
        {
            pw.stringValue = EditorGUILayout.PasswordField("SMTP User Password : ", pw.stringValue);
        }
        em.showSenderPassword = GUILayout.Toggle(em.showSenderPassword, "Show Password");

        EditorGUILayout.Separator();

        senderName.stringValue = EditorGUILayout.TextField("Sender Name : ", senderName.stringValue);

        EditorGUILayout.Separator();

        d_receiver.stringValue = EditorGUILayout.TextField("Default Receiver : ", d_receiver.stringValue);
        d_cc.stringValue = EditorGUILayout.TextField("Default CC : ", d_cc.stringValue);

        EditorGUILayout.Separator();

        subject.stringValue = EditorGUILayout.TextField("Default Subject : ", subject.stringValue);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Default Body :");
        html.boolValue = GUILayout.Toggle(html.boolValue, "HTML Body");
        EditorGUILayout.EndHorizontal();
        d_body.stringValue = EditorGUILayout.TextArea(d_body.stringValue, GUILayout.MinHeight(150));


        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
