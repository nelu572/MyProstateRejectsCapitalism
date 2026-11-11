using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using MyProstateRejectsCapitalism.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        private FirstPersonPlayer player;
        private ExhibitionInspection inspection;
        private SliceActFlow flow;
        private ClassroomToRestroomPrologue prologue;
        private ActOneFirstEncounter encounter;
        private DialogueController dialogue;
        private Canvas hudCanvas;
        private Canvas dialogueCanvas;
        private bool restoreHudCanvas;
        private bool restoreDialogueCanvas;
        private RectTransform surface;
        private RectTransform front;
        private SettingsMotion settings;
        private Image backdrop;
        private static readonly Color SettingsShade = new(0f, .03f, .025f, .72f);
        private float previousTimeScale;
        private bool returningToTitle;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal bool IsReturningToTitle => returningToTitle;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForGame()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Install(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Install(scene);

        private static void Install(Scene scene)
        {
            if (scene.name != TitleMenu.StartScene && scene.name != "Slice_Act03_Advertising"
                && scene.name != "Slice_Act04_Detour" && scene.name != "Slice_Act05_FinalToilet")
                return;

            foreach (var root in scene.GetRootGameObjects())
            {
                var owner = root.GetComponentInChildren<FirstPersonPlayer>();
                if (owner == null)
                    continue;
                if (!owner.TryGetComponent<PauseMenu>(out _))
                    owner.gameObject.AddComponent<PauseMenu>();
                return;
            }
        }

        private void Awake()
        {
            player = GetComponent<FirstPersonPlayer>();
            inspection = GetComponent<ExhibitionInspection>();
            flow = FindFirstObjectByType<SliceActFlow>();
            prologue = FindFirstObjectByType<ClassroomToRestroomPrologue>();
            encounter = FindFirstObjectByType<ActOneFirstEncounter>();
            dialogue = FindFirstObjectByType<DialogueController>();
            hudCanvas = FindFirstObjectByType<PrototypeHud>()?.GetComponentInParent<Canvas>();
            dialogueCanvas = dialogue?.GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (returningToTitle || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;
            if (inspection != null && inspection.SuppressesPause)
                return;

            if (player.IsPaused)
            {
                if (settings.gameObject.activeSelf)
                    CloseSettings();
                else
                    Resume();
            }
            else if ((flow == null || (!flow.IsTransitioning && !flow.IsEndingVisible))
                && (prologue == null || prologue.WakeComplete)
                && (encounter == null || !encounter.IsPaying))
                Pause();
        }

        private void Pause()
        {
            if (surface == null)
                Build();
            previousTimeScale = Time.timeScale;
            player.SetPaused(true);
            Time.timeScale = 0f;
            restoreHudCanvas = HideCanvas(hudCanvas);
            restoreDialogueCanvas = HideCanvas(dialogueCanvas);
            surface.gameObject.SetActive(true);
        }

        private void Resume()
        {
            if (returningToTitle)
                return;
            MenuSettings.Save();
            surface.gameObject.SetActive(false);
            Time.timeScale = previousTimeScale;
            RestoreGameplayPresentation();
            player.SetPaused(false);
        }

        private static bool HideCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.enabled)
                return false;
            canvas.enabled = false;
            return true;
        }

        private void RestoreGameplayPresentation()
        {
            if (restoreHudCanvas && hudCanvas != null)
                hudCanvas.enabled = true;
            if (restoreDialogueCanvas && dialogueCanvas != null)
                dialogueCanvas.enabled = true;
            restoreHudCanvas = restoreDialogueCanvas = false;
        }

        private void Build()
        {
            var font = Resources.Load<TMP_FontAsset>(TitleMenu.FontResource);
            var layout = MenuUi.CreateCanvas("Pause UI", 100, out surface);
            MenuUi.Shade(surface, PauseMenuArt.PauseShade);
            backdrop = surface.Find("Backdrop").GetComponent<Image>();
            MenuUi.EnsureEventSystem(surface);
            front = PauseMenuArt.Build(layout, font, Resume, OpenSettings, ReturnToTitle);
            settings = MenuUi.BuildSettings(layout, font, player.MouseSensitivity,
                dialogue == null ? MenuSettings.DefaultSubtitleSpeed : dialogue.SubtitleSpeed, CloseSettings)
                .gameObject.AddComponent<SettingsMotion>();
            settings.gameObject.SetActive(false);
        }

        private void OpenSettings()
        {
            if (returningToTitle || settings.gameObject.activeSelf)
                return;
            backdrop.color = SettingsShade;
            front.gameObject.SetActive(false);
            settings.Show();
        }

        private void CloseSettings()
        {
            if (settings.TryClose(() =>
                {
                    backdrop.color = PauseMenuArt.PauseShade;
                    front.gameObject.SetActive(true);
                }))
                MenuSettings.Save();
        }

        private void ReturnToTitle()
        {
            if (returningToTitle)
                return;
            if (!Application.CanStreamedLevelBeLoaded(TitleMenu.SceneName))
            {
                Debug.LogError("The title screen is missing from the build scene list.", this);
                return;
            }
            returningToTitle = true;
            MenuSettings.Save();

            // Cover the pause canvas itself while gameplay and its time scale remain paused.
            var curtain = MenuUi.Rect(surface, "Return to title fade", 0f, 0f, 0f, 0f);
            curtain.anchorMin = Vector2.zero;
            curtain.anchorMax = Vector2.one;
            curtain.gameObject.AddComponent<Image>().color = Color.black;
            var fade = curtain.gameObject.AddComponent<CanvasGroup>();
            fade.alpha = 0f;
            StartCoroutine(SliceActFlow.LoadNext(fade, TitleMenu.SceneName));
        }

        private void OnDestroy()
        {
            if (returningToTitle)
                Time.timeScale = 1f;
            else if (player != null && player.IsPaused)
                Time.timeScale = previousTimeScale;
            RestoreGameplayPresentation();
            if (surface != null)
                Destroy(surface.gameObject);
        }
    }
}
