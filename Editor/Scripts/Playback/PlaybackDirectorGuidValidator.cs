namespace CollabXR.ModExtras
{
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	[InitializeOnLoad]
	public static class PlaybackDirectorGuidValidator
	{
		static PlaybackDirectorGuidValidator()
		{
			EditorApplication.delayCall += ValidateGuids;
		}

		private static void ValidateGuids()
		{
			var directors = Object.FindObjectsByType<PlaybackDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			var used = new HashSet<string>();

			foreach (var director in directors)
			{
				if (string.IsNullOrEmpty(director.EditorGuid) || used.Contains(director.EditorGuid))
				{
					var field = typeof(PlaybackDirector).GetField("_editorGuid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

					field.SetValue(director, GUID.Generate().ToString());

					EditorUtility.SetDirty(director);
				}

				used.Add(director.EditorGuid);
			}
		}
	}
}
