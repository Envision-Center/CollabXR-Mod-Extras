using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CollabXR.ModExtras
{
    public class CartesianGridPlane : MonoBehaviour
    {
        public enum CullingMode
        {
            None,
            Backface,
            Frontface,
        }

        [System.Serializable]
        public struct PlaneData
        {
            [Tooltip("Surface normal of plane.")]
            public Vector3 normal;

            [Tooltip("Use right-hand rule to determine this!")]
            public Vector3 axis1;

            [Tooltip("This axis is automatically determined.")]
            public Vector3 axis2;
        }

        private struct PlaneRuntime
        {
            public Plane plane;
            public GameObject obj;
            public LineRenderer frame;
            public LineRenderer[] markers;
			public bool hasMaterialBaseColor;

			/// <summary>
			/// Updates the culling of the given plane based on the given view normal.
			/// Culled planes are disabled.
			/// </summary>
			/// <param name="data"></param>
			/// <param name="view">Global space direction from center of object to camera.</param>
			/// <param name="frameColor">Color of the plane's frame.</param>
			/// <param name="lineColor">Color of the plane's markers.</param>
			/// <param name="fadeEnabled">If true, enables transparency fading.</param>
			/// <param name="fadeDotMultiplier"></param>
			public void UpdateCulling(PlaneData data, Vector3 view, Color frameColor, Color lineColor, bool fadeEnabled, float fadeDotMultiplier)
            {
                // Convert plane normal to global space
                Vector4 transformedNormal =
                    obj.transform.localToWorldMatrix
                    * new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, 0.0f);

                // Test to see if this plane is facing away from the camera
                float dotProd = Mathf.Clamp01(Vector3.Dot(
                    view,
                    new Vector3(transformedNormal.x, transformedNormal.y, transformedNormal.z)
                ) * fadeDotMultiplier);

				bool shouldBeActive = dotProd > 0.0f;
				frame.gameObject.SetActive(shouldBeActive);

				if (fadeEnabled && shouldBeActive)
                {
					Color faded = frameColor;
					faded.a = dotProd;

					// Update vertex colors
					frame.startColor = faded;
					frame.endColor = faded;

					// Update render material for frame (this shared material is only used for this plane)
					if (hasMaterialBaseColor)
					{
						frame.sharedMaterial.SetColor("_BaseColor", faded);
					}

					faded = lineColor;
					faded.a = dotProd;

					// Update vertex colors
					foreach (LineRenderer marker in markers)
					{
						marker.startColor = faded;
						marker.endColor = faded;
					}

					// Update render material for markers (this shared material is used across all markers on this plane)
					if (hasMaterialBaseColor && markers.Length > 0)
					{
						markers[0].sharedMaterial.SetColor("_BaseColor", faded);
					}
				}
            }
        }

        private struct LabelLine
        {
            /// <value>Bound point on the box that this line starts at</value>
            public int start;
            /// <value>Bound point on the box that this line ends at</value>
            public int end;
            /// <value>Text labels assocated with this line</value>
            public GameObject[] labels;
            /// <value>Original location of each corresponding text label</value>
            public Vector3[] labelPositions;
        }

        [Tooltip("What measurement planes to show at any given time.")]
        public CullingMode cullingMode = CullingMode.Backface;

        [Tooltip("Color to use for the grid lines.")]
        public Color lineColor = Color.grey;
        [Tooltip("Color to use for the plane boundaries.")]
        public Color frameColor = Color.white;

        [Tooltip("If true, planes will get more transparent as they approach the culling threshold. Works best without labels.")]
        public bool fadeEnabled = false;

        [Tooltip("The dot product of the plane normal to camera's direction to the object gets scaled by this amount. Larger values result in a shorter fade transition. Negative values invert the plane culling.")]
        public float fadeDotMultiplier = 2.0f;

        [Tooltip("Local-space bounding box that is centered on the object and contains it.")]
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2.0f);

        [Tooltip("Measurement settings.")]
        public CartesianGizmoSettings settings;

        // Plane layout
        public PlaneData[] planes = new PlaneData[]
        {
            new PlaneData()
            {
                normal = Vector3.forward,
                axis1 = Vector3.right,
                axis2 = Vector3.up
            },
            new PlaneData()
            {
                normal = Vector3.right,
                axis1 = Vector3.up,
                axis2 = Vector3.forward
            },
            new PlaneData()
            {
                normal = Vector3.up,
                axis1 = Vector3.forward,
                axis2 = Vector3.right
            },
            new PlaneData()
            {
                normal = -Vector3.forward,
                axis1 = Vector3.right,
                axis2 = -Vector3.up
            },
            new PlaneData()
            {
                normal = Vector3.left,
                axis1 = Vector3.back,
                axis2 = Vector3.down
            },
            new PlaneData()
            {
                normal = -Vector3.up,
                axis1 = -Vector3.forward,
                axis2 = Vector3.right
            },
        };
        private PlaneRuntime[] runtimes = new PlaneRuntime[] { };
        private Vector3[] boundPoints;
        private LabelLine[] labelLines;

        private void OnValidate()
        {
            for (int i = 0; i < planes.Length; i++)
            {
                planes[i].normal = planes[i].normal.normalized;
                planes[i].axis1 = (
                    planes[i].axis1
                    - (planes[i].normal * Vector3.Dot(planes[i].normal, planes[i].axis1))
                ).normalized;
                planes[i].axis2 = Vector3.Cross(planes[i].normal, planes[i].axis1).normalized;
            }
        }

        private void InitLineRenderer(LineRenderer line)
        {
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.useWorldSpace = false;
            line.widthMultiplier = settings.measureLineWidth;
            line.startColor = lineColor;
            line.endColor = lineColor;
        }

        private GameObject PlaceMarkerLabel(GameObject parent, Vector3 position, float value)
        {
            Transform labelParent = CartesianUtilities.EnsureTransform(parent.transform, string.Format("Label {0} {1}", value, position));
            labelParent.parent = labelParent;
            labelParent.SetLocalPositionAndRotation(position, Quaternion.identity);
            labelParent.localScale = Vector3.one;

            GameObject labelObj;
            TextMesh label;
            CartesianUtilities.EnsureGameObject(labelParent, "label", out labelObj, out label);

            labelObj.SetActive(false); // Disable until unculled
            labelObj.transform.parent = labelParent;
            labelObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            labelObj.transform.localScale = Vector3.one;

            CartesianUtilities.InitializeTextLabel(label);
            label.characterSize = settings.measureLabelScale;

            // Build label string
            float valueConverted = settings.lengthConversionRate * value;
            string formattedNumber;
            if (settings.lengthLabelInteger)
            {
                formattedNumber = string.Format("{0:D" + settings.lengthLabelPrecision + "}", Mathf.RoundToInt(valueConverted));
            } else
            {
                formattedNumber = string.Format("{0:F" + settings.lengthLabelPrecision + "}", valueConverted);
            }

            label.text = string.Format("{0}{1}", formattedNumber, settings.displayedUnitString);
            label.alignment = TextAlignment.Center;

            return labelObj;
        }

        private LineRenderer[] DrawMarkers(
            GameObject parent,
            PlaneData data,
            Plane plane,
            Vector3 axisPrimary,
            Vector3 axisSecondary
        )
        {
            // Create line markers on Axis 1 and Axis 2.
            // First, figure out how many markers are needed along each axis
            float markerSpacing = settings.measureInterval;
            int markersPerSide =
                Mathf.FloorToInt(
                    Mathf.Abs(Vector3.Dot(bounds.extents, axisPrimary)) / markerSpacing
                ) - 1;

            LineRenderer[] markerList = new LineRenderer[2 * markersPerSide + 1];

			for (int i = -1, k = 0; i < markersPerSide * 2; i++, k++)
            {
                int j;
                if (i == -1)
                {
                    j = 0;
                }
                // Place a marker on every other side of the box
                else if (i % 2 == 0)
                {
                    j = (i / -2) - 1;
                }
                else
                {
                    j = (i / 2) + 1;
                }

                // Find the center of the marker along the plane
                Vector3 markerPos = plane.ClosestPointOnPlane(axisPrimary * (markerSpacing * j));

                string markerName = string.Format("Marker{0}{1}", i, axisPrimary);

                // Build the marker
                GameObject marker;
                LineRenderer markerLine;
                CartesianUtilities.EnsureGameObject(
                    parent.transform,
                    markerName,
                    out marker,
                    out markerLine
                );
                marker.SetActive(true);

                InitLineRenderer(markerLine);
                marker.transform.parent = parent.transform;
                marker.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                marker.transform.localScale = Vector3.one;
                markerLine.positionCount = 2;

                float axisLength = Mathf.Abs(Vector3.Dot(axisSecondary, bounds.extents));
                markerLine.SetPosition(0, markerPos + axisSecondary * (axisLength + settings.measureSpokeWidth));
                markerLine.SetPosition(1, markerPos - axisSecondary * (axisLength + settings.measureSpokeWidth));

                markerList[k] = markerLine;
            }

            return markerList;
        }

        private PlaneRuntime BuildPlane(PlaneData data, int idx)
        {
            GameObject frameObj;
            LineRenderer frame;
            CartesianUtilities.EnsureGameObject(
                transform,
                string.Format("GridPlaneFrame{0}", idx),
                out frameObj,
                out frame
            );

            frameObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            frameObj.transform.localScale = Vector3.one;
            frameObj.SetActive(true);

            InitLineRenderer(frame);
			// Initialize material for individual plane render
            frame.sharedMaterial = settings.GetLineRenderMaterial();

			bool hasMaterialBaseColor = frame.sharedMaterial.HasColor("_BaseColor");
			if (hasMaterialBaseColor) {
				frame.sharedMaterial.SetColor("_BaseColor", frameColor);
			}

            // Project the plane normal onto the box to find the plane center
            Plane plane = new Plane(
                data.normal,
                bounds.center + Mathf.Abs(Vector3.Dot(bounds.extents, data.normal)) * data.normal
            );

            frame.loop = true;
            frame.positionCount = 4;
            frame.SetPosition(0, plane.ClosestPointOnPlane(bounds.min));
            frame.SetPosition(
                1,
                plane.ClosestPointOnPlane(
                    Vector3.Dot(bounds.min, data.axis1) * data.axis1
                        + Vector3.Dot(bounds.max, data.axis2) * data.axis2
                )
            );
            frame.SetPosition(2, plane.ClosestPointOnPlane(bounds.max));
            frame.SetPosition(
                3,
                plane.ClosestPointOnPlane(
                    Vector3.Dot(bounds.max, data.axis1) * data.axis1
                        + Vector3.Dot(bounds.min, data.axis2) * data.axis2
                )
            );

            // Create line markers on Axis 1 and Axis 2.
            // First, figure out how many markers are needed along each axis
            LineRenderer[] markers = DrawMarkers(frameObj, data, plane, data.axis1, data.axis2);
            markers = markers
                .Concat(DrawMarkers(frameObj, data, plane, data.axis2, data.axis1))
                .ToArray();


			// Initialize material render for marker lines on plane
			var material = settings.GetLineRenderMaterial();
			if (hasMaterialBaseColor)
			{
				material.SetColor("_BaseColor", lineColor);
			}
			foreach (var marker in markers)
			{
				marker.sharedMaterial = material;
			}

			return new PlaneRuntime
            {
                obj = frameObj,
                frame = frameObj.GetComponent<LineRenderer>(),
                plane = plane,
                markers = markers,
				hasMaterialBaseColor = hasMaterialBaseColor,
            };
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (Application.isPlaying)
            {
                Build();
            }
        }

        public void Build() {
            // If no settings provided, load defaults.
            if (settings == null)
            {
                settings = new CartesianGizmoSettings();
            }

            // Initialize plane graphics
            runtimes = new PlaneRuntime[planes.Length];
            for (int i = 0; i < planes.Length; i++)
            {
                runtimes[i] = BuildPlane(planes[i], i);
            }

            // Don't bother creating labels if not configured to
            if (!settings.displayMeasureLabels)
            {
                return;
            }

            // Get vertex corners of the bounding box
            boundPoints = new Vector3[8]
            {
                bounds.min,
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), // 1
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), // 2
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // 3
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), // 4
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), // 5
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), // 6
                bounds.max,
            };

            // Now, calculate all the lines used in the Bounding Box
            labelLines = new LabelLine[12]
            {
                new LabelLine { start = 0, end = 1 },
                new LabelLine { start = 0, end = 2 },
                new LabelLine { start = 0, end = 3 },
                new LabelLine { start = 6, end = 7 },
                new LabelLine { start = 5, end = 7 },
                new LabelLine { start = 4, end = 7 },
                new LabelLine { start = 1, end = 4 },
                new LabelLine { start = 1, end = 5 },
                new LabelLine { start = 2, end = 5 },
                new LabelLine { start = 2, end = 6 },
                new LabelLine { start = 3, end = 4 },
                new LabelLine { start = 3, end = 6 },
            };

            // Finally, for each line, place labels on every marker increment.
            // We do this separately from plane construction so we don't double-up on labels (which would be bad).
            // Having labels associated with a line allows us to more easily select and cull them.
            for (int i = 0; i < 12; i++)
            {
                Vector3 start = boundPoints[labelLines[i].start];
                Vector3 end = boundPoints[labelLines[i].end];

                // Assume our increments are starting from the center of the line
                Vector3 center = (start + end) * 0.5f;

                Vector3 axis = (end - center).normalized;
                int markersPerSide = Mathf.FloorToInt((end - center).magnitude / settings.measureInterval) - 1;

                GameObject[] labelList = new GameObject[markersPerSide * 2 + 1];
                Vector3[] positionList = new Vector3[labelList.Length];
                for (int j = -1, k = 0; j < markersPerSide * 2; j++, k++)
                {
                    int l;
                    if (j == -1)
                    {
                        l = 0;
                    }
                    // Place a marker on every other side of the box
                    else if (j % 2 == 0)
                    {
                        l = (j / -2) - 1;
                    }
                    else
                    {
                        l = (j / 2) + 1;
                    }

                    // Figure out where the label should be roughly placed
                    Vector3 labelPos = center + (l * settings.measureInterval) * axis;

                    labelList[k] = PlaceMarkerLabel(
                        gameObject,
                        labelPos,
                        l * settings.measureInterval - Vector3.Dot(axis, settings.originOffset)
                    );
                    positionList[k] = labelPos;
                }

                labelLines[i].labels = labelList;
                labelLines[i].labelPositions = positionList;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (cullingMode != CullingMode.None)
            {
                //Vector3 viewVector = Camera.main.transform.forward;

                // Using direction vector from camera to object works better when translated to the side in VR
                Vector3 viewVector = (
                    transform.localToWorldMatrix.MultiplyPoint(bounds.center)
                    - Camera.main.transform.position
                ).normalized;

                if (cullingMode == CullingMode.Frontface)
                { // If front-face culling, just flip view vector so it backface culls backwards
                    viewVector *= -1;
                }

                for (int i = 0; i < planes.Length; i++)
                {
                    runtimes[i].UpdateCulling(planes[i], viewVector, frameColor, lineColor, fadeEnabled, fadeDotMultiplier);
                }
            }

            // Don't bother updating label transforms if not configured to.
            if (!settings.displayMeasureLabels)
            {
                return;
            }

            // Cull our marker labels.
            // First, find the closest and furthest points on the bounding box to the camera.
            // We'll make the camera position/up vector relative to our object so we don't have to transform as many coordinates.
            Vector3 cameraPos = transform.worldToLocalMatrix.MultiplyPoint(Camera.main.transform.position);

            int closest = 0;
            int furthest = 0;
            for (int i = 0; i < boundPoints.Length; i++)
            {
                float sqrDist = (boundPoints[i] - cameraPos).sqrMagnitude;
                if (sqrDist < (boundPoints[closest] - cameraPos).sqrMagnitude)
                {
                    closest = i;
                }
                else if (sqrDist > (boundPoints[furthest] - cameraPos).sqrMagnitude)
                {
                    furthest = i;
                }
            }

            // Make camera position world-space again so our LookAt methods work.
            cameraPos = Camera.main.transform.position;
            Vector3 cameraUp = Camera.main.transform.up;
            Vector3 cameraForward = Camera.main.transform.forward;

            // Now, enable all labels EXCEPT for the ones residing on lines with these closest or furthest points.
            foreach (LabelLine line in labelLines)
            {
                bool cull = (
                    line.start == closest
                    || line.start == furthest
                    || line.end == closest
                    || line.end == furthest
                );
                Vector3 axis = (boundPoints[line.end] - boundPoints[line.start]).normalized;

                // Determine local-space normal of relevant plane based using the line segment,
                // and furthest point from camera (which is shared across all visible planes)
                Vector3 n = CartesianUtilities.PlaneNormal(boundPoints[furthest], boundPoints[line.start], boundPoints[line.end]);

                // Vector3 localCameraForward = transform.worldToLocalMatrix.MultiplyVector(cameraForward).normalized;
                Vector3 cameraToCenter = (transform.worldToLocalMatrix.MultiplyPoint(cameraPos) - bounds.center).normalized;

                // Flip the plane normal as necessary, since we're always facing the camera
                if (Vector3.Dot(n, cameraToCenter) < 0)
                {
                    n *= -1.0f;
                }

                // Get vector that is perpendicular to our axis and plane normal
                Vector3 displayOffset = Vector3.Cross(n, axis);

                for (int i = 0; i < line.labels.Length; i++)
                {
                    GameObject label = line.labels[i];
                    if (cull)
                    {
                        label.SetActive(false);
                    } else
                    {
                        label.SetActive(true);

                        // Automatically adjust for non-uniform scale of parent
                        // NOTE: This scale must be applied to a parent transform of the actual text label, so the rotation space is uniformly scaled
                        Vector3 lossyScale = transform.lossyScale;
                        if (lossyScale.sqrMagnitude < 0.001)
                        {
                            continue;
                        }
                        float minScale = Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
                        label.transform.parent.localScale = CartesianUtilities.InverseVector3(lossyScale) * minScale;

                        // Ensure label is always on the OPPOSITE SIDE as the origin
                        float edgeOffset = settings.labelSettings.edgeOffset;
                        if ( Vector3.Dot( displayOffset, (bounds.center - label.transform.parent.localPosition).normalized) > 0)
                        {
                            edgeOffset *= -1.0f;
                        }

                        // FINALLY, offset label position based on axis
                        label.transform.localPosition = displayOffset * edgeOffset;

                        // Make sure label always faces the camera so it's legible
                        label.transform.LookAt(cameraPos, cameraUp);
                        // Text renders forward-facing, so we need to rotate it 180 degrees on yaw axis so it's not horizontally mirrored
                        label.transform.localRotation *= Quaternion.AngleAxis(180.0f, Vector3.up);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CartesianGridPlane))]
    public class CartesianGridPlaneInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CartesianGridPlane origin = (CartesianGridPlane)target;
            if (GUILayout.Button("EXPERIMENTAL Preview"))
            {
                origin.Build();
            }
        }
    }
#endif
}
