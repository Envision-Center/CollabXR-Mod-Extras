using System;
using UnityEditor;
using UnityEngine;
using CollabXR;
using CollabXR.ModExtras;

[Obsolete("Deprecated; Use PlaybackDirector system instead.")]
public class CyclePreview : EditorWindow
{
    private CycleMaster cycleMaster;

    //[MenuItem("CollabXR/Mod Extras/Cycle Preview")]
    public static void ShowWindow()
    {
        GetWindow<CyclePreview>("Cycle Preview");
    }

    void OnGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Cycle Master Controls", EditorStyles.boldLabel);

        cycleMaster = (CycleMaster) EditorGUILayout.ObjectField("Cycle Master", cycleMaster, typeof(CycleMaster), true);
        if (!cycleMaster)
        {
            EditorGUILayout.HelpBox("No CycleMaster", MessageType.Info);
            return;
        }

        if(!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to preview cycle.", MessageType.Info);
        }

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        float newPercent = EditorGUILayout.Slider("Percent", cycleMaster.GetCurrentPercent(), 0f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            cycleMaster.SetPause(true);
            cycleMaster.SetPercent(newPercent);
        }
        GUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play"))
        {
            cycleMaster.SetPause(false);
        }
        if (GUILayout.Button("Pause"))
        {
            cycleMaster.SetPause(true);
        }
        EditorGUILayout.EndHorizontal();



        if (GUI.changed)
        {
            EditorUtility.SetDirty(cycleMaster);
            SceneView.RepaintAll();
        }
    }

    private void Update()
    {
        if (EditorApplication.isPlaying)
        {
            if (cycleMaster && !cycleMaster.IsPaused())
            {
                cycleMaster?.UpdateMaster();
                Repaint();
            }
        }
    }
}
