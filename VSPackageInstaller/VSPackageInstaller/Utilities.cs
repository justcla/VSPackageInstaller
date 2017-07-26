namespace VSPackageInstaller
{
    using System.IO;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Settings;

    internal static class Utilities
    {
        private const string ExtensionName = "VSPackageInstaller";
        private static string instanceLocalAppData;

        public static string ExtensionAppDataPath => instanceLocalAppData ?? (instanceLocalAppData = ComputeLocalAppDataPath());

        private static string ComputeLocalAppDataPath()
        {
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            string localAppDataPath = settingsManager.GetApplicationDataFolder(ApplicationDataFolder.LocalSettings);

            return Path.Combine(
                localAppDataPath,
                ExtensionName);
        }
    }
}
