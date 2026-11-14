using UnityEngine;

namespace ADOFAIEditorStudio.Multiplayer
{
    /// <summary>
    /// 自动同步的持久化设置。
    ///
    /// 不塞进 Main.Settings 是为了让自动补丁程序不必再改 Main.cs；
    /// PlayerPrefs 由 Unity 自己保存，跨会话有效。
    /// </summary>
    internal static class MultiplayerAutoSyncSettings
    {
        private const string EnabledKey = "AESX.Multiplayer.AutoSync";
        private const string MinutesKey = "AESX.Multiplayer.AutoSyncMinutes";
        internal const int DefaultMinutes = 5;

        internal static bool Enabled
        {
            get { return PlayerPrefs.GetInt(EnabledKey, 0) != 0; }
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        internal static int Minutes
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(MinutesKey, DefaultMinutes), 1, 1440); }
            set
            {
                PlayerPrefs.SetInt(MinutesKey, Mathf.Clamp(value, 1, 1440));
                PlayerPrefs.Save();
            }
        }
    }
}
