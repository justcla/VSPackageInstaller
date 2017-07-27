namespace VSPackageInstaller.Cache
{
    using System;

    internal class ExtensionDataItem : IExtensionDataItemView
    {
        public Guid ExtensionId { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public String Version { get; set; }

        public String Author { get; set; }

        public String Link { get; set; }

        public String VsixId { get; set; }

        public String Installer { get; set; }
    }
}
