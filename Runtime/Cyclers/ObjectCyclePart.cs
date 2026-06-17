using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    public class ObjectCyclePart : CyclePart
    {
        private List<GameObject> objectsToCycle;
        void Start()
        {
            CalculateFrameCount();
        }

        public override void CalculateFrameCount()
        {
            base.CalculateFrameCount();
            objectsToCycle = new List<GameObject>();
            foreach (Transform child in transform)
            {
                objectsToCycle.Add(child.gameObject);
            }
            objectsToCycle.Sort((p, q) => p.name.CompareTo(q.name));
        }

        public override void SetFramePercentage(float frameRatio) // set frame to display as a ratio of the total frame count (0.0 to 1.0)
        {
            if (objectsToCycle == null) return;
            int frame = GetFrame(frameRatio);
            objectsToCycle[lastFrame].SetActive(false);
            objectsToCycle[frame].SetActive(true);
            base.SetFramePercentage(frameRatio);
        }

        public override int GetFrameCount()
        {
            if (objectsToCycle == null) return 0;
            return objectsToCycle.Count;
        }
    }
}
