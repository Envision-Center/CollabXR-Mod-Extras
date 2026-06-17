using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    public class LocalTrailManager : MonoBehaviour
    {
        public List<LocalTrailRenderer> trails;
        public void ClearTrail()
        {
            foreach(LocalTrailRenderer trail in trails)
            {
                trail.ClearLine();
            }
        }
    }
}
