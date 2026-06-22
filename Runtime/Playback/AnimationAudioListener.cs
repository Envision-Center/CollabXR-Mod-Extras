using System;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
	/*
	 * <summary>
	 * Attach this to the gameObject with the Animator component to listen to animation events
	 * and play audio clips.
	 */
    public class AnimationAudioListener : MonoBehaviour
    {
	    [Serializable]
	    public struct AudioEvent
	    {
		    [Tooltip("The index of the animation event call. This is used to identify which audio clip to play when an animation event is fired.")]
		    public int callIndex;
		    [Tooltip("The audio clip to play when the animation event with the corresponding call index is fired.")]
		    public AudioClip clip;
		    [Tooltip("The volume at which to play the audio clip. This is a value between 0 and 1, where 1 is the original volume of the audio clip.")]
		    public float volume;
	    }
	    [Tooltip("The audio source to play the audio clips from.")]
	    public AudioSource source;
	    [Tooltip("The list of audio events, which specify the audio clip and volume to play for each call index.")]
	    public List<AudioEvent> audioEvents;

	    /**
	     * <summary>
	     * Call this in an animation event to play the audio clip specified by the index.
	     * </summary>
	     */
	    public void PlayAudio(int callIndex)
	    {
		    PlayByIndex(callIndex);
	    }

	    private void PlayByIndex(int callIndex)
	    {
		    if (source == null) return;

		    AudioEvent audioEvent = audioEvents.Find(e => e.callIndex == callIndex);

		    if (audioEvent.clip != null)
		    {
			    source.PlayOneShot(audioEvent.clip, audioEvent.volume);
		    }
	    }
    }
}

