using MyProstateRejectsCapitalism.Characters;
using MyProstateRejectsCapitalism.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyProstateRejectsCapitalism.Gameplay
{
    // Shared doorway behavior at the prologue-to-showroom boundary.
    public sealed class PrologueExhibitionDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private FirstPersonPlayer player;
        [SerializeField] private ActOneFreeConversation conversation;
        [SerializeField] private ClassroomToRestroomPrologue prologue;
        [SerializeField] private Transform leaf;
        [SerializeField] private Transform exhibitTarget;
        [SerializeField] private GameObject returnWall;
        [SerializeField] private GameObject frame;
        [SerializeField] private float doorwayZ = 27.4f;
        [SerializeField] private string sceneAfterOpening;
        [SerializeField] private bool arriveOpen;
        [SerializeField] private TMP_Text arrivalThought;

        private bool opening;
        private float observedAt;
        private bool handoffStarted;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal bool IsHandingOff => handoffStarted;
#endif
        public float OpenFraction { get; private set; }
        public bool IsOpen => OpenFraction >= 1f;
        public bool ExhibitionObserved { get; private set; }
        public bool ReturnSealed { get; private set; }
        public string InteractionPrompt => conversation != null && !conversation.IsUnlocked
            ? "잠겨 있다" : opening ? "열리는 중" : "문 열기";

        private void Start()
        {
            returnWall.SetActive(false);
            if (arriveOpen)
            {
                OpenFraction = 1f;
                leaf.localRotation = Quaternion.Euler(0f, -105f, 0f);
            }
        }

        public void Interact(GameObject owner)
        {
            if (opening || IsOpen || owner != player.gameObject || !player.CanControl
                || (conversation != null && !conversation.IsUnlocked))
                return;
            opening = true;
        }

        private void Update()
        {
            if (opening)
            {
                OpenFraction = Mathf.MoveTowards(OpenFraction, 1f, Time.deltaTime / .9f);
                leaf.localRotation = Quaternion.Euler(0f, -105f * Mathf.SmoothStep(0f, 1f, OpenFraction), 0f);
                if (IsOpen)
                    opening = false;
            }
            if (!IsOpen)
                return;

            if (!string.IsNullOrEmpty(sceneAfterOpening))
            {
                if (!handoffStarted && player.transform.position.z > doorwayZ - .02f
                    && player.CanControl && (conversation == null || !conversation.IsTalking))
                {
                    handoffStarted = true;
                    var load = SceneManager.LoadSceneAsync(sceneAfterOpening, LoadSceneMode.Single);
                    if (load != null)
                        player.SetInputBlocked(true);
                }
                return;
            }

            var view = player.ViewCamera;
            var screen = view.WorldToViewportPoint(exhibitTarget.position);
            bool seeingExhibit = screen.z > 0f && screen.x > .1f && screen.x < .9f
                && screen.y > .1f && screen.y < .9f
                && Vector3.Distance(view.transform.position, exhibitTarget.position) < 7f;
            if (!ExhibitionObserved && seeingExhibit && TryObserve())
            {
                ExhibitionObserved = true;
                observedAt = Time.time;
            }
            if (arrivalThought != null && ExhibitionObserved && (Time.time >= observedAt + 2.4f || player.InputBlocked))
                arrivalThought.text = string.Empty;
            // Wait for the whole player to cross and the entrance to leave the camera frustum.
            // No closing animation or visible swap: looking back reveals the same white finish.
            if (ExhibitionObserved && !ReturnSealed && Time.time > observedAt + .3f
                && player.transform.position.z > doorwayZ + 1.25f)
            {
                var bounds = new Bounds(new Vector3(0f, 1.5f, doorwayZ), new Vector3(4f, 3f, .35f));
                if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(view), bounds))
                {
                    returnWall.SetActive(true);
                    frame.SetActive(false);
                    leaf.gameObject.SetActive(false);
                    ReturnSealed = true;
                }
            }
        }

        private bool TryObserve()
        {
            if (prologue != null)
                return prologue.TryShowExhibitionObservation();
            if (!player.CanControl)
                return false;
            if (arrivalThought != null)
                arrivalThought.text = "……여긴 뭐야?";
            return true;
        }
    }
}
