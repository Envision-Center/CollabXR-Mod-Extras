using System;
using System.Collections.Generic;
using UnityEngine;

public class ShaderParameterDriver : MonoBehaviour
{
    private Vector4 with_w(Vector3 xyz, float w)
    {
        return new Vector4(xyz.x, xyz.y, xyz.z, w);
    }

    public enum DriverValue {
        Position = 0,
        RotationEuler = 1, // Automatically converted to radians.
        RotationQuaternion = 2, // Pipes Quaternion directly into vector.
        Scale = 3,
    }

    public enum DriverValueType
    {
        Float = 0,
        Integer = 1,
        Vector = 2,

    }

    [System.Serializable]
    public class DriverConfig
    {
        // Name of shader parameter to drive.
        public string parameter;
        // Object to use for driving shader parameter.
        public GameObject driver;

        // Whether to automatically make the driven value local to the target's local space.
        public bool localSpace = true;

        // What value is used to drive the shader parameter.
        public DriverValue value = DriverValue.Position;
        // What the shader parameter primitive type is.
        public DriverValueType type = DriverValueType.Vector;
    }

    [System.Serializable]
    public class TargetConfig
    {
        public Renderer target;
        public uint materialIndex = 0;
    }

    // List of Renderers to apply parameter updates to.
    public List<TargetConfig> targets;

    public List<DriverConfig> drivers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Update called immediately after Start step anyhow
    }

    // Update is called once per frame
    void Update()
    {
        foreach (TargetConfig target in this.targets) {
            // Skip if we don't have a material at the given index.
            if (target.target.materials.Length <= target.materialIndex)
            {
                continue;
            }

            GameObject targetObject = target.target.gameObject;

            Material mat = target.target.materials[target.materialIndex];            
            foreach (DriverConfig driver in this.drivers)
            {
                Vector4 vector = Vector4.zero; // Vector value derived from the driver value

                // Convert coordinate space as necessary
                Matrix4x4 matrix = driver.localSpace ? targetObject.transform.worldToLocalMatrix : Matrix4x4.identity;

                switch (driver.value)
                {
                    case DriverValue.Position:
                        vector = matrix * with_w(driver.driver.transform.position, 1.0f); // A W of 1 is used for transforming positions
                        vector.w = 0.0f; // Change to zero so the vector's length is true to the origin
                        break;
                    case DriverValue.RotationEuler:
                        // Perform rotation on quaternion and convert to euler angles for easy consumption
                        Vector3 euler = (driver.driver.transform.rotation * matrix.rotation).eulerAngles;
                        vector = new Vector4(euler.x, euler.y, euler.z, 0.0f); // A W of 0 is used for transforming rotations/directions
                        break;
                    case DriverValue.RotationQuaternion:
                        // Perform rotation on quaternion and convert to euler angles for easy consumption
                        Quaternion quat = driver.driver.transform.rotation * matrix.rotation;
                        vector = new Vector4(quat.x, quat.y, quat.z, quat.w); // Directly pipe quaternion
                        break;
                    case DriverValue.Scale:
                        // Scale is kind of like a direction, and should not be translated
                        vector = matrix * with_w(driver.driver.transform.lossyScale, 0.0f);
                        break;
                }

                switch (driver.type)
                {
                    case DriverValueType.Float:
                        mat.SetFloat("_" + driver.parameter, vector.magnitude);
                        break;
                    case DriverValueType.Integer:
                        mat.SetInteger("_" + driver.parameter, (int)Math.Round(vector.magnitude));
                        break;
                    case DriverValueType.Vector:
                        mat.SetVector("_" + driver.parameter, vector);
                        break;
                }
            }
        }
    }
}
