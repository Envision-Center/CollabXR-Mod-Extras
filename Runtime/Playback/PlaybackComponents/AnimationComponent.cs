using System.Collections.Generic;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Component that tracks the active animation set index for AnimatorCyclePart.
     * Only attached to entities that have animation playback capabilities.
     * </summary> */
    public class AnimationComponent : IPlaybackComponent
    {
	    public string DisplayName => "Animation Cycler";
	    public int ActiveSetIndex { get; set; }
	    public int SetCount { get; set; }
        public void OnAttached() { }
        public void OnDetached() { }

        public string GetSetDisplayName(int index)
        {
	        return _effect.GetSetDisplayName(index);
        }

        public int ComponentId { get; } = 1;

        private AnimatorCyclePart _effect;

        public AnimationComponent(AnimatorCyclePart effect, int numSets, int initial = 0)
        {
	        _effect = effect;
	        SetCount = numSets;
	        ActiveSetIndex = initial;
        }

        public void SetComponentValues(int index)
        {
	        if (index < 0 || index >= SetCount)
		        return;

	        ActiveSetIndex = index;

	        _effect?.SetActiveSet(index);
        }
    }
}

