using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CollabXR.ModExtras.Editor
{
    [CustomEditor(typeof(ObjectCyclePart))]
    public class ObjectCyclePartEditor : UnityEditor.Editor
    {
        private SerializedProperty objectsProperty;

        private void OnEnable()
        {
            objectsProperty = serializedObject.FindProperty("objects");
        }

        public override void OnInspectorGUI()
        {
	        base.OnInspectorGUI();

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Editor Populate Tools", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Populate the objects list from child GameObjects.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Populate (Hierarchy Order)", GUILayout.Height(30)))
            {
                PopulateObjectsByHierarchy();
            }

            if (GUILayout.Button("Populate (Alphabetical Order)", GUILayout.Height(30)))
            {
                PopulateObjectsByName();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void PopulateObjectsByHierarchy()
        {
            ObjectCyclePart cyclePart = (ObjectCyclePart)target;
            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in cyclePart.transform)
            {
                children.Add(child.gameObject);
            }

            objectsProperty.arraySize = children.Count;
            for (int i = 0; i < children.Count; i++)
            {
                objectsProperty.GetArrayElementAtIndex(i).objectReferenceValue = children[i];
            }

            EditorUtility.SetDirty(cyclePart);
        }

        private void PopulateObjectsByName()
        {
            ObjectCyclePart cyclePart = (ObjectCyclePart)target;
            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in cyclePart.transform)
            {
                children.Add(child.gameObject);
            }

            children.Sort((a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));

            objectsProperty.arraySize = children.Count;
            for (int i = 0; i < children.Count; i++)
            {
                objectsProperty.GetArrayElementAtIndex(i).objectReferenceValue = children[i];
            }

            EditorUtility.SetDirty(cyclePart);
        }
    }
}


