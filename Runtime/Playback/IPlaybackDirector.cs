
using System.Collections.Generic;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Contract for the local (non-networked) playback director.
     * </summary> */
    public interface IPlaybackDirector
    {
        float CurrentPercent { get; }
        bool IsPlaying { get; }
        float Speed { get; }
        float Duration { get; }
        List<IPlaybackComponent> Components { get; }

        /** <summary>Advances the internal clock by deltaSeconds and drives all effects.
         * Must only be called by the network authority path (FixedUpdateNetwork).</summary> */
        void Tick(float deltaSeconds);

        /** <summary>Drives all effects to the given percent without advancing the internal clock.
         * Called every render frame by non-authority clients from the extrapolated anchor position.</summary> */
        void ApplyAnchorPercent(float percent);

        void Play();
        void Pause();
        void Stop();

        /** <summary>Seeks to the given normalized position in [0,1] and immediately drives all effects.</summary> */
        void Seek(float normalizedPercent);

        void SetSpeed(float multiplier);
        void SetComponentIndex(int componentId, int index);

        /** <summary>When false, Update() will not advance the clock.
         * NetworkPlaybackDirector sets this to false on all clients so it owns the tick cadence.</summary> */
        void SetLocallyDriven(bool driven);
    }
}

