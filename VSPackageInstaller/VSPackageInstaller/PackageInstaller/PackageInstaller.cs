using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tasks = System.Threading.Tasks;
using WebEssentials;
using System.Threading;

namespace VSPackageInstaller.PackageInstaller
{
    public sealed class InstallerPackage : AsyncPackage

    {

        public const string _packageGuid = "4f2f2873-be87-4716-a4d5-3f3f047942c4";



        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)

        {

            InstallerService.Initialize(this);

            await InstallerService.RunAsync().ConfigureAwait(false);

        }

        public async Tasks.Task InitalizeAsync()
        {
            IProgress<ServiceProgressData> _progress = new System.Progress<ServiceProgressData>();

            await InitializeAsync(new CancellationToken(false), _progress);
        }

    }
    public class PackageInstaller
    {
        public string PackageTitle
        {
            get
            { return PackageTitle; }
            set
            { PackageTitle = value; }
        }

        public string PackageInstallPath
        {
            get
            { return PackageInstallPath; }
            set
            { PackageInstallPath = value; }
        }

        public PackageInstaller()
        {
            PackageTitle = "";
            PackageInstallPath = "";
        }

        public string InstallPackage(string _packageTitle, string _packageInstallPath)
        {
            PackageTitle = _packageTitle;
            PackageInstallPath = _packageInstallPath;
            return InstallPackage();
        }

        public string InstallPackage(string _packageInstallPath)
        {
            PackageInstallPath = _packageInstallPath;
            return InstallPackage();
        }

        public string InstallPackage()
        {
            InstallerPackage _pi = new InstallerPackage();
            // Todo: add code here to call InstallerPackage with dummy params and handle async
            return "";
        }

    }
}
