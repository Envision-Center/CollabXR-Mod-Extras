using CollabXR;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CollabXR.ModExtras
{
    [ExecuteInEditMode]
    public class CartesianOrigin : MonoBehaviour
    {
        [Tooltip("Axis label settings.")]
        public CartesianGizmoSettings settings;

        private struct Label
        {
            public Vector3 axis;
            public float length;
            public CartesianGizmoSettings.AxisLabel settings;
            public GameObject label;
            public GameObject axisObject;
        }

        private Label[] labels;

        private Label ConstructAxisLine(
            string objectLabel,
            CartesianGizmoSettings.AxisLabel axisSettings,
            Vector3 axisVector,
            bool negative
        )
        {
            float length = axisSettings.lineLength;
            Label labelInfo = new Label
            {
                axis = axisVector,
                length = length,
                settings = axisSettings
            };

            // Attempt to find our transform if it already exists
            string axisLabelName = string.Format("CartesianAxisLine{0}", objectLabel);
            Transform axisT = transform.Find(axisLabelName);

            if (
                (!axisSettings.showAxisNegative && negative)
                || (!axisSettings.showAxisPositive && !negative)
            )
            {
                // If our transform exists and it shouldn't, destroy it
                if (axisT != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(axisT);
                    }
                    else
                    {
                        // Running code in-editor requires immediate destruction instead of deferred
                        DestroyImmediate(axisT);
                    }
                }
                return labelInfo;
            }

            GameObject axisObj;
            LineRenderer axis;
            CartesianUtilities.EnsureGameObject(transform, axisLabelName, out axisObj, out axis);

            // Start setting up transform
            axisObj.transform.SetLocalPositionAndRotation(settings.originOffset, Quaternion.identity);
            axisObj.transform.localScale = Vector3.one;

            labelInfo.axisObject = axisObj;

            axis.material = settings.GetLineRenderMaterial();
            axis.material.SetColor("_BaseColor", axisSettings.color);
            axis.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            axis.useWorldSpace = false;
            axis.startWidth = axisSettings.lineWidth;
            axis.endWidth = axisSettings.lineWidth;

            // Build color gradient, fade off tips of axis for coolness factor
            Gradient colorGradient = new Gradient();
            GradientAlphaKey[] keysAlpha = new GradientAlphaKey[3];
            keysAlpha[0].alpha = 1.0f;
            keysAlpha[0].time = 0.0f;
            keysAlpha[1].alpha = 1.0f;
            keysAlpha[1].time = 0.8f;
            keysAlpha[2].alpha = 0.0f;
            keysAlpha[2].time = 1.0f;
            colorGradient.alphaKeys = keysAlpha;
            GradientColorKey[] keysColor = new GradientColorKey[1];
            keysColor[0].color = axisSettings.color;
            keysColor[0].time = 0.0f;
            axis.colorGradient = colorGradient;
            colorGradient.colorKeys = keysColor;

            axis.colorGradient = colorGradient;

            axis.positionCount = 2;
            // All axii start at 0, 0, 0
            axis.SetPosition(1, axisVector * length);

            if (axisSettings.showLabel)
            {
                labelInfo.label = ConstructAxisLabel(
                    objectLabel,
                    axisObj.transform,
                    axisSettings,
                    axisVector,
                    length,
                    negative
                );
            }

            return labelInfo;
        }

        private GameObject ConstructAxisLabel(
            string objectLabel,
            Transform parent,
            CartesianGizmoSettings.AxisLabel axisSettings,
            Vector3 axisVector,
            float length,
            bool negative
        )
        {
            // Create parent transform to ensure consistent scaling
            string axisLabelName = string.Format("CartesianAxisLabel{0}", objectLabel);
            Transform axisParent = CartesianUtilities.EnsureTransform(parent, axisLabelName);

            GameObject axisLabel;
            TextMesh label;
            CartesianUtilities.EnsureGameObject(axisParent, "label", out axisLabel, out label);

            axisLabel.transform.SetLocalPositionAndRotation(
                axisVector * (length * axisSettings.labelProportion),
                Quaternion.identity
            );
            axisLabel.transform.localScale = Vector3.one;

            CartesianUtilities.InitializeTextLabel(label);
            label.characterSize = axisSettings.labelScale;

            if (negative)
            {
                label.text = axisSettings.labelNegative;
                label.alignment = TextAlignment.Right;
            }
            else
            {
                label.text = axisSettings.labelPositive;
                label.alignment = TextAlignment.Left;
            }

            return axisLabel;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (Application.isPlaying)
            {
                Build();
            }
        }

        // Build a new preview
        public void Build()
        {
            // If no settings provided, load defaults.
            if (settings == null)
            {
                settings = new CartesianGizmoSettings();
            }

            // Note: positive and negative lines are split up to reduce depth-sorting/layering issues at the origin
            labels = new Label[6]
            {
                ConstructAxisLine("XPositive", settings.XAxis, Vector3.right, false),
                ConstructAxisLine("XNegative", settings.XAxis, Vector3.left, true),
                ConstructAxisLine("YPositive", settings.YAxis, Vector3.up, false),
                ConstructAxisLine("YNegative", settings.YAxis, Vector3.down, true),
                ConstructAxisLine("ZPositive", settings.ZAxis, Vector3.forward, false),
                ConstructAxisLine("ZNegative", settings.ZAxis, Vector3.back, true),
            };
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying)
            { // Don't modify transforms in editor preview
                return;
            }

            // Update label orientations
            foreach (Label label in labels)
            {
                // Don't bother repositioning text if text does not exist, or there is no current camera
                if (label.label == null)
                {
                    continue;
                }

                // Automatically adjust for non-uniform scale of parent
                // NOTE: This scale must be applied to a parent transform of the actual text label, so the rotation space is uniformly scaled
                Vector3 lossyScale = transform.lossyScale;
                if (lossyScale.sqrMagnitude < 0.001)
                {
                    return;
                }
                float minScale = Mathf.Min(lossyScale.x, lossyScale.y, lossyScale.z);
                label.label.transform.parent.localScale = CartesianUtilities.InverseVector3(lossyScale) * minScale;

                // Dynamically pick label offset from actual axis drawing based on camera orientation
                Vector3 offset = Vector3.Cross(
                    label.label.transform.parent.worldToLocalMatrix.MultiplyVector(Camera.main.transform.forward),
                    label.axis
                ).normalized * (label.settings.lineWidth + label.settings.labelScale * 2.0f);

                // Place label at center of axis
                label.label.transform.localPosition = offset + Vector3.Scale(label.axis, lossyScale / minScale) * (label.length * 0.8f);

                // Make sure label always faces the camera
                label.label.transform.LookAt(Camera.main.transform.position, Camera.main.transform.up);
                // Text renders forward-facing, so we need to rotate it 180 degrees on yaw axis so it's not horizontally mirrored
                label.label.transform.localRotation *= Quaternion.AngleAxis(180.0f, Vector3.up);
            }
        }

        private void OnDestroy()
        {
            // Automatically clean up transforms if component was destroyed
            if (labels == null)
            {
                return;
            }

            foreach (Label label in labels)
            {
                if (label.label != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(label.label);
                    }
                    else
                    {
                        DestroyImmediate(label.label);
                    }
                }

                if (label.axisObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(label.axisObject);
                    }
                    else
                    {
                        DestroyImmediate(label.axisObject);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CartesianOrigin))]
    public class CartesianOriginInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CartesianOrigin origin = (CartesianOrigin)target;
            if (GUILayout.Button("Generate Preview"))
            {
                origin.Build();
            }
        }
    }
#endif
}
