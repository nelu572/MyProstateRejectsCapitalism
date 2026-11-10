using UnityEngine;

namespace MyProstateRejectsCapitalism.Gameplay
{
    internal enum ProstateLimitGaugeLayout
    {
        Vertical,
        Horizontal
    }

    [CreateAssetMenu(menuName = "Game/Prostate Limit Settings")]
    public sealed class ProstateLimitSettings : ScriptableObject
    {
        [SerializeField, Min(1f)] private float playableSeconds = 180f;
        [SerializeField] private ProstateLimitGaugeLayout gaugeLayout = ProstateLimitGaugeLayout.Vertical;
        [SerializeField, Range(0f, 1f)] private float vignetteIntensity = .58f;
        [SerializeField] private Color vignetteColor = new(1f, .80f, .08f);
        [SerializeField] private Material failureLiquidMaterial;

        internal float PlayableSeconds => playableSeconds;
        internal ProstateLimitGaugeLayout GaugeLayout => gaugeLayout;
        internal float VignetteIntensity => vignetteIntensity;
        internal Color VignetteColor => vignetteColor;
        internal Material FailureLiquidMaterial => failureLiquidMaterial;
    }
}
