namespace VSPackageInstaller.Cache
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ExtensionDataItem : IExtensionDataItemView
    {
        [DataMember]
        public Guid ExtensionId { get; set; }

        [DataMember]
        public String Title { get; set; }

        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public String Version { get; set; }

        [DataMember]
        public String Author { get; set; }

        [DataMember]
        public String Link { get; set; }

        [DataMember]
        public String Installer { get; set; }
    }
}
