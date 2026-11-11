using System;
using System.Collections;
using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.Gameplay
{
    public sealed class SliceActFlow : MonoBehaviour
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private DialogueController dialogue;
        [SerializeField] private PaidToiletEvent paidToilet;
        [SerializeField] private LevelExperimentInteraction[] observations;
        [SerializeField] private bool allowExitWithoutObservation;
        [SerializeField] private string nextScene;
        [SerializeField] private string firstScene = "Slice_Act01_PaidToilet";
        [SerializeField] private CanvasGroup fade;
        [SerializeField] private bool fadeOnArrival = true;
        [SerializeField] private Canvas gameplayCanvas;
        [SerializeField] private bool endAfterDialogue;
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private ProstateLimitSettings limitSettings;
        [SerializeField] private bool restartOnFall;
        [SerializeField] private float fallBelowY = -2.75f;

        public bool IsTransitioning { get; private set; }
        public bool IsEndingVisible => endingPanel != null && endingPanel.activeSelf;
        public bool RequirementCompleted
        {
            get
            {
                if (paidToilet != null && paidToilet.HasBeenUsed)
                    return true;
                if (observations != null)
                    foreach (var observation in observations)
                        if (observation != null && observation.HasBeenUsed)
                            return true;
                return false;
            }
        }

        public bool Ready => (allowExitWithoutObservation || RequirementCompleted)
            && !dialogue.IsInteractive && !IsTransitioning;

        internal bool TracksLimit => !IsTransitioning && !IsEndingVisible
            && !(endAfterDialogue && RequirementCompleted);

        private const string FinalToiletScene = "Slice_Act05_FinalToilet";
        private static AsyncOperation developmentJump;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDevelopmentJump() => developmentJump = null;

        internal static bool TryJumpToFinalToilet(FirstPersonPlayer owner)
        {
            if (developmentJump != null || !Application.CanStreamedLevelBeLoaded(FinalToiletScene))
                return false;

            var flow = FindFirstObjectByType<SliceActFlow>();
            if ((flow != null && flow.IsTransitioning && (!flow.IsEndingVisible || !flow.restartButton.interactable))
                || FindFirstObjectByType<PrologueExhibitionDoor>()?.IsHandingOff == true
                || owner.GetComponent<UI.PauseMenu>()?.IsReturningToTitle == true)
                return false;

            developmentJump = SceneManager.LoadSceneAsync(FinalToiletScene, LoadSceneMode.Single);
            if (developmentJump == null)
                return false;

            owner.SetInputBlocked(true);
            if (flow != null)
            {
                flow.IsTransitioning = true;
                if (flow.IsEndingVisible)
                {
                    flow.restartButton.interactable = false;
                    flow.quitButton.interactable = false;
                }
            }
            developmentJump.completed += PlaceAtFinalToilet;
            return true;
        }

        private static void PlaceAtFinalToilet(AsyncOperation operation)
        {
            developmentJump = null;
            Time.timeScale = 1f;
            var flow = FindFirstObjectByType<SliceActFlow>();
            if (flow == null || flow.gameObject.scene.name != FinalToiletScene
                || flow.observations is not { Length: 1 } || flow.observations[0] == null
                || !flow.observations[0].TryGetComponent<Collider>(out var toilet))
                return;

            // Stand inside the room, beyond the arrival dialogue trigger and clear of the bowl.
            var target = toilet.bounds.center;
            var position = target + Vector3.back * 1.55f;
            position.y = flow.player.transform.position.y;
            flow.player.PlaceForFinalToiletTest(position, target);
        }
        private void Awake()
        {
            IsTransitioning = fadeOnArrival;
            if (fadeOnArrival)
                player.SetInputBlocked(true);
            fade.alpha = fadeOnArrival ? 1f : 0f;
            fade.blocksRaycasts = fadeOnArrival;
            if (endingPanel != null)
                endingPanel.SetActive(false);
            if (restartButton != null)
                restartButton.onClick.AddListener(Restart);
            if (quitButton != null)
                quitButton.onClick.AddListener(Quit);
            if (limitSettings != null)
                gameObject.AddComponent<ProstateLimit>().Initialize(this, player, dialogue, limitSettings,
                    restartOnFall, fallBelowY);
        }

        private IEnumerator Start()
        {
            if (!fadeOnArrival)
                yield break;
            // Scene activation may consume an entire frame. Draw the destination black
            // before starting the reveal, and do not charge stalled frames to that fade.
            yield return null;
            yield return FadeTo(fade, 0f, 0.25f, limitFrameStep: true);
            IsTransitioning = false;
            player.SetInputBlocked(false);
        }

        private void Update()
        {
            if (player.IsPaused)
                return;

            if (IsEndingVisible)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                    Restart();
                return;
            }

            if (endAfterDialogue && Ready)
                StartCoroutine(ShowEnding());
        }

        public bool TryLeave(FirstPersonPlayer owner) => Ready && TryTransition(owner, nextScene);

        internal bool TryTransition(FirstPersonPlayer owner, string sceneName)
        {
            if (owner != player || !owner.CanControl || dialogue.IsInteractive || IsTransitioning || IsEndingVisible)
                return false;
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("Next scene is missing from the build: " + sceneName, this);
                return false;
            }

            dialogue.StopAutomatic();
            IsTransitioning = true;
            player.SetInputBlocked(true);
            StartCoroutine(LoadNext(fade, sceneName));
            return true;
        }

        internal static IEnumerator LoadNext(CanvasGroup fade, string sceneName, IEnumerator leadIn = null,
            Action onCovered = null)
        {
            if (leadIn != null)
                yield return leadIn;
            if (fade.alpha < 1f)
                yield return FadeTo(fade, 1f, 0.3f, limitFrameStep: true);
            // Present the fully covered source before Unity begins loading/activating.
            yield return null;
            onCovered?.Invoke();
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        internal bool TryRecover(string sceneName, Func<CanvasGroup, IEnumerator> leadIn = null, Action onCovered = null)
        {
            if (!TracksLimit || player.IsPaused || !Application.CanStreamedLevelBeLoaded(sceneName))
                return false;

            IsTransitioning = true;
            dialogue.enabled = false;
            player.SetInputBlocked(true);
            if (fade.transform.parent == gameplayCanvas.transform)
            {
                foreach (Transform item in gameplayCanvas.transform)
                    if (item != fade.transform)
                        item.gameObject.SetActive(false);
                fade.transform.SetAsLastSibling();
            }
            else
            {
                gameplayCanvas.enabled = false;
            }
            StartCoroutine(LoadNext(fade, sceneName, leadIn?.Invoke(fade), onCovered));
            return true;
        }

        private IEnumerator ShowEnding()
        {
            IsTransitioning = true;
            player.SetInputBlocked(true);
            yield return FadeTo(fade, 1f, 0.6f);
            gameplayCanvas.enabled = false;
            endingPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        internal static IEnumerator FadeTo(CanvasGroup fade, float target, float seconds, bool limitFrameStep = false)
        {
            var from = fade.alpha;
            fade.blocksRaycasts = true;
            for (var elapsed = 0f; elapsed < seconds;
                elapsed += limitFrameStep ? Mathf.Min(Time.unscaledDeltaTime, 1f / 30f) : Time.unscaledDeltaTime)
            {
                fade.alpha = Mathf.Lerp(from, target, elapsed / seconds);
                yield return null;
            }
            fade.alpha = target;
            fade.blocksRaycasts = target > 0f;
        }

        public void Restart()
        {
            if (!IsEndingVisible || !restartButton.interactable)
                return;
            if (!Application.CanStreamedLevelBeLoaded(firstScene))
            {
                Debug.LogError("First scene is missing from the build: " + firstScene, this);
                return;
            }

            restartButton.interactable = false;
            quitButton.interactable = false;
            ProstateLimit.ResetRun();
            IsTransitioning = true;
            player.SetInputBlocked(true);
            // The receipt starts above the dark backdrop. Cover it when leaving the ending.
            fade.transform.SetAsLastSibling();
            fade.alpha = 0f;
            StartCoroutine(LoadNext(fade, firstScene));
        }

        public void Quit()
        {
            if (!IsEndingVisible || !quitButton.interactable)
                return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
