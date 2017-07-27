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

        //public const string _packageGuid = "4f2f2873-be87-4716-a4d5-3f3f047942c4";
        public void InstallPackage(string _extensionId, string _extensionName, string _extensionURL)
        {
            //            IProgress<ServiceProgressData> _progress = new System.Progress<ServiceProgressData>();
            //            var task = InitializeAsync(new CancellationToken(false), _progress);

            InstallExtension(_extensionId, _extensionName);
        }
        private void InstallExtension(string extensionId, string extensionName)
        {
            GalleryEntry entry = null;
            try
            {
                // Waits for MEF to initialize before the extension manager is ready to use
                var scComponentModel = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SComponentModel));

                IVsExtensionRepository repository = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
                IVsExtensionManager manager = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;

                Logger.Log($"{Environment.NewLine}{extensionName}");
                Logger.Log("  " + "Verifying ", false);


                //testing with hardcoded extension id
                extensionId = "2d8aa02a-8810-421f-97b9-86efc573fea3";
                entry = repository.GetVSGalleryExtensions<GalleryEntry>(new List<string> { extensionId }, 1033, false)?.FirstOrDefault();

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
