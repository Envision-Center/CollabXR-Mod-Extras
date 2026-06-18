using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CollabXR.ModExtras.Editor
{
    public class PlaybackDirectorEditor : EditorWindow
    {
        private PlaybackDirector _director;
        private PlaybackViewModel _viewModel;
        private Vector2 _scrollPos;

        [MenuItem("CollabXR/Mod Extras/Playback Preview")]
        public static void ShowWindow()
        {
            GetWindow<PlaybackDirectorEditor>("Playback Preview");
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Playback Director", EditorStyles.boldLabel);

            PlaybackDirector newDirector = (PlaybackDirector)EditorGUILayout.ObjectField("Director", _director, typeof(PlaybackDirector), true);

            // If director changed, reset viewmodel reference so we find the new one
            if (newDirector != _director)
            {
                _director = newDirector;
                _viewModel = null;
            }

            if (_director == null)
            {
                EditorGUILayout.HelpBox("Drag a PlaybackDirector into the Director field.", MessageType.Info);
                return;
            }

            // Try to find ViewModel from the director or its children
            if (_viewModel == null)
            {
                _viewModel = _director.GetComponent<PlaybackViewModel>();
                if (_viewModel == null)
                    _viewModel = _director.GetComponentInChildren<PlaybackViewModel>();
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to preview playback.", MessageType.Info);
                return;
            }

            DrawPlaybackControls();
            DrawComponentSelector();
            DrawInfoPanel();
        }

        private void DrawPlaybackControls()
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);

            if (_director == null) return;

            // Display current frame counter and max frames when in frame-sync mode
            if (_viewModel != null)
            {
                int currentFrame = _director.CurrentFrame;
                int maxFrame = _director.MaxFrameCount;

                EditorGUILayout.LabelField($"Frame: {currentFrame} / {maxFrame}", EditorStyles.miniLabel);
            }

            // Frame snapping slider for frame-sync mode
            if (_director.SyncMode == PlaybackSyncMode.SyncByFrame && _director.MaxFrameCount > 0)
            {
                EditorGUI.BeginChangeCheck();
                int currentFrame = _director.CurrentFrame;
                int newFrame = EditorGUILayout.IntSlider("Frame", currentFrame, 0, _director.MaxFrameCount - 1);
                if (EditorGUI.EndChangeCheck())
                {
                    _director.SeekToFrame(newFrame);
                    EditorUtility.SetDirty(_director);
                }
            }
            else
            {
                // Regular percent-based slider for scale-percent mode
                EditorGUI.BeginChangeCheck();
                float currentPercent = _viewModel != null ? _viewModel.Percent.Value : _director.CurrentPercent;
                float newPercent = EditorGUILayout.Slider("Position", currentPercent, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_viewModel != null)
                        _viewModel.RequestSeek(newPercent);
                    else
                        _director.Seek(newPercent);
                    EditorUtility.SetDirty(_director);
                }
            }

            EditorGUILayout.BeginHorizontal();
            bool isPlaying = _viewModel != null ? _viewModel.IsPlaying.Value : _director.IsPlaying;
            if (GUILayout.Button(isPlaying ? "Pause" : "Play", GUILayout.Height(24)))
            {
                if (_viewModel != null)
                {
                    if (isPlaying)
                        _viewModel.RequestPause();
                    else
                        _viewModel.RequestPlay();
                }
                else
                {
                    if (_director.IsPlaying)
                        _director.Pause();
                    else
                        _director.Play();
                }
                EditorUtility.SetDirty(_director);
            }

            if (GUILayout.Button("Stop", GUILayout.Height(24)))
            {
                if (_viewModel != null)
                    _viewModel.RequestStop();
                else
                    _director.Stop();
                EditorUtility.SetDirty(_director);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            float currentSpeed = _viewModel != null ? _viewModel.Speed.Value : _director.Speed;
            float newSpeed = EditorGUILayout.Slider("Speed", currentSpeed, 0.1f, 3f);
            if (EditorGUI.EndChangeCheck())
            {
                if (_viewModel != null)
                    _viewModel.RequestSetSpeed(newSpeed);
                else
                    _director.SetSpeed(newSpeed);
                EditorUtility.SetDirty(_director);
            }
        }

        private void DrawComponentSelector()
        {
            if (_viewModel == null || _viewModel.CountByComponent.Value == null || _viewModel.CountByComponent.Value.Count == 0)
                return;

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);

            var countByComponent = _viewModel.CountByComponent.Value;
            var activeSetByComponent = _viewModel.ActiveSetByComponent.Value;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < countByComponent.Count; i++)
			{
				var kvp = countByComponent.ElementAt(i);
                int componentId = kvp.Key;
                int count = kvp.Value;
                int currentIndex = activeSetByComponent.ContainsKey(componentId) ? activeSetByComponent[componentId] : 0;

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.IntSlider($"Component {componentId}", currentIndex, 0, count - 1);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_viewModel != null)
                    {
                        _viewModel.RequestSetComponentSet(componentId, newIndex);
                        EditorUtility.SetDirty(_director);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawInfoPanel()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);

            float duration = _viewModel != null ? _viewModel.Duration.Value : _director.Duration;
            float currentPercent = _viewModel != null ? _viewModel.Percent.Value : _director.CurrentPercent;
            bool isPlaying = _viewModel != null ? _viewModel.IsPlaying.Value : _director.IsPlaying;

            EditorGUILayout.LabelField($"Duration: {duration:F2}s");
            EditorGUILayout.LabelField($"Current Time: {currentPercent * duration:F2}s");
            EditorGUILayout.LabelField($"Playing: {isPlaying}");
            EditorGUILayout.LabelField($"Sync Mode: {_director.SyncMode}");
            EditorGUILayout.LabelField($"Max Frames: {_director.MaxFrameCount}");
        }

        private void Update()
        {
            if (EditorApplication.isPlaying && _director != null)
            {
                Repaint();
            }
        }
    }
}

