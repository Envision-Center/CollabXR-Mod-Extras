namespace CollabXR.ModExtras
{
    /** <summary>
     * Interface for playback effects that need special handling during timeline scrubbing.
     * PlaybackDirector calls these when the networked IsScrubbing state changes.
     * </summary> */
    public interface IScrubbableEffect
    {
        /** <summary>Called when scrubbing begins.</summary> */
        void OnScrubStart();

        /** <summary>Called when scrubbing ends.</summary> */
        void OnScrubEnd(bool resumePlay);
    }
}
