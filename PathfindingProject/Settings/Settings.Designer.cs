using System.Configuration;

namespace PathfindingProject.Settings
{
    [System.Runtime.CompilerServices.CompilerGenerated()]
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static readonly Settings s_defaultInstance = ((Settings)Synchronized(new Settings()));

        public static Settings Default => s_defaultInstance;

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool PopupShown
        {
            get => (bool)this["PopupShown"];
            set => this["PopupShown"] = value;
        }
    }
}