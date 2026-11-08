using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MyProstateRejectsCapitalism.UI
{
    public sealed class TitleMenu : MonoBehaviour
    {
        internal const string SceneName = "Title_MainMenu";
        internal const string StartScene = "Prologue_ClassroomToRestroom_Validation";
        internal const string FontResource = "Menus/MenuKorean";
        [SerializeField] private TMP_FontAsset approvedFont;
        [SerializeField] private Camera monumentCamera;
        [SerializeField] private GameObject approvedLogo;
        [SerializeField] private MonumentTurntable turntable;
        [SerializeField] private Transform terminalMount;
        [SerializeField] private Transform[] insetSteps;
        private RectTransform front;
        private RectTransform settings;
        private TitleLettering lettering;
        private GameObject monumentPanel;
        private bool loading;

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var font = approvedFont != null ? approvedFont : Resources.Load<TMP_FontAsset>(FontResource);
            var layout = MenuUi.CreateCanvas("Title UI", 30, out var surface);
            MenuUi.EnsureEventSystem(surface);
            if (terminalMount != null)
            {
                var start = TitleStartTransition.Bind(this, surface, turntable, CanStart, StartGame);
                monumentPanel = RecessedTitlePanel.Build(terminalMount, insetSteps, monumentCamera, font,
                    start, OpenSettings, Quit, polished: true);
            }
            else
            {
                // The previous title scene remains usable as a recovery copy.
                lettering = GetComponent<TitleLettering>();
                lettering.Build(font);
                front = MenuUi.Rect(layout, "Title page", 0, 0, 1920, 1080);
                BuildTitle(font);
            }
            settings = MenuUi.BuildSettings(layout, font, MenuSettings.Sensitivity(MenuSettings.DefaultSensitivity),
                MenuSettings.SubtitleSpeed(MenuSettings.DefaultSubtitleSpeed), CloseSettings);
            settings.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (settings.gameObject.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseSettings();
        }

        private void BuildTitle(TMP_FontAsset font)
        {
            MenuUi.Text(front, font, "공중위생 관리과", 91, 58, 440, 35, 23, MenuUi.Mint);
            MenuUi.Panel(front, "Copper stamp rule", 91, 101, 184, 3, MenuUi.Amber, MenuPlate.Shape.Rectangle);
            MenuUi.Text(front, font, "PUBLIC TOILET  /  05-A", 91, 112, 420, 32, 17, MenuUi.Paper);

            var controls = MenuUi.Panel(front, "Payment control strip", 85, 882, 970, 136,
                new Color32(18, 33, 31, 243), MenuPlate.Shape.Ticket);
            MenuUi.Panel(controls, "Receipt stripe", 14, 18, 4, 101, MenuUi.Amber, MenuPlate.Shape.Rectangle);
            MenuUi.Button(controls, font, "게임 시작", 36, 28, 406, 82, MenuUi.Amber, MenuUi.Ink,
                MenuPlate.Shape.Flush, StartGame);
            MenuUi.Button(controls, font, "설정", 466, 33, 211, 73, MenuUi.Plate, MenuUi.Paper,
                MenuPlate.Shape.Terminal, OpenSettings);
            MenuUi.Button(controls, font, "종료", 703, 38, 162, 62, new Color32(93, 87, 69, 255),
                MenuUi.Paper, MenuPlate.Shape.Ticket, Quit);
            MenuUi.Text(controls, font, "05", 885, 38, 60, 62, 26, MenuUi.Mint, TextAlignmentOptions.Center);
            MenuUi.Text(front, font, "수납 확인 전 사용 금지", 1335, 943, 458, 52, 24, MenuUi.Paper,
                TextAlignmentOptions.MidlineRight);
        }

        private void OpenSettings()
        {
            ShowFront(false);
            settings.gameObject.SetActive(true);
        }

        private void CloseSettings()
        {
            MenuSettings.Save();
            settings.gameObject.SetActive(false);
            ShowFront(true);
        }

        private void ShowFront(bool visible)
        {
            if (monumentPanel != null)
            {
                monumentPanel.SetActive(visible);
                approvedLogo.SetActive(visible);
                return;
            }
            front.gameObject.SetActive(visible);
            lettering.Show(visible);
        }

        private bool CanStart()
        {
            if (loading)
                return false;
            if (!Application.CanStreamedLevelBeLoaded(StartScene))
            {
                Debug.LogError("The prologue is missing from the build scene list.", this);
                return false;
            }
            return true;
        }

        private void StartGame()
        {
            if (!CanStart())
                return;
            loading = true;
            MenuSettings.Save();
            SceneManager.LoadSceneAsync(StartScene);
        }

        private static void Quit()
        {
            MenuSettings.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
