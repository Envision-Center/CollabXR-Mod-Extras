using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using UnityEngine.Events;

namespace CollabXR.ModExtras
{
    public class ToggleController : MonoBehaviour
    {
        public List<ToggleableObject> toggleableChildren;

        private void OnDestroy()
        {
            if (toggleableChildren != null)
            {
                foreach (ToggleableObject obj in toggleableChildren)
                {
                    obj.RemoveListeners();
                }
            }
        }
    }

    public enum ToggleableObjectType
    {
        GameObject,
        Component,
        VFXBool,
        ShaderBool
    }

    [System.Serializable]
    public class ToggleableObject
    {
        public ToggleableObjectType type;
        public GameObject obj;
        public MonoBehaviour component;
        public VisualEffect vfx;
        public MeshRenderer mesh;
        public string shaderPropertyName;
        public bool defaultEnabled = false;
        public bool needsTransparencySlider = false;
        public float defaultTransparency = 0.5f;

		/// <summary>
		/// Internal enabled state.
		/// </summary>
		private bool enabled = false;

		/// <summary>
		/// Whether or not this is currently enabled.
		/// </summary>
		public bool currentlyEnabled { get { return enabled; } }

        [Tooltip(
            "Emitted when this variable is toggled on or off. Repeats may occur due to networking."
        )]
		[NonSerialized]
        public UnityEvent<bool> toggledEvent = new UnityEvent<bool>();

        /// <summary>
        /// Remove attached event listeners.
        /// </summary>
        public void RemoveListeners()
        {
            if (toggledEvent != null)
            {
                toggledEvent.RemoveAllListeners();
            }
        }

        public void Toggle(bool enabled)
        {
			this.enabled = enabled;
            switch (type)
            {
                case ToggleableObjectType.GameObject:
                    obj.SetActive(enabled);
                    break;
                case ToggleableObjectType.Component:
                    component.enabled = enabled;
                    break;
                case ToggleableObjectType.VFXBool:
                    vfx.SetBool(shaderPropertyName, enabled);
                    break;
                case ToggleableObjectType.ShaderBool:
                    mesh.materials[0].SetInt(shaderPropertyName, enabled ? 1 : 0);
                    break;
            }
            toggledEvent.Invoke(enabled);
        }

        /// <summary>
        /// Sets the Sorting Group priority of the toggleable object, if possible.
        /// </summary>
        /// <param name="priority"></param>
        public void SetPriority(int priority)
        {
            switch (type)
            {
                case ToggleableObjectType.GameObject:
                    SortingGroup group;
                    if (obj.TryGetComponent(out group))
                    {
                        group.sortingOrder = priority;
                    }
                    break;
            }
        }

        /// <summary>
        /// Whether to show a priority menu.
        /// </summary>
        /// <returns></returns>
        public bool CanPrioritize()
        {
            switch (type)
            {
                case ToggleableObjectType.GameObject:
                    return obj.TryGetComponent<SortingGroup>(out _);
                default:
                    return false;
            }
        }
    }
}
