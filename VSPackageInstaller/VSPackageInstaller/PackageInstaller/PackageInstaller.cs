using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPackageInstaller.PackageInstaller
{
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
            return "";
        }

    }
}
