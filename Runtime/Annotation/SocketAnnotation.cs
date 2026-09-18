using Unity.Properties;
using UnityEngine;
using UnityEngine.Events;

namespace CollabXR.ModExtras.Annotation
{
	/// <summary>
	/// What kind of data is being passed through this socket.
	/// </summary>
	public enum SocketBehavior
	{
		/// <summary>
		/// Pass a generic data structure, such as Legend information.
		/// The type of the data structure will be used to determine the socket type inside Collab.
		/// </summary>
		ScriptableObject = 0,

		/// <summary>
		/// Outputs a stream of floating-point values using the float value stored in this annotation.
		/// This can be animated over time.
		/// </summary>
		FloatStream = 1,

		/// <summary>
		/// Outputs a Texture. This property can be animated on the SocketAnnotation.
		/// </summary>
		Texture = 2,

		/// <summary>
		/// Used for volumetric data visualization, like volume slicing.
		/// Outputs a Texture3D, along with a Point of Reference transform used to determine orientation/scale. Can be animated.
		/// </summary>
		Volumetric = 3,
	}

	[CreateAssetMenu(fileName = "SocketAnnotation", menuName = "CollabXR/Sockets/Annotation")]
	public class SocketAnnotation : MonoBehaviour
    {
		[Tooltip("Determines how the socket behaves.")]
		public SocketBehavior behavior;

		[Header("Behavior Data")]

		[Tooltip("Metadata to pass to the Socket constructor. The type of data passed determines what socket is constructed.")]
		public ScriptableObject scriptableObject;

		[Tooltip("Value to pass in the floating-point stream. This can be animated.")]
		public float floatStreamValue;

		// We use a private field + a setter and getter so we can bind to change events
		[CreateProperty]
		[Tooltip("Texture to use for 2D or 3D display. This can be animated by changing the reference.")]
		public Texture texture {
			get { return f_texture; }
			set { f_texture = value; c_texture.Invoke(value); }
		}
		[SerializeField, DontCreateProperty]
		private Texture f_texture;
		/// <summary>
		/// Change event for texture.
		/// </summary>
		[HideInInspector]
		public UnityEvent<Texture> c_texture;

		[Tooltip("Point of reference Transform for volume slicing or other things.")]
		public Transform pointOfReference;

		// Set up event emitters
		private void Awake()
		{
			if (c_texture == null)
			{
				c_texture = new UnityEvent<Texture>();
			}
		}

		private void OnDestroy()
		{
			c_texture.RemoveAllListeners();
		}
	}
}
