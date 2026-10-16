using UnityEngine;

namespace CollabXR
{
	/// <summary>
	/// How a physics object's mass responds when a player resizes it.
	/// </summary>
	public enum PhysicsScaleMode
	{
		ScaleMassWithVolume,
		FixedMass,
		BlockScaling,
	}

	/// <summary>
	/// Optional settings for a mod with a Rigidbody on its root.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	public class PhysicsBodySettings : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("How mass changes when a player resizes this object.")]
		private PhysicsScaleMode scaleMode = PhysicsScaleMode.ScaleMassWithVolume;

		public PhysicsScaleMode ScaleMode => scaleMode;
	}
}
