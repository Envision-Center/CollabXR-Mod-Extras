using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Synchronizes an AudioSource to playback position with smooth interpolation.
     * Uses a seek deadband to avoid re-seeking on every frame (which causes glitches).
     * Continuously follows the playback position regardless of sync mode.
     * </summary> */
    public class AudioCyclePart : CyclePart
    {
	    [Header("Audio Cycle Settings")]
	    [Tooltip("The audio source that this part will play.")]
        [SerializeField] private AudioSource source;
	    [Tooltip("Can leave at default; Minimum time difference (in seconds) required to seek the audio source when changing playback position. This prevents glitches caused by re-seeking on every frame when the playback position changes slightly.")]
        [SerializeField] private float seekDeadbandSeconds = 0.08f;

        public override int FrameCount => 0;  // Audio is continuous, not frame-based

        public override void SetPercent(float normalizedPercent)
        {
            if (source == null || source.clip == null) return;

            float targetTime = normalizedPercent * source.clip.length;
            float currentTime = source.time;

            if (Mathf.Abs(currentTime - targetTime) > seekDeadbandSeconds)
            {
                source.time = targetTime;
            }

            base.SetPercent(normalizedPercent);
        }

        public override void OnPlay()
        {
            if (source != null && source.clip != null)
            {
                if (!source.isPlaying)
                    source.Play();
                else
                    source.UnPause();
            }
        }

        public override void OnPause()
        {
            if (source != null && source.isPlaying)
                source.Pause();
        }

        public override void OnStop()
        {
            if (source != null)
            {
                source.Stop();
                source.time = 0f;
            }
        }
    }
}

