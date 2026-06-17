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
		/// Outputs a static Texture2D. Can be animated.
		/// </summary>
		StaticImage = 2,

		/// <summary>
		/// Used for volumetric data visualization, like volume slicing.
		/// Outputs a static Texture3D, along with a Point of Reference transform used to determine orientation/scale. Can be animated.
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
		[Tooltip("Texture to use for displaying static images. This can be animated.")]
		public Texture2D imageTexture {
			get { return f_imageTexture; }
			set { f_imageTexture = value; c_imageTexture.Invoke(value); }
		}
		[SerializeField, DontCreateProperty]
		private Texture2D f_imageTexture;
		/// <summary>
		/// Change event for image textures.
		/// </summary>
		[HideInInspector]
		public UnityEvent<Texture2D> c_imageTexture;

		[CreateProperty]
		[Tooltip("Texture to use for Volume Slicing. This can be animated.")]
		public Texture3D volumeTexture {
			get { return f_volumeTexture; }
			set { f_volumeTexture = value; c_volumeTexture.Invoke(value); }
		}
		[SerializeField, DontCreateProperty]
		private Texture3D f_volumeTexture;
		/// <summary>
		/// Change event for volume textures.
		/// </summary>
		[HideInInspector]
		public UnityEvent<Texture3D> c_volumeTexture;
		[Tooltip("Point of reference Transform for volume slicing or other things.")]
		public Transform pointOfReference;

		// Set up event emitters
		private void Awake()
		{
			if (c_imageTexture == null)
			{
				c_imageTexture = new UnityEvent<Texture2D>();
			}
			if (c_volumeTexture == null)
			{
				c_volumeTexture = new UnityEvent<Texture3D>();
			}
		}

		private void OnDestroy()
		{
			c_imageTexture.RemoveAllListeners();
			c_volumeTexture.RemoveAllListeners();
		}
	}
}
