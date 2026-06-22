using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Base class for all playback effect implementations.
     * </summary> */
    public abstract class CyclePart : MonoBehaviour, IPlaybackEffect
    {
        protected float lastPercent = 0;
        protected int lastFrame = 0;

        public virtual int FrameCount => 0;

        /** <summary>Sets the visual/audio state to match normalizedPercent. Subclasses override to apply effect-specific logic.</summary> */
        public virtual void SetPercent(float normalizedPercent)
        {
            lastPercent = normalizedPercent;
            lastFrame = GetFrame(normalizedPercent);
        }

        public virtual void CalculateFrameCount() { }

        public virtual void OnPlay() { }
        public virtual void OnPause() { }
        public virtual void OnStop() { }

        /** <summary>Converts a normalized percent [0,1] to a discrete frame index [0, FrameCount-1].</summary> */
        protected int GetFrame(float normalizedPercent)
        {
            if (FrameCount == 0) return 0;
            int frame = Mathf.FloorToInt(normalizedPercent * FrameCount);
            return Mathf.Clamp(frame, 0, FrameCount - 1);
        }
    }
}
