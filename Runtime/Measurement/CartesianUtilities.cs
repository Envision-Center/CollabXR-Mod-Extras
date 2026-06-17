using UnityEngine;

namespace CollabXR.ModExtras
{
    public static class CartesianUtilities
    {
        /// <summary>A Unit of Length.</summary>
        public enum UnitOfLength
        {
            Custom,

            Petameters,
            Kilometers,
            Meters,
            Centimeters,
            Millimeters,
            Micrometers,
            Nanometers,
            Picometers,
            Femtometers,

            Miles,
            Yards,
            Feet,
            Inches,
            ThousandthsOfAnInch,
            NauticalMiles,

            Parsecs,
            LightYears,
            AstronomicalUnits,
            LightSeconds,
        }

        /// <summary>Returns a unit suffix for the given Unit of Length.</summary>
        public static string UnitSuffix(UnitOfLength unitOfLength)
        {
            switch (unitOfLength)
            {
                case UnitOfLength.Petameters:
                    return "Pm";
                case UnitOfLength.Kilometers:
                    return "km";
                case UnitOfLength.Meters:
                    return "m";
                case UnitOfLength.Centimeters:
                    return "cm";
                case UnitOfLength.Millimeters:
                    return "mm";
                case UnitOfLength.Micrometers:
                    return "μm";
                case UnitOfLength.Nanometers:
                    return "nm";
                case UnitOfLength.Picometers:
                    return "pm";
                case UnitOfLength.Femtometers:
                    return "fm";
                case UnitOfLength.Miles:
                    return "mi";
                case UnitOfLength.Yards:
                    return "yd";
                case UnitOfLength.Feet:
                    return "ft";
                case UnitOfLength.Inches:
                    return "in";
                case UnitOfLength.ThousandthsOfAnInch:
                    return "mil";
                case UnitOfLength.NauticalMiles:
                    return "NM";
                case UnitOfLength.Parsecs:
                    return "pc";
                case UnitOfLength.LightYears:
                    return "ly";
                case UnitOfLength.AstronomicalUnits:
                    return "AU";
                case UnitOfLength.LightSeconds:
                    return "ls";
                default:
                case UnitOfLength.Custom:
                    return "";
            }
        }

        public static void InitializeTextLabel(TextMesh label)
        {
            MeshRenderer renderer = label.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

            label.anchor = TextAnchor.MiddleCenter;
            label.fontSize = 64;
        }

        /// <summary>
        /// Find a child of the given transform with the given name, and ensure it has a component.
        /// If no component is found, add one.
        /// If no game object is found, make a child and reset its local position/rotation/scale.
        ///
        /// Note: the transform name must be unique from other children for deterministic results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="transformName"></param>
        /// <param name="gameObject"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static Transform EnsureGameObject<T>(
            Transform parent,
            string transformName,
            out GameObject gameObject,
            out T component
        )
            where T : Component
        {
            // First, see if we can find the child in the tree.
            Transform child = parent.Find(transformName);
            if (child != null)
            {
                gameObject = child.gameObject;

                // Check if we have a component of desired type. Add one if not.
                if (!gameObject.TryGetComponent(out component))
                {
                    component = gameObject.AddComponent<T>();
                }

                return child;
            }

            // Otherwise, create a new game object with corresponding component
            gameObject = new GameObject(transformName, typeof(T));
            component = gameObject.GetComponent<T>();

            // Set parent and clear any accumulated transforms (just in case)
            child = gameObject.transform;
            child.parent = parent;
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            child.localScale = Vector3.one;

            return child;
        }

        /// <summary>
        /// Find a child of the given transform with the given name.
        /// If no game object is found, make a child and reset its local position/rotation/scale.
        ///
        /// Note: the transform name must be unique from other children for deterministic results.
        /// </summary>
        public static Transform EnsureTransform(Transform parent, string transformName)
        {
            // First, see if we can find the child in the tree.
            Transform child = parent.Find(transformName);
            if (child != null)
            {
                return child;
            }

            // Otherwise, create a new game object with corresponding component
            var gameObject = new GameObject(transformName);

            // Set parent and clear any accumulated transforms (just in case)
            child = gameObject.transform;
            child.parent = parent;
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            child.localScale = Vector3.one;

            return child;
        }

        /// <summary>Divides one by the given vector.</summary>
        public static Vector3 InverseVector3(Vector3 divisor)
        {
            return new Vector3(1.0f / divisor.x, 1.0f / divisor.y, 1.0f / divisor.z);
        }

        /// <summary>
        /// Returned result is not normalized.
        /// <see href="https://discussions.unity.com/t/calculating-a-movement-direction-that-is-a-tangent-to-a-slope-surface/1077"/>
        /// </summary>
        public static Vector3 SlideAgainstNormal(Vector3 direction, Vector3 normal)
        {
            return Vector3.Cross(Vector3.Cross(normal, direction), normal);
        }

        /// <summary>
        /// Computes a plane normal from 3 points using counter-clockwise face-winding.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns>A normalized plane normal vector</returns>
        public static Vector3 PlaneNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var u = p2 - p1;
            var v = p3 - p1;
            var c = Vector3.Cross(u, v);

            var len = c.sqrMagnitude;
            if (len <= 1e-6)
            {
                return Vector3.up;
            }
            return c / Mathf.Sqrt(len);
        }
    }
}
