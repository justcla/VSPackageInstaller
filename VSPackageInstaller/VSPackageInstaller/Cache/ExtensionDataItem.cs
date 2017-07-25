namespace VSPackageInstaller.Cache
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class ExtensionDataItem : IExtensionDataItemView
    {
        public ExtensionDataItem(
            string title,
            string description,
            string tags,
            string version,
            string author,
            string link,
            string installer)
        {
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(tags) ||
                string.IsNullOrWhiteSpace(version) ||
                string.IsNullOrWhiteSpace(author) ||
                string.IsNullOrWhiteSpace(link) ||
                string.IsNullOrWhiteSpace(installer))
            {
                throw new ArgumentException($"{nameof(ExtensionDataItem)} parameters cannot be null or empty");
            }

            this.Title = title;
            this.Description = description;
            this.Tags = tags;
            this.Version = version;
            this.Author = author;
            this.Link = link;
            this.Installer = installer;
        }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Tags { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public string Installer { get; set; }
    }
}
