using Microsoft.VisualStudio.Shell;
using System;
using Tasks = System.Threading.Tasks;
using WebEssentials;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;

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
            IProgress<ServiceProgressData> _progress = new System.Progress<ServiceProgressData>();

            var task = InitializeAsync(new CancellationToken(false), _progress);
        }

    }
}
