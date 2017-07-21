namespace VSPackageInstaller.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class CacheItemsCollection<TItem>
    {
        public CacheItemsCollection(IEnumerable<TItem> items)
        {
            this.Items = items ?? throw new ArgumentException($"{nameof(items)} cannot be null");
        }

        [DataMember]
        public IEnumerable<TItem> Items { get; set; }
    }
}
