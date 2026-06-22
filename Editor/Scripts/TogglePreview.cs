using UnityEditor;
using UnityEngine;
using CollabXR;
using CollabXR.ModExtras;

public class TogglePreview : EditorWindow
{
    private ToggleController toggleController;
    private Vector2 scroll;
    private int activeIndex = 0;

    [MenuItem("CollabXR/Mod Extras/Toggle Preview")]
    public static void ShowWindow()
    {
        GetWindow<TogglePreview>("Toggle Preview");
    }

    void OnGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Toggle Controller", EditorStyles.boldLabel);
        toggleController = (ToggleController) EditorGUILayout.ObjectField("Toggle Controller", toggleController, typeof(ToggleController), true);

        if (toggleController == null)
        {
            EditorGUILayout.HelpBox("No ToggleController", MessageType.Warning);
        }

        GUILayout.Space(5);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Dataset", EditorStyles.boldLabel);
        GUILayout.Space(2);

        if (toggleController)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            var children = toggleController.toggleableChildren;

            if (children == null || children.Count == 0)
            {
                EditorGUILayout.HelpBox("No datasets found", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    var t = children[i];
                    if (t == null || t.obj == null) continue;

                    bool current = t.obj.activeSelf;

                    EditorGUILayout.BeginHorizontal();


                    GUI.backgroundColor = current ? Color.green : new Color(0.5f, 0.5f, 0.5f);
                    if (GUILayout.Button(t.obj.name, GUILayout.Height(22)))
                    {
                        t.Toggle(!current);
                    }
                    GUI.backgroundColor = Color.white;

                    var part = t.obj.GetComponent<CyclePart>();
                    if (part)
                    {
                        part?.CalculateFrameCount();
                        EditorGUILayout.LabelField($"{part?.GetFrameCount()} frames", GUILayout.Width(80));
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Navigation", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Prev"))
        {
            activeIndex = GetActiveIndex(toggleController, -1);
        }
        if (GUILayout.Button("Next"))
        {
            activeIndex = GetActiveIndex(toggleController, 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();

        

        if (GUI.changed)
        {
            EditorUtility.SetDirty(toggleController);
            SceneView.RepaintAll();
        }
    }


    // Get next or previous
    private int GetActiveIndex(ToggleController toggleController, int direction)
    {
        if (toggleController == null || toggleController.toggleableChildren.Count == 0) return 0;

        var list = toggleController.toggleableChildren;
        int currentIndex = -1;
        for (int i = 0; i < list.Count; i++)
            if (list[i].obj.activeSelf) currentIndex = i;

        if (direction == 1)
            currentIndex = (currentIndex + direction) % list.Count;
        else
            currentIndex = (currentIndex + direction + list.Count) % list.Count;

        return currentIndex;
    }


    public void UpdateFrameDisplay()
    {
        if (!toggleController) return;

        // Show visible datasets
        if (toggleController && toggleController.toggleableChildren.Count > 0)
        {
            for (int i = 0; i < toggleController.toggleableChildren.Count; i++)
            {
                var t = toggleController.toggleableChildren[i];
                bool shouldBeOn = (i == activeIndex);
                t.Toggle(shouldBeOn);
            }
        }
    }
}
