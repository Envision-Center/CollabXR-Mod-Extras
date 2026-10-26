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
	/// Tuning for how an object bounces and slides off surfaces.
	/// </summary>
	[Serializable]
	public class BounceTuning
	{
		[SerializeField]
		[Tooltip("If true, replaces the Physics Material on all of this object's colliders with one auto-built from the values below.")]
		private bool overrideMaterial;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("How much speed is kept when bouncing, where 0 doesn't bounce and 1 bounces back at full speed.")]
		private float bounciness = 0.6f;

		[SerializeField]
		[Min(0f)]
		[Tooltip("The object's friction; higher values slow sliding.")]
		private float friction = 0.6f;

		[SerializeField]
		[Tooltip("Keep this object's bounciness on surfaces that are less bouncy than it. When off, bounciness is averaged with the surface's.")]
		private bool ignoreSurfaceBounce = true;

		public bool OverrideMaterial => overrideMaterial;
		public float Bounciness => bounciness;
		public float Friction => friction;
		public bool IgnoreSurfaceBounce => ignoreSurfaceBounce;
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

		[SerializeField]
		private BounceTuning bounce = new();

		public PhysicsScaleMode ScaleMode => scaleMode;
		public float GravityScale => gravityScale;
		public ThrowTuning Throwing => throwing;
		public BounceTuning Bounce => bounce;
	}
}
