using System;
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
	/// Tuning for how an object leaves the hand when thrown.
	/// </summary>
	[Serializable]
	public class ThrowTuning
	{
		[SerializeField]
		[Min(0f)]
		[Tooltip("Multiplier on the hand's speed when released. Above 1 makes the object fly further.")]
		private float strength = 1f;

		[SerializeField]
		[Min(0f)]
		[Tooltip("Multiplier on the hand's spin when released.")]
		private float spin = 1f;

		[SerializeField]
		[Min(0f)]
		[Tooltip("Fastest the object can leave the hand, in meters per second.")]
		private float maxSpeed = 15f;

		[SerializeField]
		[Range(0.02f, 0.5f)]
		[Tooltip("How many seconds of recent hand motion the throw is based on. Shorter is snappier, longer is smoother.")]
		private float sampleWindow = 0.1f;

		public float Strength => strength;
		public float Spin => spin;
		public float MaxSpeed => maxSpeed;
		public float SampleWindow => sampleWindow;
	}

	/// <summary>
	/// Optional physics settings for an object with a Rigidbody on its root.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	public class PhysicsBodySettings : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("How mass changes when a player resizes this object.")]
		private PhysicsScaleMode scaleMode = PhysicsScaleMode.ScaleMassWithVolume;

		[SerializeField]
		[Min(0f)]
		[Tooltip("Multiplier on world gravity for this object. Only applies when the Rigidbody uses gravity.")]
		private float gravityScale = 1f;

		[SerializeField]
		private ThrowTuning throwing = new();

		public PhysicsScaleMode ScaleMode => scaleMode;
		public float GravityScale => gravityScale;
		public ThrowTuning Throwing => throwing;
	}
}
