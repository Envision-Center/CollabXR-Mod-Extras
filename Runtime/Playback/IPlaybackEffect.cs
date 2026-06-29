namespace CollabXR.ModExtras
{
    /** <summary>
     * Implemented by any MonoBehaviour component that should respond to normalized playback position.
     * </summary> */
    public interface IPlaybackEffect
    {
        /** <summary>Total discrete frames this effect contains. Return 0 for continuous effects (audio, particles).</summary> */
        int FrameCount { get; }

        /** <summary>Sets the visual or audio state to match the given normalized position in [0,1].</summary> */
        void SetPercent(float normalizedPercent);

        /** <summary>Pre-computes frame lists or other cached data. Called once by PlaybackDirector on initialization and on dataset switches.</summary> */
        void CalculateFrameCount();

        /** <summary>Called when playback transitions from paused to playing.</summary> */
        void OnPlay();

        /** <summary>Called when playback transitions from playing to paused.</summary> */
        void OnPause();

        /** <summary>Called when playback is stopped and reset to position zero.</summary> */
        void OnStop();
    }
}

