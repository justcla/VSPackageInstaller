using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSPackageInstaller.MarketplaceService
{
    internal class ExtensionDataItem
    {
        public Guid ExtensionId { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public String Version { get; set; }

        public String Author { get; set; }

        public String Link { get; set; }

        public String Installer { get; set; }
    }
}
