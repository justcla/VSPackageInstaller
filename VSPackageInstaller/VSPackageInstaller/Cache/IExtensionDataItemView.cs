namespace VSPackageInstaller.Cache
{
    using System;

    internal interface IExtensionDataItemView
    {
        Guid ExtensionId { get; }

        String Title { get; }

        String Description { get; }

        String Version { get; }

        String Author { get; }

        String Link { get; }

        String Installer { get; }

        String VsixId { get; }
    }
}
