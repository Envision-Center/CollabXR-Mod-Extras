using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
	/**
	 * <summary>
	 * An interface for cycle parts that use playback components. Implementing this interface allows a cycle part to specify which playback components it needs and how they should be built.
	 * </summary>
	 */
	public interface IPlaybackComponentUser
	{
		/**
		 * <summary>
		 * Called during cycle part initialization to build the necessary playback components for this cycle part. The implementing class should add any required IPlaybackComponent instances to the provided list, and can use the cycleParts list to reference other parts of the cycle if needed.
		 * </summary>
		 */
		public void BuildComponent(List<IPlaybackComponent> components, List<MonoBehaviour> cycleParts);
	}
}
