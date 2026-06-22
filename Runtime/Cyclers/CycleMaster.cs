using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    public class CycleMaster : MonoBehaviour
    {
        public List<CyclePart> objectCyclers;
        public float duration;
        protected float lastPercent = 0;
        protected bool paused = false;

        private void Update()
        {
            UpdateDisplay();
        }

        public void UpdateMaster()
        {
            if (paused) return;
            CalculatePercent();
        }

        private void UpdateDisplay()
        {
            foreach (CyclePart cycler in objectCyclers)
            {
                cycler.SetFramePercentage(lastPercent);
            }
        }

        public float CalculatePercent()
        {
            float effectiveDuration = duration > 0 ? duration : 0.01f;
            float percent = lastPercent + (Time.deltaTime / effectiveDuration);

            if (percent > 1.0f)
            {
                percent = percent % 1.0f;
            }

            lastPercent = percent;

            return percent;
        }

        public void SetPercent(float targetPercent)
        {
            lastPercent = targetPercent;
        }

        public void TogglePause()
        {
            paused = !paused;
        }

        public void SetPause(bool pause)
        {
            paused = pause;
        }

        public float GetCurrentPercent()
        {
            return lastPercent;
        }

        public void SetCurrentPercent(float percent)
        {
            lastPercent = percent;
        }

        public bool IsPaused()
        {
            return paused;
        }
    }
}
