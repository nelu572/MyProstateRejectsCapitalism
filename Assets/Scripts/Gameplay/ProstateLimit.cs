using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MyProstateRejectsCapitalism.Gameplay
{
    [DefaultExecutionOrder(350)]
    internal sealed class ProstateLimit : MonoBehaviour
    {
        private const string VolumeLayer = "Prostate Limit FX";
        private static readonly float[] CheatLevels = { 1f, .5f, .15f, .05f, 0f };
        private static float remaining = 1f;
        private static int cheatLevel;

        private SliceActFlow flow;
        private FirstPersonPlayer player;
        private DialogueController dialogue;
        private ExhibitionInspection inspection;
        private ProstateLimitSettings settings;
        private PrototypeHud hud;
        private Volume volume;
        private VolumeProfile profile;
        private Vignette vignette;
        private ChromaticAberration chromatic;
        private Camera viewCamera;
        private Quaternion unrolledRotation;
        private bool rollApplied;
        private float pressureRoll;
        private ProstateFailureSequence failure;
        private float actEntry;
        private bool restartOnFall;
        private float fallBelowY;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetRun()
        {
            remaining = 1f;
            cheatLevel = 0;
            CompanionAutoDialogue.ResetRun();
        }

        private static bool TryAdvanceCheatLimit()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.ctrlKey.isPressed || !keyboard.shiftKey.isPressed
                || !keyboard.f8Key.wasPressedThisFrame || keyboard.f9Key.wasPressedThisFrame)
                return false;

            cheatLevel = (cheatLevel + 1) % CheatLevels.Length;
            remaining = CheatLevels[cheatLevel];
            return true;
        }

#if UNITY_EDITOR
        internal static void SetRemainingForReview(float value) => remaining = Mathf.Clamp01(value);
#endif

        internal void Initialize(SliceActFlow owner, FirstPersonPlayer controlledPlayer,
            DialogueController subtitles, ProstateLimitSettings tuning, bool recoverFalls, float minimumY)
        {
            flow = owner;
            player = controlledPlayer;
            dialogue = subtitles;
            settings = tuning;
            restartOnFall = recoverFalls;
            fallBelowY = minimumY;
            actEntry = remaining;
        }

        private void Start()
        {
            hud = FindFirstObjectByType<PrototypeHud>();
            inspection = player.GetComponent<ExhibitionInspection>();
            int layer = LayerMask.NameToLayer(VolumeLayer);
            if (layer < 0)
            {
                Debug.LogError("Prostate Limit FX volume layer is missing.", this);
                return;
            }

            viewCamera = player.ViewCamera;
            var cameraData = viewCamera.GetUniversalAdditionalCameraData();
            int previousMask = cameraData.renderPostProcessing ? cameraData.volumeLayerMask.value : 0;
            cameraData.volumeLayerMask = previousMask | (1 << layer);
            cameraData.renderPostProcessing = true;

            var effect = new GameObject("Prostate pressure effects") { layer = layer };
            effect.transform.SetParent(transform, false);
            volume = effect.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;
            volume.weight = 0f;
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;
            vignette = profile.Add<Vignette>();
            vignette.color.Override(settings.VignetteColor);
            vignette.intensity.Override(0f);
            vignette.smoothness.Override(.46f);
            vignette.rounded.Override(true);
            chromatic = profile.Add<ChromaticAberration>();
            chromatic.intensity.Override(0f);
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
        }

        private void LateUpdate()
        {
            bool tracksLimit = flow.TracksLimit;
            bool inspecting = inspection != null && inspection.SuppressesPause;
            if (tracksLimit && !player.IsPaused && restartOnFall && player.transform.position.y < fallBelowY
                && flow.TryRecover(gameObject.scene.name))
            {
                remaining = actEntry;
                tracksLimit = false;
            }

            if (tracksLimit && player.CanControl && !dialogue.IsPlaying && !inspecting)
            {
                if (!TryAdvanceCheatLimit())
                    remaining = Mathf.Max(0f, remaining - Time.deltaTime / Mathf.Max(1f, settings.PlayableSeconds));
                if (remaining <= 0f)
                {
                    var sequence = new ProstateFailureSequence(transform, player, settings.FailureLiquidMaterial);
                    if (flow.TryRecover(TitleMenu.StartScene, sequence.Play, ResetRun))
                    {
                        failure = sequence;
                        tracksLimit = false;
                    }
                }
            }

            bool visible = tracksLimit && !player.IsPaused && !inspecting;
            if (hud != null)
                hud.SetProstateLimit(visible && (!player.InputBlocked || dialogue.IsPlaying), remaining,
                    settings.GaugeLayout);
            UpdatePressureEffects(visible && player.CanControl && !dialogue.IsPlaying);
        }

        private void UpdatePressureEffects(bool playing)
        {
            float pressure = playing ? Mathf.InverseLerp(.72f, .05f, remaining) : 0f;
            float colorSplit = playing && remaining <= .5f
                ? Mathf.Lerp(.035f, .46f, Mathf.InverseLerp(.50f, .05f, remaining)) : 0f;
            float breath = playing && remaining <= .15f
                ? Mathf.Lerp(.16f, .68f, Mathf.InverseLerp(.15f, .05f, remaining)) : 0f;
            float time = Time.unscaledTime;
            pressureRoll = Mathf.Sin(time * 4.8f) * breath;

            float vignetteAmount = settings.VignetteIntensity * pressure;
            if (failure != null)
            {
                vignetteAmount = Mathf.Lerp(settings.VignetteIntensity, .84f, failure.PressureSurge);
                colorSplit = Mathf.Lerp(.46f, 1f, failure.PressureSurge) * (1f - failure.Blackout);
                float catchBreath = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.16f, .72f, failure.Elapsed));
                pressureRoll = Mathf.Sin(failure.Elapsed * 20f) * catchBreath * 1.45f;
            }
            if (volume == null)
                return;

            vignette.active = vignetteAmount > 0f;
            vignette.intensity.value = vignetteAmount;
            chromatic.active = colorSplit > 0f;
            chromatic.intensity.value = colorSplit;
            volume.weight = vignette.active || chromatic.active ? 1f : 0f;
        }

        // Roll the rendered view only. Interaction rays and inspection poses keep their authored rotation.
        private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != viewCamera || pressureRoll == 0f || rollApplied)
                return;
            unrolledRotation = camera.transform.localRotation;
            camera.transform.localRotation = unrolledRotation * Quaternion.AngleAxis(pressureRoll, Vector3.forward);
            rollApplied = true;
        }

        private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != viewCamera || !rollApplied)
                return;
            camera.transform.localRotation = unrolledRotation;
            rollApplied = false;
        }

        private void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
            if (viewCamera != null && rollApplied)
                viewCamera.transform.localRotation = unrolledRotation;
            if (vignette != null)
                Destroy(vignette);
            if (chromatic != null)
                Destroy(chromatic);
            if (profile != null)
                Destroy(profile);
            failure?.Dispose();
        }
    }
}
