using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Optional interface for effects that need awareness of the global frame context.
     * PlaybackDirector notifies all effects implementing this after calculating max frame count.
     * </summary> */
    public interface IFrameContextAware
    {
        /// <summary>Called by PlaybackDirector with the maximum frame count across all effects.</summary>
        void SetFrameContext(int maxFrameCount);
    }
}

