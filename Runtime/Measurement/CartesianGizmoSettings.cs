using CollabXR;
using System;
using UnityEngine;

namespace CollabXR.ModExtras
{
    [CreateAssetMenu(
        fileName = "CoordinateGizmoSettings",
        menuName = "CollabXR/Measurement/Cartesian Gizmo Settings"
    )]
    public class CartesianGizmoSettings : ScriptableObject
    {
        [Tooltip("Custom local-space offset for the displayed origin and grid measurements.")]
        public Vector3 originOffset = Vector3.zero;

        [Tooltip("Whether to display measurement labels.")]
        public bool displayMeasureLabels = true;

        [Tooltip(
            "Unit of length to display measurements in. Presets are used for standardization, but custom units can be specified. Note that this only affects how units are marked, not actual measurement."
        )]
        public CartesianUtilities.UnitOfLength displayedUnitPreset = CartesianUtilities
            .UnitOfLength
            .Meters;

        [Tooltip("Displayed unit on length measurements.")]
        public string displayedUnitString = "m";

        [Tooltip("Automatically scales distances by this amount when writing measurements.")]
        public float lengthConversionRate = 1f;

        [Tooltip("Number of decimal points to display on labels.")]
        public int lengthLabelPrecision = 1;

        [Tooltip("If true, uses integer precision instead of floating-point.")]
        public bool lengthLabelInteger = false;

        [
            Tooltip("At what interval (in local-space) to place measurements along each axis."),
            Range(0.001f, 100.0f)
        ]
        public float measureInterval = 0.5f;

        [Tooltip("Draw width of grid lines."), Range(0.001f, 1.0f)]
        public float measureLineWidth = 0.01f;

        [Tooltip(
            "Additional extension of measurement lines to help mark corresponding measurements."
        )]
        public float measureSpokeWidth = 0.05f;

        [Tooltip("Scaling applied to measurement text size.")]
        public float measureLabelScale = 0.01f;

        [Serializable]
        private struct GridSettings { }

        [Serializable]
        public class LabelSettings
        {
            [Tooltip(
                "How far from grid edges, in engine units, the center of the label is placed at."
            )]
            public float edgeOffset = 0.1f;
        }

        public LabelSettings labelSettings;

        [Serializable]
        public class AxisLabel
        {
            [Tooltip(
                "What color to use for this axis. Typically, color channels match the direction vector (i.e. 1,0,0 is drawn as red)."
            )]
            public Color color = Color.red;

            [Tooltip("Whether or not to show the positive axis.")]
            public bool showAxisPositive = true;

            [Tooltip("Whether or not to show the positive axis.")]
            public bool showAxisNegative = false;

            [Tooltip("Displayed length of the axis."), Range(0.01f, 20.0f)]
            public float lineLength = 1.0f;

            [Tooltip("Displayed width of the axis."), Range(0.001f, 2.0f)]
            public float lineWidth = 0.02f;

            [Tooltip("Whether or not to show the string label of the axis.")]
            public bool showLabel = true;

            [Tooltip("Label to use for the positive axis direction.")]
            public string labelPositive = "+X";

            [Tooltip("Label to use for the negative axis direction.")]
            public string labelNegative = "-X";

            [Tooltip("Text scaling for the axis label."), Range(0.01f, 2.0f)]
            public float labelScale = 0.02f;

            [
                Tooltip("Proportion along the axis line to place the axis label at."),
                Range(0.01f, 2.0f)
            ]
            public float labelProportion = 0.625f;
        }

        public AxisLabel XAxis = new AxisLabel()
        {
            labelPositive = "+X",
            labelNegative = "-X",
            color = Color.red
        };
        public AxisLabel YAxis = new AxisLabel()
        {
            labelPositive = "+Y",
            labelNegative = "-Y",
            color = Color.green
        };
        public AxisLabel ZAxis = new AxisLabel()
        {
            labelPositive = "+Z",
            labelNegative = "-Z",
            color = Color.blue
        };

        [Tooltip(
            "Material to use for Line Renders. Automatically loads a default if none provided."
        )]
        public Shader lineRenderShader = null;

        [Tooltip("Render queue to use for line renders. -1 to automatically use from shader.")]
        public int lineRenderQueue = 2999;

        private void OnValidate()
        {
            if (displayedUnitPreset != CartesianUtilities.UnitOfLength.Custom)
            {
                displayedUnitString = CartesianUtilities.UnitSuffix(displayedUnitPreset);
            }
        }

        // Return the configured material to use for line renders, or a default if none is provided.
        public Material GetLineRenderMaterial()
        {
            Material mat;
            if (lineRenderShader != null)
            {
                mat = new Material(lineRenderShader);
                mat.renderQueue = lineRenderQueue;
                return mat;
            }

            // Try to load ModExtras occluded sprite shader
            Shader loaded = Shader.Find("EnvironmentDepth/URP/OcclusionUnlit");
            if (loaded == null)
            {
                // Debug.LogWarning("Unable to find Occluded Line Render shader material for Cartesian Grid. Using default Sprite shader.");

                // If loading fails, just return the default shader
                mat = new Material(Shader.Find("Sprites/Default"));
                mat.renderQueue = lineRenderQueue;
                return mat;
            }

            mat = new Material(loaded);
            mat.renderQueue = lineRenderQueue;
            return mat;
        }
    }
}
