using Microsoft.VisualStudio.Shell;
using System;
using Tasks = System.Threading.Tasks;
using WebEssentials;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Collections.Generic;
using System.Linq;

namespace VSPackageInstaller.PackageInstaller
{
    public sealed class PackageInstaller
    {

        public const string _packageGuid = "4f2f2873-be87-4716-a4d5-3f3f047942c4";
        public void InstallPackages(string _extensionId, string _extensionName)
        {
            //            IProgress<ServiceProgressData> _progress = new System.Progress<ServiceProgressData>();
            //            var task = InitializeAsync(new CancellationToken(false), _progress);

            // Waits for MEF to initialize before the extension manager is ready to use
            var scComponentModel = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SComponentModel));

            var repository = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
            var manager = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;
            Version vsVersion = VsHelpers.GetVisualStudioVersion();

            var registry = new RegistryKeyWrapper(VSPackageInstaller.VSPackage.thePackage.UserRegistryRoot);

            ExtensionEntry extensionEntry = new ExtensionEntry
            {
                Id = _extensionId,
                Name = _extensionName,
                MaxVersion = vsVersion,
                MinVersion = vsVersion
            };
            InstallExtension(extensionEntry, repository, manager);
        }
        private void InstallExtension(ExtensionEntry extension, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            GalleryEntry entry = null;
            try
            {
                Logger.Log($"{Environment.NewLine}{extension.Name}");
                Logger.Log("  " + "Verifying ", false);

                entry = repository.GetVSGalleryExtensions<GalleryEntry>(new List<string> { extension.Id }, 1033, false)?.FirstOrDefault();

                if (entry != null)
                {
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
            catch (Exception)
            {
                Logger.Log("Install failed exception");
            }
        }


    }
}
