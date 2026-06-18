using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Drives an Animator to a normalized time position.
     * </summary> */
    public class AnimatorCyclePart : CyclePart, IPlaybackComponentUser, ISubFrameEffect
    {
        [SerializeField] private Animator target;
        [SerializeField] private AnimationSet[] sets;

        private bool _initialized = false;
        private int _activeSetIndex = 0;

        /// <summary>The currently active animation set index. Exposed for state synchronization.</summary>
        public int ActiveSetIndex => _activeSetIndex;

        public int NumSets => sets?.Length ?? 0;

        /// <summary>Frame count is computed from the ACTIVE animation set only, not all sets.</summary>
        public override int FrameCount
        {
	        get
            {
	            if (sets == null || sets.Length == 0 || _activeSetIndex < 0 || _activeSetIndex >= sets.Length)
	                return 0;

	            AnimationSet activeSet = sets[_activeSetIndex];
	            if (activeSet.animations == null) return 0;

	            int count = 0;
				foreach (var a in activeSet.animations)
				{
					if (a.clip != null)
						count += Mathf.CeilToInt(a.clip.frameRate * a.clip.length);
				}
				return count;
            }
        }

        private void Start()
        {
            if (sets != null)
			{
	            foreach (var set in sets)
	            {
		            set.HashClips();
	            }
			}
        }

        private void OnEnable()
        {
            _initialized = true;
        }

        public override void SetPercent(float normalizedPercent)
        {
            if (!_initialized || target == null || sets == null || sets.Length == 0) return;
            if (_activeSetIndex < 0 || _activeSetIndex >= sets.Length) return;
            if (sets[_activeSetIndex].animations == null) return;

            sets[_activeSetIndex].Play(target, normalizedPercent);
            target.speed = 0f;

            base.SetPercent(normalizedPercent);
        }

        public override void OnPlay()
        {
            if (target != null)
                target.speed = 1f;
        }

        public override void OnPause()
        {
            if (target != null)
                target.speed = 0f;
        }

        public override void OnStop()
        {
            if (target != null)
            {
                target.speed = 0f;
                if (_initialized && sets != null && sets.Length > 0 && _activeSetIndex >= 0 && _activeSetIndex < sets.Length && sets[_activeSetIndex].animations != null)
					sets[_activeSetIndex].Play(target, 0f);
            }
        }

        /// <summary>
        /// Switches to a different animation set and plays it at the current playback position.
        /// Properly clamps the index and only updates if the index actually changes.
        /// </summary>
        public void SetActiveSet(int index)
        {
	        Debug.Log($"SetActiveSet called with index {index} for AnimatorCyclePart on {gameObject.name}");
	        if (sets == null || sets.Length == 0) return;

	        index = Mathf.Clamp(index, 0, sets.Length - 1);

	        if (index != _activeSetIndex)
	        {
		        Debug.Log($"Changing active animation set from {_activeSetIndex} to {index}");
		        _activeSetIndex = index;
		        if (_initialized && target != null && _activeSetIndex >= 0 && _activeSetIndex < sets.Length && sets[_activeSetIndex].animations != null)
		        {
			        Debug.Log($"Playing new active animation set {_activeSetIndex} at last percent {lastPercent}");
			        sets[_activeSetIndex].Play(target, lastPercent);
		        }
	        }
        }

        public void BuildComponent(List<IPlaybackComponent> components, List<MonoBehaviour> cycleParts)
        {
	        var animComp = components.OfType<AnimationComponent>().FirstOrDefault();
	        if (animComp != null) return;

	        var animatorPart = cycleParts.OfType<AnimatorCyclePart>().FirstOrDefault();

	        // Initialize with the maximum number of sets across all AnimatorCycleParts
	        int maxSets = animatorPart != null ? animatorPart.NumSets : 0;
	        int initialIndex = 0; // Start with first animation set

	        animComp = new AnimationComponent(animatorPart, maxSets, initialIndex);

	        components.Add(animComp);
        }

        public string GetSetDisplayName(int setIndex)
        {
	        return sets != null && setIndex >= 0 && setIndex < sets.Length ? sets[setIndex].DisplayName ?? $"Set {setIndex}" : $"Set {setIndex}";
        }

        private void OnValidate()
        {
	        if (sets == null || sets.Length == 0) return;

	        for (int i = 0; i < sets.Length; i++)
	        {
		        var set  = sets[i];
		        if (string.IsNullOrEmpty(set.DisplayName))
		        {
			        set.DisplayName = $"Set {i}";
		        }
	        }
        }

        [Serializable]
        public struct AnimationSet
        {
	        [Tooltip("Display name for this animation set. Used in the UI to identify different sets of animations.")]
	        public string DisplayName;
	        [Tooltip("List of animations to play together as part of this set. Each animation specifies a clip, the corresponding state name in the Animator, and the layer to play on.")]
	        public List<LayeredAnimation> animations;

	        public void Play(Animator animator, float normalizedTime = Single.NegativeInfinity)
	        {
	            if (animator == null || animations == null) return;

	            if (float.IsNegativeInfinity(normalizedTime))
	            {
		            // If no specific time is provided, just play the first frame of each animation
		            normalizedTime = 0f;
	            }

		        foreach (var animation in animations)
		        {
			        animator.Play(animation.stateName, animation.layer, normalizedTime);
		        }
	        }

	        public void HashClips()
	        {
	            if (animations == null) return;

		        foreach (var animation in animations)
		        {
			        animation.HashClip();
		        }
	        }

	        [Serializable]
	        public struct LayeredAnimation
	        {
		        [Tooltip("Animation clip to play. Must be in the target Animator's controller.")]
		        public AnimationClip clip;
		        [Tooltip("Name of the state in the Animator to play. Must correspond to a state that uses the specified clip.")]
		        public string stateName;
		        [Tooltip("Name of the layer to play the animation on.")]
		        public int layer;
		        [HideInInspector] public int clipHash;

		        public void HashClip()
		        {
			        if (clip != null)
				        clipHash = Animator.StringToHash(clip.name);
		        }
	        }
        }
    }
}
