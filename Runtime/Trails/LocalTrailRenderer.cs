using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    public class LocalTrailRenderer : MonoBehaviour
    {
        public int capacity = 50;
        public float pollingFrequency = 0.01f;
        public LineRenderer siblingLR;
        Queue<Vector3> positions;
        Coroutine trailGen;
        // Start is called before the first frame update
        void Start()
        {
            siblingLR.positionCount = capacity;
            siblingLR.useWorldSpace = false;
            ClearLine();
            trailGen = StartCoroutine(AddPosition());
        }

        IEnumerator AddPosition()
        {
            while (true)
            {
                if (positions.Count > capacity)
                {
                    positions.Dequeue();
                }
                positions.Enqueue(transform.localPosition);
                siblingLR.positionCount = positions.Count;
                siblingLR.SetPositions(positions.ToArray());
                yield return new WaitForSeconds(pollingFrequency);
            }
        }

        public void ClearLine()
        {
            positions = new Queue<Vector3>();
            siblingLR.positionCount = 0;
        }
    }
}
