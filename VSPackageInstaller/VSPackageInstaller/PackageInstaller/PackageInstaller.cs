using Microsoft.VisualStudio.Shell;
using System;
using Tasks = System.Threading.Tasks;
using WebEssentials;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.ComponentModelHost;

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
            WebEssentials.Installer installer = new WebEssentials.Installer();

            ExtensionEntry extensionEntry = new ExtensionEntry
            {
                Id = _extensionId,
                Name = _extensionName
            };
            installer.InstallExtension(extensionEntry, repository, manager);
        }

    }
}
