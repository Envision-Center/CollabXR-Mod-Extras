using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CollabXR.ModExtras
{
    public class DepthMask : MonoBehaviour
    {
        [NonSerialized] public UnityEvent OnMeshDestroyed = new UnityEvent();
        public List<MeshFilter> filters;

        private void OnDestroy()
        {
            OnMeshDestroyed.Invoke();
        }
    }
}
