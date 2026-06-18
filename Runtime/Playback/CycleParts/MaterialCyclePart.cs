using System.Collections.Generic;
using UnityEngine;

namespace CollabXR.ModExtras
{
    /** <summary>
     * Swaps Texture3D on a MeshRenderer per frame.
     * </summary> */
    public class MaterialCyclePart : CyclePart
    {
        private static readonly int MainTexHash = Shader.PropertyToID("_MainTex");
        private static readonly int AlphaFactorHash = Shader.PropertyToID("_AlphaFactor");

        [Header("Material Cycle Settings")]
        [Tooltip("The textures to cycle through. Each frame will swap the material's main texture to the corresponding Texture3D in this list.")]
        [SerializeField] private List<Texture3D> textures;
        [Tooltip("The MeshRenderer whose material will be modified to cycle through the textures. The script will modify a copy of the material, so the original material will not be affected.")]
        [SerializeField] private MeshRenderer toCycle;
        [Tooltip("The alpha transparency to apply to the material. This will be set on the material's _AlphaFactor property, which should be used in the shader to modulate the final alpha. Changing this value at runtime will update the material immediately without affecting the current frame.")]
        [SerializeField] private float transparency = 1f;

        private Material _copyMaterial;
        private bool _initialized = false;

        public override int FrameCount => textures?.Count ?? 0; // CalculateFrameCount() unnecessary

        private void Initialize()
        {
            if (toCycle != null)
                _copyMaterial = toCycle.material;
            _initialized = true;
        }

        public override void SetPercent(float normalizedPercent)
        {
            if (!_initialized) Initialize();
            if (textures == null || textures.Count == 0 || _copyMaterial == null) return;

            int frame = GetFrame(normalizedPercent);
            if (frame == lastFrame) return;

            _copyMaterial.SetTexture(MainTexHash, textures[frame]);
            _copyMaterial.SetFloat(AlphaFactorHash, transparency);
            if (toCycle != null) toCycle.materials[0] = _copyMaterial;

            base.SetPercent(normalizedPercent);
        }

        /** <summary>Changes transparency without affecting the current frame.</summary> */
        public void SetFrameTransparency(float alpha)
        {
            if (!_initialized) Initialize();
            transparency = alpha;
            if (_copyMaterial != null)
            {
                _copyMaterial.SetFloat(AlphaFactorHash, transparency);
                if (toCycle != null) toCycle.materials[0] = _copyMaterial;
            }
        }

        public void AddTextures(IEnumerable<Texture3D> moreTextures)
        {
	        textures.AddRange(moreTextures);
	        CalculateFrameCount();
        }
    }
}
