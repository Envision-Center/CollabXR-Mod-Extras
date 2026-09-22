using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CollabXR.ModExtras.Measurement
{
    /// <summary>
    /// Describes a legend, used to annotate graphs and scientific visualizations.
    /// </summary>
    [CreateAssetMenu(
        fileName = "LegendMetadata",
        menuName = "CollabXR/Measurement/Legend Metadata"
    )]
    public class LegendMetadata : ScriptableObject
    {
        [Serializable]
        public struct Variable
        {
#if UNITY_EDITOR
            [Tooltip(
                "Optional ID of the variable. Only used within the editor--useful for tooling."
            )]
            public string id;
#endif

			[Header("Labels")]
            [Tooltip("The displayed name of the variable.")]
            public string name;

			[Tooltip("Whether to display the name of variable.")]
			public bool displayLabel;

			[Header("Value Ranges")]
			[Tooltip("Whether to show unit and value information with the variable.")]
			public bool displayValues;

			[Tooltip("Associated unit of measure to display with values for this variable.")]
			public string unit;

			[Tooltip("How to format range and threshold numbers. Optionally specify in a decimal format, i.e. '0.00' or leave blank for automatic.")]
			public string precision;

			[Header("Values")]
			[Tooltip("The minimum bound of the variable's range.")]
			public float rangeMinimum;

			[Tooltip("The minimum bound of the variable's range.")]
			public float rangeMaximum;

			/// <summary>
			/// Hexadecimal version of the colors, specifically for JSON importing.
			/// </summary>
			[HideInInspector]
            public string[] colorsHex;

            [Header("Thresholds")]
            [Tooltip(
                "A single color or multiple colors associated with the variable. Will be drawn in a gradient associated with the minimum value in the range to the maximum."
            )]
            public List<Color> colors;

            [Tooltip(
                "Optional list of values to display for each corresponding color. Using thresholds will disable the total range display, because thresholds are more granular."
            )]
            public List<float> thresholds;

			[Header("Toggle Controller Integration")]
			[Tooltip(
				"Corresponding index of this variable on the given Toggle Controller, if any."
			)]
			public int toggleIndex;
		}

        /// <summary>
        /// Used for JSON deserialization.
        /// </summary>
        private struct Metadata
        {
            public Variable[] variables;
        }

        [Tooltip("Name of the graph, to display at the top of the legend.")]
        public string title = "Untitled";

#if UNITY_EDITOR
        /// <summary>
        /// JSON filepath to import this metadata from.
        /// </summary>
        [HideInInspector]
        public string importFrom = "";
#endif

        [Tooltip(
            "Definition for each variable to be displayed on the legend. The order of this list determines the order in the resulting legend."
        )]
        [SerializeField]
        public List<Variable> variables = new List<Variable>();

        /// <summary>
        /// Deserializes the JSON data into this LegendMetadata.
        /// </summary>
        /// <param name="jsonData"></param>
        public void LoadFromString(string jsonData)
        {
            Debug.Log("Received JSON: " + jsonData);
            JsonSerializerSettings settings = new JsonSerializerSettings();
            Metadata result = JsonConvert.DeserializeObject<Metadata>(jsonData, settings);
            Debug.Log("Got result " + result);

            variables.Clear();
            var newVariables = new List<Variable>();
            foreach (Variable variable in result.variables)
            {
                Debug.Log("Created variable " + variable.name);
                // Parse hexadecimal colors into Unity colors
                List<Color> colors = new List<Color>();
                foreach (string hex in variable.colorsHex)
                {
                    Color newColor;
                    if (!ColorUtility.TryParseHtmlString(hex, out newColor))
                    {
                        Debug.LogError(
                            string.Format(
                                "LegendMetadata: failed to parse hex string '{0}' as Color",
                                hex
                            )
                        );
                        newColor = Color.black;
                    }
                    colors.Add(newColor);
                }

                // Update references
                Variable newVar = variable;
                newVar.colors = colors;
                newVar.colorsHex = null;
                newVariables.Add(newVar);
            }

            variables = newVariables;
            Debug.Log(string.Format("Created {0} variables", variables.Count));
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LegendMetadata))]
    public class LegendMetadataInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            LegendMetadata data = (LegendMetadata)target;

            GUILayout.Label("JSON Import", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("JSON File", GUILayout.Width(70)); // Directory to load assets from
            data.importFrom = EditorGUILayout.TextField(data.importFrom);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFilePanel(
                    "Choose JSON File",
                    // https://stackoverflow.com/questions/674479/how-do-i-get-the-directory-from-a-files-full-path#674495
                    data.importFrom,
                    "json"
                );
                if (!string.IsNullOrEmpty(path))
                {
                    data.importFrom = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Import"))
            {
                string text = File.ReadAllText(data.importFrom);
                data.LoadFromString(text);
            }

            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            GUIStyle labelStyle = EditorStyles.label;
            labelStyle.wordWrap = true;
            labelStyle.stretchWidth = true;
            GUILayout.Label(
                "To dynamically show and hide variables on the legend, optionally add a 'Point of Reference' Transform with a ToggleController component to the legend's SocketAnnotation.",
                labelStyle
            );

            DrawDefaultInspector();
        }
    }
#endif
}
