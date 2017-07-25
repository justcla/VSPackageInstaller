namespace VSPackageInstaller.Cache
{
    internal interface IExtensionDataItemView
    {
        string Title { get; }

        string Description { get;}

        string Tags { get; }

        string Version { get; }

        string Author { get; }

        string Link { get; }

        string Installer { get; }
    }
}
