using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace VSPackageInstaller.MarketplaceService
{
    internal static class VsSkus
    {
        public const string Ultimate = "Ultimate";
        public const string Enterprise = "Enterprise";
        public const string Pro = "Pro";
        public const string Community = "Community";
        public const string IntegratedShell = "IntegratedShell";
        public const string Premium = "Premium";
        public const string TeamExplorer = "TeamExplorer";
        public const string SQL = "SQL";
    }
    static class VsEditionUtil
    {
        private static IDictionary<String, List<String>> EditionToSkuDictionary =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    VsSkus.Enterprise, new List<string>()
                    {
                        VsSkus.Enterprise,
                        VsSkus.Ultimate,
                        VsSkus.Pro,
                        VsSkus.Community,
                        VsSkus.IntegratedShell
                    }
                },
                {
                    VsSkus.Ultimate, new List<string>()
                    {
                        VsSkus.Ultimate,
                        VsSkus.Enterprise,
                        VsSkus.Pro,
                        VsSkus.Community,
                        VsSkus.IntegratedShell
                    }
                },
                {
                    VsSkus.Premium, new List<string>()
                    {
                        VsSkus.Premium,
                        VsSkus.Pro,
                        VsSkus.Community,
                        VsSkus.IntegratedShell
                    }
                },
                {
                    VsSkus.Pro, new List<string>()
                    {
                        VsSkus.Pro,
                        VsSkus.Community,
                        VsSkus.IntegratedShell
                    }
                },
                {
                    VsSkus.Community, new List<string>()
                    {
                        VsSkus.Community,
                        VsSkus.Pro,
                        VsSkus.IntegratedShell
                    }
                },
                {
                    VsSkus.TeamExplorer, new List<string>()
                    {
                        VsSkus.TeamExplorer,
                        VsSkus.IntegratedShell
                    }
                },
                {
                    VsSkus.SQL, new List<string>()
                    {
                        VsSkus.TeamExplorer,
                        VsSkus.IntegratedShell
                    }
                }
            };

        public static string GetCurrentVsVersion()
        {
            //does not give the full version value only gives 15.0
            EnvDTE.DTE dte = ((EnvDTE.DTE)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE).GUID));
            string version = dte.Version;
            return version;
        }

        public static List<string> GetSkusList()
        {
            EnvDTE.DTE dte = ((EnvDTE.DTE)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE).GUID));
            string edition = dte.Edition;
            return GetSupportedSkus(edition);
        }

        private static List<string> GetSupportedSkus(string currentEdition)
        {
            List<String> skus;
            if (!EditionToSkuDictionary.TryGetValue(currentEdition, out skus))
            {
                skus = new List<string>() { currentEdition };
            }
            return skus;
        }
    }
}
