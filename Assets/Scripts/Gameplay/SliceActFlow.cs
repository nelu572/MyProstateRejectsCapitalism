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
        [SerializeField] private string nextScene;
        [SerializeField] private string firstScene = "Slice_Act01_PaidToilet";
        [SerializeField] private CanvasGroup fade;
        [SerializeField] private bool fadeOnArrival = true;
        [SerializeField] private Canvas gameplayCanvas;
        [SerializeField] private bool endAfterDialogue;
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

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

        public bool Ready => RequirementCompleted && !dialogue.IsPlaying && !IsTransitioning;

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
        }

        private IEnumerator Start()
        {
            if (!fadeOnArrival)
                yield break;
            yield return FadeTo(0f, 0.25f);
            IsTransitioning = false;
            player.SetInputBlocked(false);
        }

        private void Update()
        {
            if (IsEndingVisible)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                    Restart();
                return;
            }

            if (endAfterDialogue && Ready)
                StartCoroutine(ShowEnding());
        }

        public bool TryLeave(FirstPersonPlayer owner)
        {
            if (owner != player || !owner.CanControl || !Ready || IsEndingVisible)
                return false;
            if (!Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError("Next scene is missing from the build: " + nextScene, this);
                return false;
            }

            IsTransitioning = true;
            player.SetInputBlocked(true);
            StartCoroutine(LoadNext(nextScene));
            return true;
        }

        private IEnumerator LoadNext(string sceneName)
        {
            yield return FadeTo(1f, 0.3f);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        private IEnumerator ShowEnding()
        {
            IsTransitioning = true;
            player.SetInputBlocked(true);
            yield return FadeTo(1f, 0.6f);
            gameplayCanvas.enabled = false;
            endingPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            var from = fade.alpha;
            fade.blocksRaycasts = true;
            for (var elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
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
            IsTransitioning = true;
            player.SetInputBlocked(true);
            StartCoroutine(LoadNext(firstScene));
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
