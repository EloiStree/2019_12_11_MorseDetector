
using UnityEditor;

[CustomEditor(typeof(MorseDetector),true)]
public class MorseDetectedEditor : Editor
{
    public string m_warningMessage="";
    public override void OnInspectorGUI()
    {
        MorseDetector myTarget = (MorseDetector) target;
        if (myTarget.m_morseSource == null)
        {
            m_warningMessage = "Please provide a morse emittor that use MorseEmittorInterface";
        }
        

        if(m_warningMessage.Length>0)
            EditorGUILayout.HelpBox(m_warningMessage, MessageType.Warning);

        DrawDefaultInspector();
        m_warningMessage = "";
    }
}