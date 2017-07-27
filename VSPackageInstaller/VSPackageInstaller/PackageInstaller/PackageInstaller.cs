using Microsoft.VisualStudio.Shell;
using System;
using VSPackageInstaller.Cache;
using System.Diagnostics;
using WebEssentials;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ExtensionManager;
using System.Linq;

namespace VSPackageInstaller.PackageInstaller
{
    public sealed class PackageInstaller
    {
        internal IExtensionDataItemView Extension { get; set; }

        public void InstallPackage()
        {
            System.Threading.Tasks.Task.Run(ManualInstallExtensionAsync);
//            System.Threading.Tasks.Task.Run(ExtMgrInstallExtensionAsync);
        }

        private async System.Threading.Tasks.Task ManualInstallExtensionAsync()
        {
            try
            {
                var fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.vsix");
                using (var webClient = new System.Net.WebClient())
                {
                    // TODO: a good citizen would keep a list of known temp artifacts (like this one) and delete it on next start up.

                    Logger.Log("Marketplace OK"); // Marketplace ok
                    Logger.Log("  " + "Downloading", false);

                    await webClient.DownloadFileTaskAsync(this.Extension.Installer, fileName);

                    Logger.Log("Downloading OK"); // Download ok
                }



                // Use the default windows file associations to invoke VSIXinstaller.exe since we don't know the path.

                Logger.Log("  " + "Installing", false);

                Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });

                Logger.Log("Install OK"); // Install ok
            }

            catch (Exception ex)
            {

                // TODO: perhaps we should handle specific exceptions and give custom error messages.

                Logger.Log("Install failed exception:"+ ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ExtMgrInstallExtensionAsync()
        {
            GalleryEntry entry = null;
            try
            {
                // Waits for MEF to initialize before the extension manager is ready to use
                var scComponentModel = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SComponentModel));

                IVsExtensionRepository repository = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
                IVsExtensionManager manager = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;

                Logger.Log($"{Environment.NewLine}{this.Extension.Title}");
                Logger.Log("  " + "Verifying ", false);


                entry = repository.GetVSGalleryExtensions<GalleryEntry>(new System.Collections.Generic.List<string> { this.Extension.VsixId.ToString() }, 1033, false)?.FirstOrDefault();

                if (entry != null)
                {
                    // ensure that we update the URL if it is empty
                    if (entry.DownloadUrl == null)
                        entry.DownloadUrl = this.Extension.Link;

                    Logger.Log("Marketplace OK"); // Marketplace ok
                    Logger.Log("  " + "Downloading", false);

                    IInstallableExtension installable = repository.Download(entry);
                    Logger.Log("Downloading OK"); // Download ok
                    Logger.Log("  " + "Installing", false);
                    manager.Install(installable, false);
                    Logger.Log("Install OK"); // Install ok
                }
                else
                {
                    Logger.Log("Marketplace failed"); // Markedplace failed
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Install failed exception: " + ex);
            }
            finally
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }
    }
}
