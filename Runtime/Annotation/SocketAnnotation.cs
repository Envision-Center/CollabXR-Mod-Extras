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
		/// Pass a generic data structure, such as Legend Metadata.
		/// The type of the data structure will be used to determine the socket type inside Collab.
		/// This cannot be animated.
		/// </summary>
		ScriptableObject = 0,

		/// <summary>
		/// Outputs a floating-point value.
		/// This can be animated over time using callbacks.
		/// </summary>
		Float = 1,

		/// <summary>
		/// Outputs a Texture. This property can be animated on the SocketAnnotation.
		/// </summary>
		Texture = 2,

        /// <summary>
        /// Used for volumetric data visualization, like volume slicing.
        /// Outputs a Texture3D, along with a Point of Reference transform used to determine orientation/scale.
        /// This can be animated over time using callbacks.
        /// </summary>
        Volumetric = 3,

        /// <summary>
        /// Fires a plain event with no parameters.
        /// This can be animated over time using callbacks.
        /// </summary>
        PlainEvent = 4,

        /// <summary>
        /// Pass a string.
        /// This can be animated over time using callbacks.
        /// </summary>
        String = 5,
	}

	[CreateAssetMenu(fileName = "SocketAnnotation", menuName = "CollabXR/Sockets/Annotation")]
	public class SocketAnnotation : MonoBehaviour
	{
		[Tooltip("Determines what kind of data is passed through the socket, and how it behaves.")]
		public SocketBehavior behavior;

        [Header("Behavior Data")]
        [Tooltip("Point of reference Transform. This can be used to augment certain functionalities. For example, pointing to a Toggle Controller will allow it to be integrated with Legend Metadata.")]
        public Transform pointOfReference = null;
        
		[Tooltip("Metadata to pass to the Socket constructor. The type of data passed determines what socket is constructed.")]
		public ScriptableObject scriptableObject = null;

        [SerializeField, Tooltip("Default texture to output over the socket.")]
        public Texture textureValue = null;

        [Tooltip("Default string value to output over the socket.")]
        public string stringValue = "";

        [Tooltip("Default floating-point value to output over the socket.")]
		public float floatValue = 0.0f;

        /// <summary>Generic event with no parameters.</summary>
        [HideInInspector]
        public UnityEvent c_plainEvent;

        /// <summary>Change event for texture.</summary>
        [HideInInspector]
		public UnityEvent<Texture> c_texture;

        /// <summary>Change event for string data.</summary>
        [HideInInspector]
        public UnityEvent<string> c_string;

        /// <summary>Change event for floating-point data.</summary>
        [HideInInspector]
		public UnityEvent<float> c_float;

		// Set up event emitters
		private void Awake()
		{
            if (c_plainEvent == null)
            {
                c_plainEvent = new UnityEvent();
            }
            if (c_texture == null)
			{
				c_texture = new UnityEvent<Texture>();
			}
            if (c_string == null)
            {
                c_string = new UnityEvent<string>();
            }
            if (c_float == null)
			{
				c_float = new UnityEvent<float>();
			}
		}

		private void OnDestroy()
		{
            c_plainEvent.RemoveAllListeners();
            c_texture.RemoveAllListeners();
            c_string.RemoveAllListeners();
            c_float.RemoveAllListeners();
		}

		/// <summary>
		/// Fires the plain event with no parameters.
		/// Allows users to manually trigger it with UnityEvents or animations.
		/// </summary>
		public void PushPlainEvent()
		{
			c_plainEvent.Invoke();
		}

		/// <summary>
		/// Pushes a new texture through the socket.
		/// Can be used for animating textures or volumetrics in mods.
		/// 
		/// Note that this does NOT replace the default texture.
		/// </summary>
		/// <param name="newTexture"></param>
		public void PushTexture(Texture newTexture)
		{
			c_texture.Invoke(newTexture);
		}

		/// <summary>
		/// Pushes a new string through the socket.
		/// Can be used for animating text in mods.
		/// 
		/// Note that this does NOT replace the default texture.
		/// </summary>
		/// <param name="newString"></param>
		public void PushString(string newString)
		{
			c_string.Invoke(newString);
		}

		/// <summary>
		/// Pushes a new floating-point value through the socket.
		/// </summary>
		/// <param name="newFloat"></param>
		public void PushFloat(float newFloat)
		{
			c_float.Invoke(newFloat);
		}
	}
}
