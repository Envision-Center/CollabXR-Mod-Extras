using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Activates one child GameObject per frame.
     * </summary> */
    public class ObjectCyclePart : CyclePart
    {
        protected List<GameObject> objectsToCycle;

        [Header("Object Cycle Settings")]
        [Tooltip("All objects to cycle through (enabling the active one and disabling all others). Note that this list is NOT used at runtime, as a copy is made.")]
        /// <summary>Optiontally preset a list of objects to cycle through. Only used if autoPopulateAtRuntime is false.</summary>
        public List<GameObject> objects;
        [Header("Runtime Options")]
        [FormerlySerializedAs("autoPopulate")]
        [Tooltip("If true, at runtime, the objects list will automatically be overwritten with all child GameObjects. If false, only the manually assigned objects will be used.")]
        public bool autoPopulateAtRuntime = true;
        [FormerlySerializedAs("sortByName")]
        [Tooltip("If true, the objects will be sorted alphabetically by name before cycling. If false, will use hierarchy order. Only applies if Auto Populate is enabled.")]
        public bool sortByNameWhenPopulating = false;

        public override int FrameCount => objectsToCycle?.Count ?? 0;

        private void Start()
        {
            CalculateFrameCount();
        }

        protected virtual void ObtainObjects()
        {
	        objectsToCycle = new List<GameObject>();

	        if (autoPopulateAtRuntime)
	        {
		        foreach (Transform child in transform)
		        {
			        objectsToCycle.Add(child.gameObject);
			        child.gameObject.SetActive(false); // hide on start
		        }
	        }
	        else
	        {
		        objectsToCycle = new List<GameObject>(objects);
	        }
        }

        public override void CalculateFrameCount()
        {
            ObtainObjects();

            if (!sortByNameWhenPopulating) return;
            objectsToCycle = objectsToCycle.OrderBy(obj => obj.name).ToList();
        }

        public override void SetPercent(float normalizedPercent)
        {
            if (objectsToCycle == null || objectsToCycle.Count == 0) return;

            int frame = GetFrame(normalizedPercent);
            if (frame == lastFrame) return;

            if (lastFrame >= 0 && lastFrame < objectsToCycle.Count)
                ActivateObject(lastFrame, false);

            if (frame >= 0 && frame < objectsToCycle.Count)
                ActivateObject(frame, true);

            base.SetPercent(normalizedPercent);
        }

        protected virtual void ActivateObject(int index, bool enable)
        {
	        if (objectsToCycle == null || index < 0 || index >= objectsToCycle.Count)
	        {
		        return;
	        }

	        objectsToCycle[index].gameObject.SetActive(enable);
        }
    }
}


