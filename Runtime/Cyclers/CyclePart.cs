using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    public class CyclePart : MonoBehaviour
    {
        protected int lastFrame = 0;

        public virtual void SetFramePercentage(float frameRatio) {
            lastFrame = GetFrame(frameRatio);
        }

        public virtual int GetFrameCount() { return 0; }

        public virtual void CalculateFrameCount() { }

        public int GetFrame(float frameRatio) {
            int frame = Mathf.FloorToInt(frameRatio * GetFrameCount()); // frame including partial frames
            frame = Mathf.Clamp(frame, 0, GetFrameCount() - 1);
            return frame;
        }
    }
}
