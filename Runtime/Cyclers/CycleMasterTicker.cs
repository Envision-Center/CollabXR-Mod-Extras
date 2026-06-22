using System.Collections;
using UnityEngine;

namespace CollabXR.ModExtras
{
	[System.Obsolete("Deprecated; all systems refactored into PlaybackDirector.")]
    [ExecuteAlways]
    public class CycleMasterTicker : MonoBehaviour
    {
        [SerializeField] private CycleMaster target;

        [Min(0.01f)] public float interval = 0.25f;

        [Range(0f, 1f)] public float step = 0.05f;

        [Range(0f, 1f)] public float percent = 0f;


        [SerializeField] private ToggleController toggleController; // Attached or some as target probably



        private Coroutine loop;
        private int activeIndex = 0;

        void Reset()
        {
            if (!target)
                target = GetComponent<CycleMaster>() ?? GetComponentInChildren<CycleMaster>();

            if (!toggleController)
                toggleController = GetComponent<ToggleController>() ?? GetComponentInChildren<ToggleController>();
        }

        void OnEnable()
        {
            if (Application.isPlaying)
                StartTicking();

        }

        void OnDisable()
        {
            if (Application.isPlaying)
                StopTicking();
        }

        public void StartTicking()
        {
            if (loop == null)
                loop = StartCoroutine(TickLoop());
        }

        public void StopTicking()
        {
            if (loop != null)
            {
                StopCoroutine(loop);
                loop = null;
            }
        }

        //Probably should replace with update
        IEnumerator TickLoop()
        {
            var wait = new WaitForSeconds(interval);
            while (true)
            {
                Tick();
                yield return wait;
            }
        }

        void Tick()
        {
            if (!target) return;

            if (step > 0f)
            {
                percent += step;
                if (percent > 1f) percent -= 1f;
            }

            target.SetPercent(percent);
            UpdateFrameDisplay();
        }


        public void UpdateFrameDisplay()
        {
            if (!target) return;

            // Show visible datasets
            if (toggleController && toggleController.toggleableChildren.Count > 0)
            {
                for (int i = 0; i < toggleController.toggleableChildren.Count; i++)
                {
                    var t = toggleController.toggleableChildren[i];
                    bool shouldBeOn = (i == activeIndex);
                    t.Toggle(shouldBeOn);
                }
            }

            // Set teh frame
            foreach (var cycler in target.objectCyclers)
            {
                if (cycler is ObjectCyclePart part && part.gameObject.activeSelf)
                    part.SetPercent(percent);
            }
        }



        public void SetActiveIndex(int index)
        {
            activeIndex = Mathf.Clamp(index, 0, toggleController.toggleableChildren.Count - 1);
            UpdateFrameDisplay();
        }

        public void SetPercent(float p)
        {
            percent = Mathf.Clamp01(p);
            target.SetPercent(percent);
            UpdateFrameDisplay();
        }


    }
}
