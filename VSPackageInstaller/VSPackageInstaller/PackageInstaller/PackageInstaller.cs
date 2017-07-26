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
    public class StaticRegistryKey : IRegistryKey

    {

        private object _value;



        public IRegistryKey CreateSubKey(string subKey)

        {

            return this;

        }



        public void Dispose()

        {

            //

        }



        public object GetValue(string name)

        {

            return _value;

        }



        public void SetValue(string name, object value)

        {

            _value = value;

        }

    }

    [Guid(_packageGuid)]

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PackageInstaller:AsyncPackage
    {

        public const string _packageGuid = "4f2f2873-be87-4716-a4d5-3f3f047942c4";
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)

        {

            InstallerService.Initialize(this);

            await InstallerService.RunAsync().ConfigureAwait(false);

        }

        public void InstallPackages()
        {
            //            IProgress<ServiceProgressData> _progress = new System.Progress<ServiceProgressData>();
            //            var task = InitializeAsync(new CancellationToken(false), _progress);

            // Waits for MEF to initialize before the extension manager is ready to use
            var scComponentModel = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SComponentModel));

            var repository = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
            var manager = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;
            Version vsVersion = VsHelpers.GetVisualStudioVersion();

            var registry = new RegistryKeyWrapper(VSPackageInstaller.VSPackage.thePackage.UserRegistryRoot);
            var store = new DataStore(registry, Constants.LogFilePath);
            var feed = new LiveFeed(Constants.LiveFeedUrl, Constants.LiveFeedCachePath);

            WebEssentials.Installer installer = new WebEssentials.Installer(feed, store);
 
            ExtensionEntry extensionEntry = new ExtensionEntry();
            extensionEntry.Id = "3b64e04c-e8de-4b97-8358-06c73a97cc68";
            extensionEntry.Name = "ResXManager";
            installer.InstallExtension(extensionEntry, repository, manager);
        }

    }
}
