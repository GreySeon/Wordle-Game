using Microsoft.Maui.Storage;

namespace Wordle
{
    public partial class AppShell : Shell
    {
        public static bool IsSoundMuted { get; private set; } = false; // default

        public AppShell()
        {
            InitializeComponent();

            // Load dark mode preference
            var IsDarkMode = Preferences.Get("IsDarkMode", false); // Light mode for default
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
            ThemeToggleCheckBox.IsChecked = IsDarkMode;

            // Load sound mute preference
            IsSoundMuted = Preferences.Get("IsSoundMuted", false); // Unmuted for default
            SoundToggleCheckBox.IsChecked = IsSoundMuted;
        }

        private void ThemeToggleCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
                Preferences.Set("IsDarkMode", true);
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
                Preferences.Set("IsDarkMode", false);
            }
        }

        private void SoundToggleCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            IsSoundMuted = e.Value;
            Preferences.Set("IsSoundMuted", IsSoundMuted);
        }
    }
}
