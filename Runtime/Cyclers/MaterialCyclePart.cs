using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    public class MaterialCyclePart : CyclePart
    {
        public List<Texture3D> textures;
        public MeshRenderer toCycle;
        public float transparency;
        Material copyMaterial;
        bool initialized = false;
        // Start is called before the first frame update
        void Initialize()
        {
            copyMaterial = toCycle.material;
            initialized = true;
        }
        public override void SetFramePercentage(float frameRatio) // set frame to display as a ratio of the total frame count (0.0 to 1.0)
        {
            if (!initialized) Initialize();
            int frame = GetFrame(frameRatio);
            int actualLastFrame = Mathf.FloorToInt(lastFrame);
            if(frame != actualLastFrame)
            {
                copyMaterial.SetTexture("_MainTex", textures[frame]);
                copyMaterial.SetFloat("_AlphaFactor", transparency);
                toCycle.materials[0] = copyMaterial;
            }
            base.SetFramePercentage(frameRatio);
        }

        public void SetFrameTransparency(float transparency)
        {
            if (!initialized) Initialize();
            this.transparency = transparency;
            copyMaterial.SetFloat("_AlphaFactor", transparency);
            toCycle.materials[0] = copyMaterial;
        }

        public override int GetFrameCount()
        {
            return textures.Count;
        }
    }
}
