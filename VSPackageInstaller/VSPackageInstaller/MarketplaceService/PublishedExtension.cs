using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPackageInstaller.MarketplaceService
{
    public class PublishedExtension
    {
        public Guid ExtensionId { get; set; }

        public String DisplayName { get; set; }

        public DateTime LastUpdated { get; set; }

        public String ShortDescription { get; set; }

        public List<ExtensionVersion> Versions { get; set; }
    }

    public class ExtensionVersion
    {
        public String Version { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<KeyValuePair<String, String>> Properties { get; set; }
    }

    public class ExtensionQuery
    {
        public List<QueryFilter> Filters { get; set; }

        public Int32 Flags { get; set; }

        public List<String> AssetTypes { get; set; }
    }

    public class ExtensionQueryResult
    {
        public List<ExtensionFilterResult> Results { get; set; }
    }

    public class ExtensionFilterResult
    {
        public List<PublishedExtension> Extensions { get; set; }

        public List<ExtensionFilterResultMetadata> ResultMetadata { get; set; }
    }

    public class ExtensionFilterResultMetadata
    {
        public String MetadataType { get; set; }

        public List<MetadataItem> MetadataItems { get; set; }
    }

    public class MetadataItem
    {
        public String Name { get; set; }

        public int Count { get; set; }
    }

    public class QueryFilter
    {
        public List<FilterCriteria> Criteria { get; set; }

        public Int32 PageSize { get; set; }

        public Int32 PageNumber { get; set; }

        public Int32 SortBy { get; set; }

        public Int32 SortOrder { get; set; }
    }

    public class FilterCriteria
    {
        public Int32 FilterType { get; set; }

        public String Value { get; set; }
    }
}
