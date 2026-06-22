using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Determines how PlaybackDirector synchronizes CycleParts with varying frame counts.
     * ScalePercent: All effects driven at the same normalized percent [0,1]
     * SyncByFrame: Effects synced by frame; shorter effects stop at their last frame while longer ones continue
     * </summary> */
    public enum PlaybackSyncMode
    {
        ScalePercent,  // All effects at same normalized percent
        SyncByFrame    // Sync by absolute frame number, use max frame count as duration
    }

    /** <summary>
     * Local (non-networked) playback director. Manages playback clock and drives all <c>IPlaybackEffect</c> modules
     * </summary> */
    public class PlaybackDirector : MonoBehaviour, IPlaybackDirector
    {
        #region Inspector Fields

        [Header("Playback Settings")]
        [Tooltip("Duration in seconds for a full playback cycle. Affects speed of playback and is used as reference for percent-based synchronization.")]
        [SerializeField] private float duration = 10f;
        [Tooltip("Whether playback should loop back to start when reaching the end.")]
        [SerializeField] private bool loop = true;
        [Tooltip("If true, playback begins automatically when the mod is spawned.")]
        [SerializeField] private bool playOnAwake = true;
        [Tooltip("Assign this to a PlaybackViewModel for the effect. If left empty, will search for the component on the GameObject.")]
        [SerializeField] private PlaybackViewModel viewModel;

        [Header("Playback Effects")]
        [Tooltip("Optional list of effect components to drive. If left empty, will auto-discover all IPlaybackEffect components in children.")]
        [SerializeField] private List<MonoBehaviour> effectComponents = new List<MonoBehaviour>();
        [Tooltip("Scale Percent: All effects driven at the same normalized percent [0,1]. Sync By Frame: Effects synced by frame; shorter effects stop at their last frame while longer ones continue")]
        [SerializeField] private PlaybackSyncMode syncMode = PlaybackSyncMode.ScalePercent;

        #endregion

        #region Private State

        private List<IPlaybackEffect> _effects = new List<IPlaybackEffect>();
        private float _currentPercent = 0f;
        private bool _isPlaying = false;
        private float _speed = 1f;
        private bool _locallyDriven = true;
        private List<IPlaybackComponent> _components = new List<IPlaybackComponent>();
        private int _maxFrameCount = 0;

        #endregion

        #region IPlaybackDirector Properties

        public float CurrentPercent => _currentPercent;
        public bool IsPlaying => _isPlaying;
        public float Speed => _speed;
        public float Duration => duration;
        public bool PlayOnAwake => playOnAwake;
        public List<IPlaybackComponent> Components => _components;
        public PlaybackSyncMode SyncMode => syncMode;
        public int MaxFrameCount => _maxFrameCount;
        public int CurrentFrame => Mathf.FloorToInt(_currentPercent * _maxFrameCount);

        #endregion

        #region Lifecycle

        private void Awake()
        {
            RefreshEffects();
            InitializeViewModel();
        }

        private void Update()
        {
            if (_isPlaying)
            {
                Tick(Time.deltaTime);
            }
        }

        #endregion

        #region IPlaybackDirector Methods

        /** <summary>Advances internal clock and drives all effects. Authority path only.</summary> */
        public void Tick(float deltaSeconds)
        {
            if (!_isPlaying) return;

            float advance = (deltaSeconds * _speed) / duration;
            _currentPercent += advance;

            if (_currentPercent > 1f)
            {
                if (loop)
                    _currentPercent = _currentPercent % 1f;
                else
                {
                    _currentPercent = 1f;
                    Pause();
                }
            }

            viewModel.RequestSeek(_currentPercent);
        }

        /** <summary>Drives effects without changing internal clock. Non-authority path.</summary> */
        public void ApplyAnchorPercent(float percent)
        {
            _currentPercent = Mathf.Clamp01(percent);
            DriveEffects(_currentPercent);
            PushStateToViewModel();
        }

        public void Play()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            foreach (var effect in _effects) effect.OnPlay();
            PushStateToViewModel();
        }

        public void Pause()
        {
            if (!_isPlaying) return;
             _isPlaying = false;
            foreach (var effect in _effects) effect.OnPause();
            PushStateToViewModel();
        }

        public void Stop()
        {
            _isPlaying = false;
            _currentPercent = 0f;
            foreach (var effect in _effects) effect.OnStop();
            DriveEffects(0f);
            PushStateToViewModel();
        }

        public void Seek(float normalizedPercent)
        {
            _currentPercent = Mathf.Clamp01(normalizedPercent);
            DriveEffects(_currentPercent);
            PushStateToViewModel();
        }

        public void SeekToFrame(int frame)
        {
            if (_maxFrameCount <= 0) return;

            int clampedFrame = Mathf.Clamp(frame, 0, _maxFrameCount - 1);
            float percent = (float)clampedFrame / _maxFrameCount;
            Seek(percent);
        }

        public void SetSpeed(float multiplier)
        {
            _speed = Mathf.Max(0.01f, multiplier);
            PushStateToViewModel();
        }

        public void SetComponentIndex(int componentId, int index)
        {
	        IPlaybackComponent component = _components.FirstOrDefault(c => c.ComponentId == componentId);

	        if (component == null) return;

	        component.SetComponentValues(index);

	        // Recalculate frame counts after component change (different animation may have different duration)
	        RecalculateFrameCounts();

	        DriveEffects(_currentPercent);
	        PushStateToViewModel();
        }

        public void SetLocallyDriven(bool driven)
        {
            _locallyDriven = driven;
        }

        public void SetSyncMode(PlaybackSyncMode mode)
        {
            syncMode = mode;
            // Re-drive effects with new sync mode
            DriveEffects(_currentPercent);
        }

        #endregion

        #region Internal Methods

        /**
         * <summary>
         * Begins tracking and updating the state of all effects controlled by this playback system, first discovering
         * all effects either from the assigned list or by auto-discovering in children, then calculating frame counts
         * and max frame count for sync purposes, and finally initializing the effects themselves. Called once on startup.
         * </summary>
         */
        private void RefreshEffects()
        {
            _effects.Clear();
            _maxFrameCount = 0;

            if (effectComponents.Count > 0)
            {
                foreach (var comp in effectComponents)
                {
                    if (comp is IPlaybackEffect effect)
                        _effects.Add(effect);
                }
            }
            else
            {
                var foundEffects = GetComponentsInChildren<IPlaybackEffect>(true);
                _effects.AddRange(foundEffects);
            }

            foreach (var effect in _effects)
            {
                effect.CalculateFrameCount();
                _maxFrameCount = Mathf.Max(_maxFrameCount, effect.FrameCount);
            }

            // Notify effects that implement IFrameContextAware of the frame context
            foreach (var effect in _effects)
            {
                if (effect is IFrameContextAware frameAware)
                    frameAware.SetFrameContext(_maxFrameCount);
            }

            InitializeEntityComponents();
        }

        private void InitializeEntityComponents()
        {
            // Check for anything that might require a PlaybackComponent for extra information and initialize them
            List<IPlaybackComponentUser> componentUsers = _effects.OfType<IPlaybackComponentUser>().ToList();

            foreach (var user in componentUsers)
            {
	            user.BuildComponent(_components, _effects.OfType<MonoBehaviour>().ToList());
            }
        }

        /**
         * <summary>
         * Drives the current state of all effects by updating the percent the effect should be at.
         * Called every update.
         * </summary>
         */
        private void DriveEffects(float percent)
        {
            foreach (var effect in _effects)
            {
                if (effect is MonoBehaviour mb && mb != null && mb.gameObject.activeInHierarchy)
                {
                    // ISubFrameEffect always gets raw continuous percent instead of quantized
                    float effectPercent = (syncMode == PlaybackSyncMode.ScalePercent || effect is ISubFrameEffect)
                        ? percent
                        : CalculateFrameBasedPercent(percent, effect.FrameCount);

                    effect.SetPercent(effectPercent);
                }
            }
        }

        private float CalculateFrameBasedPercent(float globalPercent, int effectFrameCount)
        {
            if (_maxFrameCount == 0) return globalPercent;

            int currentFrame = Mathf.FloorToInt(globalPercent * _maxFrameCount);

            // Clamp frame to this effect's available frames
            int clampedFrame = Mathf.Clamp(currentFrame, 0, Mathf.Max(0, effectFrameCount - 1));

            // Convert back to percent
            return effectFrameCount > 0 ? (float)clampedFrame / effectFrameCount : 0f;
        }

        private void PushStateToViewModel()
        {
            if (viewModel == null) return;

            viewModel.Percent.Value = _currentPercent;
            viewModel.IsPlaying.Value = _isPlaying;
            viewModel.Speed.Value = _speed;
            viewModel.Duration.Value = duration;
            viewModel.MaxFrames.Value = _maxFrameCount;

            var allComponents = new List<IPlaybackComponent>(_components);

            // Rebuild dictionaries with current component state
            var activeSetDict = new Dictionary<int, int>();
            var countDict = new Dictionary<int, int>();

            foreach (var component in allComponents)
            {
                activeSetDict[component.ComponentId] = component.ActiveSetIndex;
                countDict[component.ComponentId] = component.SetCount;
            }

            viewModel.ActiveSetByComponent.Value = activeSetDict;
            viewModel.CountByComponent.Value = countDict;
        }

        private void InitializeViewModel()
        {
	        if (viewModel == null)
	        {
		        viewModel = GetComponent<PlaybackViewModel>();
	        }

            if (viewModel == null)
            {
                // No ViewModel found; local mode won't work with ViewModel
                return;
            }

            viewModel.OnPlayRequested += Play; // local always
            viewModel.OnPauseRequested += Pause; // local always
            viewModel.OnStopRequested += Stop; // local always
            viewModel.OnSeekRequested += (p => { if (_locallyDriven) Seek(p); }); // not local
            viewModel.OnSpeedChangeRequested += SetSpeed; // local always
            viewModel.OnComponentChangeRequested += ((id, index) =>
            {
                if (_locallyDriven)
                {
                    SetComponentIndex(id, index);
                    // Immediately push state back to ViewModel so editor sees the change
                    if (viewModel != null)
                        PushStateToViewModel();
                }
            }); // not local

            viewModel.Percent.AddListener(percent =>
            {
                if (!_locallyDriven)
                {
                    ApplyAnchorPercent(percent);
                }
            });

            viewModel.InitializeConstants(new List<IPlaybackComponent>(_components));

            PushStateToViewModel();
        }

        private void RecalculateFrameCounts()
        {
            _maxFrameCount = 0;

            foreach (var effect in _effects)
            {
                effect.CalculateFrameCount();
                _maxFrameCount = Mathf.Max(_maxFrameCount, effect.FrameCount);
            }

            // Update IFrameContextAware effects with new max frame count
            foreach (var effect in _effects)
            {
                if (effect is IFrameContextAware frameAware)
                    frameAware.SetFrameContext(_maxFrameCount);
            }
        }

        #endregion
    }
}

