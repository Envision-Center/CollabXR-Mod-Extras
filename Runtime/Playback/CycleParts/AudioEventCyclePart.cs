using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Triggers one-shot audio clips at specific frame markers during playback.
     * Unlike AudioCyclePart (continuous), this fires discrete audio events at frame boundaries,
     * similar to animation events. Automatically detects frame crossings and triggers events.
     * </summary> */
    public class AudioEventCyclePart : CyclePart, IFrameContextAware, ISubFrameEffect, IScrubbableEffect
    {
	    [Header("Audio Event Settings")]
	    [Tooltip("AudioSource to play one-shot clips from. If null, events will not be triggered.")]
        [SerializeField] private AudioSource source;
	    [Tooltip("List of audio events to trigger at specific frame numbers. Frame numbers should be within the total frame count of the cycle.")]
        [SerializeField] private List<AudioFrameEvent> frameEvents = new();

        private float _lastPercent = -1f;
        private int _maxFrameCount = 0;
        private bool _isPlaying = false;
        private bool _isScrubbing = false;

        [System.Serializable]
        public struct AudioFrameEvent
        {
	        [Tooltip("Frame number at which to trigger this audio event. Should be between 0 and the total frame count of the cycle.")]
            public int frame;
            [Tooltip("Audio clip to play when this event is triggered.")]
            public AudioClip clip;
            [Tooltip("Volume at which to play the audio clip (0.0 to 1.0).")]
            public float volume;

            public AudioFrameEvent(int frameNum, AudioClip audioClip, float vol = 1f)
            {
                frame = frameNum;
                clip = audioClip;
                volume = Mathf.Clamp01(vol);
            }
        }

        public override int FrameCount => frameEvents.Max(e => e.frame) + 1; // Total frames is one more than the highest frame index

        /** <summary>
         * Receives normalized playback position. Automatically detects frame boundary crossings
         * and triggers registered one-shot audio events.
         * </summary> */
        public override void SetPercent(float normalizedPercent)
        {
            if (source == null || frameEvents.Count == 0 || _maxFrameCount == 0)
            {
                base.SetPercent(normalizedPercent);
                return;
            }

            int currentFrame = Mathf.FloorToInt(normalizedPercent * _maxFrameCount);
            // On first call treat last as current so no spurious events fire at spawn
            int prevFrame = _lastPercent >= 0f ? Mathf.FloorToInt(_lastPercent * _maxFrameCount) : currentFrame;

            if (_isPlaying && !_isScrubbing)
                CheckFrameEventCrossings(prevFrame, currentFrame);

            _lastPercent = normalizedPercent;
            base.SetPercent(normalizedPercent);
        }

        /** <summary>Checks if any frame events occur in the range between last and current frame.</summary> */
        private void CheckFrameEventCrossings(int last, int current)
        {
            if (last == current) return;

            bool wrapped = current < last;

            foreach (var audioEvent in frameEvents)
            {
                bool crossed = wrapped
                    ? (audioEvent.frame > last || audioEvent.frame <= current)
                    : (audioEvent.frame > last && audioEvent.frame <= current);

                if (crossed && audioEvent.clip != null)
                    source.PlayOneShot(audioEvent.clip, audioEvent.volume);
            }
        }

        public override void OnPlay() { _isPlaying = true; }
        public override void OnPause() { _isPlaying = false; }
        public override void OnStop()
        {
            _isPlaying = false;
            _lastPercent = -1f;
        }

        public void SetFrameContext(int maxFrameCount)
        {
	        _maxFrameCount = maxFrameCount;
        }

        public void OnScrubStart()
        {
            _isScrubbing = true;
        }

        public void OnScrubEnd(bool resumePlay)
        {
            _isScrubbing = false;
            // _lastPercent is already at the scrubbed position from continuous SetPercent calls,
            // so the next tick will only detect crossings from the final scrub position onward.
        }
    }
}


