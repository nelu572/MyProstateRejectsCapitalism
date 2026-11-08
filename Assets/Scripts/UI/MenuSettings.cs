using UnityEngine;

namespace MyProstateRejectsCapitalism.UI
{
    internal static class MenuSettings
    {
        private const string MouseKey = "settings.mouseSensitivity";
        private const string SubtitleKey = "settings.subtitleSpeed";
        private const string FullscreenKey = "settings.fullscreen";

        internal const float DefaultSensitivity = .10f;
        internal const float MinSensitivity = .03f;
        internal const float MaxSensitivity = .25f;
        internal const int DefaultSubtitleSpeed = 55;
        internal const int MinSubtitleSpeed = 30;
        internal const int MaxSubtitleSpeed = 100;

        private static float? mouseSensitivity;
        private static int? subtitleSpeed;
        private static bool? fullscreen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            mouseSensitivity = PlayerPrefs.HasKey(MouseKey)
                ? Mathf.Clamp(PlayerPrefs.GetFloat(MouseKey), MinSensitivity, MaxSensitivity) : null;
            subtitleSpeed = PlayerPrefs.HasKey(SubtitleKey)
                ? Mathf.Clamp(PlayerPrefs.GetInt(SubtitleKey), MinSubtitleSpeed, MaxSubtitleSpeed) : null;
            fullscreen = PlayerPrefs.HasKey(FullscreenKey) ? PlayerPrefs.GetInt(FullscreenKey) != 0 : null;
            if (fullscreen.HasValue)
                ApplyDisplayMode(fullscreen.Value);
        }

        internal static float Sensitivity(float sceneDefault) => mouseSensitivity ?? sceneDefault;
        internal static float SubtitleSpeed(float sceneDefault) => subtitleSpeed ?? sceneDefault;
        internal static bool Fullscreen => fullscreen ?? Screen.fullScreen;

        internal static void SetSensitivity(float value)
        {
            mouseSensitivity = value;
            PlayerPrefs.SetFloat(MouseKey, value);
        }

        internal static void SetSubtitleSpeed(float value)
        {
            subtitleSpeed = Mathf.RoundToInt(value);
            PlayerPrefs.SetInt(SubtitleKey, subtitleSpeed.Value);
        }

        internal static void SetFullscreen(bool value)
        {
            fullscreen = value;
            PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
            ApplyDisplayMode(value);
        }

        private static void ApplyDisplayMode(bool value) =>
            Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        internal static void Save() => PlayerPrefs.Save();
    }
}
