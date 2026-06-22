using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CollabXR.ModExtras
{
    /** <summary>
     * MVVM ViewModel for playback state. Uses EventVariable pattern for reactive state that UI can observe.
     * All command methods fire UnityEvents that NetworkPlaybackDirector subscribes to.
     * </summary> */
    public class PlaybackViewModel : MonoBehaviour
    {
        #region State (Observable by UI via EventVariable pattern)

        public EventVariable<float> Percent = new();
        public EventVariable<bool> IsPlaying = new();
        public EventVariable<float> Speed = new();
        public EventVariable<Dictionary<int, int>> ActiveSetByComponent = new(new Dictionary<int, int>()); // id, index
        public EventVariable<Dictionary<int, int>> CountByComponent = new(new Dictionary<int, int>()); // id, max count
        public EventVariable<float> Duration = new();
        public EventVariable<int> MaxFrames = new();

        #endregion

        #region Commands

        public Action OnPlayRequested = delegate { };
        public Action OnPauseRequested = delegate { };
        public Action OnStopRequested = delegate { };
        public Action<float> OnSeekRequested = delegate { };
        public Action<float> OnSpeedChangeRequested = delegate { };
        public Action<int, int> OnComponentChangeRequested = delegate { };

        public void RequestPlay() => OnPlayRequested.Invoke();
        public void RequestPause() => OnPauseRequested.Invoke();
        public void RequestStop() => OnStopRequested.Invoke();
        public void RequestSeek(float normalizedPercent) => OnSeekRequested.Invoke(normalizedPercent);
        public void RequestSetSpeed(float multiplier) => OnSpeedChangeRequested.Invoke(multiplier);
        public void RequestSetComponentSet(int componentId, int index) =>  OnComponentChangeRequested.Invoke(componentId, index);

        #endregion

        #region Immutable Properties

        public Dictionary<int, IPlaybackComponent> components;

        public void InitializeConstants(IEnumerable<IPlaybackComponent> c)
		{
			components = new Dictionary<int, IPlaybackComponent>();
			foreach (IPlaybackComponent component in c)
			{
				components[component.ComponentId] = component;
			}
		}
        #endregion

        /** <summary>
         * idk why i cant access eventvariable in main namespace but wtv
         * </summary> */
        [Serializable]
        public class EventVariable<T>
        {
            private T _value;
            private UnityEvent<T> _onChange = new();

            public T Value
            {
                get => _value;
                set
                {
                    if (!EqualityComparer<T>.Default.Equals(_value, value))
                    {
                        _value = value;
                        _onChange.Invoke(_value);
                    }
                }
            }

            public EventVariable() { }
			public EventVariable(T initialValue) => _value = initialValue;


            public void AddListener(UnityAction<T> callback) => _onChange.AddListener(callback);
            public void RemoveListener(UnityAction<T> callback) => _onChange.RemoveListener(callback);

            /** <summary>Adds listener and immediately invokes it with the current value.</summary> */
            public void AddListenerAndCheck(UnityAction<T> callback)
            {
                callback.Invoke(_value);
                _onChange.AddListener(callback);
            }
        }
    }
}

