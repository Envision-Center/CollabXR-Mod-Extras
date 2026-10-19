using System;
using UnityEngine;

namespace CollabXR
{
	/// <summary>
	/// The physics body a mod belongs to, implemented by CollabXR on the object's network root.
	/// Mod extras components get it with <c>GetComponentInParent&lt;IPhysicsBody&gt;()</c> from Start or later,
	/// and use it instead of the Rigidbody so their changes are networked.
	/// </summary>
	public interface IPhysicsBody
	{
		/// <summary>
		/// True when this machine is simulating the body. Continuous behaviours should only run when this is true while one-off commands are routed automatically.
		/// </summary>
		bool HasAuthority { get; }

		/// <summary>
		/// True while any player is holding the body.
		/// </summary>
		bool IsHeld { get; }

		/// <summary>
		/// The GameObject the underlying Rigidbody lives on.
		/// </summary>
		GameObject gameObject { get; }

		float Mass { get; set; }
		float LinearDamping { get; set; }
		float AngularDamping { get; set; }
		bool UseGravity { get; set; }
		bool IsKinematic { get; set; }
		Vector3 LinearVelocity { get; set; }
		Vector3 AngularVelocity { get; set; }

		void AddForce(Vector3 force, ForceMode mode = ForceMode.Force);
		void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force);
		void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force);

		/// <summary>
		/// Raised when the body starts touching another collider. Only reliable where <see cref="HasAuthority"/> is true,
		/// since other clients hold the body kinematic.
		/// </summary>
		event Action<Collision> CollisionEntered;
	}
}
