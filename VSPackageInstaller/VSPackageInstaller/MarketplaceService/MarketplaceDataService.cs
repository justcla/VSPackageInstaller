using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace VSPackageInstaller.MarketplaceService
{
    internal class MarketplaceDataService
    {
        private const string s_extensionQueryUrl = "https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery";
        private HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        public MarketplaceDataService()
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;api-version=3.1-preview.1");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
        }

        public void GetMarketplaceData()
        {
            GetMarketplaceDataItems("15.0", new List<string>() {"Pro", "Ultimate"}, DateTime.MinValue, TestCallback );
        }

        private bool TestCallback(IEnumerable<ExtensionDataItem> extensionDataItems)
        {
            foreach (ExtensionDataItem extensionDataItem in extensionDataItems)
            {
                Console.WriteLine(extensionDataItem.Title);
            }
            return true;
        }

        public void GetMarketplaceDataItems(String vsVersion, List<String> skus, DateTime baseTimeStamp,
            Func<IEnumerable<ExtensionDataItem>, bool> callback)
        {
            bool fetchNextPage = true;
            int pageNumber = 1;
            int pageSize = 500;
            while (fetchNextPage)
            {
                List<PublishedExtension> extensionsPage = GetNextPageFromMarketplace(pageNumber, pageSize, vsVersion, skus);
                if (extensionsPage == null)
                {
                    //some error occured 
                    break;
                }
                if (extensionsPage.Count != pageSize)
                {
                    fetchNextPage = false;
                }
                List<ExtensionDataItem> extensionDataItems = new List<ExtensionDataItem>();
                foreach (PublishedExtension publishedExtension in extensionsPage)
                {
                    if (publishedExtension.LastUpdated > baseTimeStamp)
                    {
                        extensionDataItems.Add(TransformToExtensionDataItem(publishedExtension));
                    }
                    else
                    {
                        fetchNextPage = false;
                        break;
                    }
                }
                if (callback != null)
                {
                    callback(extensionDataItems);
                }
                pageNumber++;
            }
        }

        private ExtensionDataItem TransformToExtensionDataItem(PublishedExtension publishedExtension)
        {
            ExtensionDataItem extensionDataItem = new ExtensionDataItem();
            extensionDataItem.ExtensionId = publishedExtension.ExtensionId;
            extensionDataItem.Description = publishedExtension.ShortDescription;
            extensionDataItem.Title = publishedExtension.DisplayName;
            extensionDataItem.Version = publishedExtension.Versions[0].Version;

            foreach (KeyValuePair<string, string> keyValuePair in publishedExtension.Versions[0].Properties)
            {
                if (keyValuePair.Key.Equals("VsixVersion"))
                {
                    extensionDataItem.Version = keyValuePair.Value;
                }
                else if (keyValuePair.Key.Equals("DownloadUrl")) //how does it work for updates?
                {
                    extensionDataItem.Installer = keyValuePair.Value;
                }
                else if (keyValuePair.Key.Equals("MoreInfoUrl")) // marketplace page
                {
                    extensionDataItem.Link = keyValuePair.Value;
                }
                else if (keyValuePair.Key.Equals("Author"))
                {
                    extensionDataItem.Author = keyValuePair.Value;
                }
            }
            return extensionDataItem;;
        }

        private List<PublishedExtension> GetNextPageFromMarketplace(int pageNumber, int pageSize, String vsVersion, List<String> skus)
        {
            try
            {
                ExtensionQuery query = CreateQueryForExtensions(pageNumber, pageSize, skus, vsVersion);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(s_extensionQueryUrl));
                String queryJson = JsonConvert.SerializeObject(query);
                request.Content = new StringContent(queryJson, Encoding.UTF8, "application/json");
                request.Headers.TryAddWithoutValidation("User-Agent", "VSIDE-" + vsVersion);

                HttpResponseMessage response = _httpClient.SendAsync(request).Result;
                String jsonString = response.Content.ReadAsStringAsync().Result;
                ExtensionQueryResult result = JsonConvert.DeserializeObject<ExtensionQueryResult>(jsonString);
                return result.Results[0].Extensions;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private ExtensionQuery CreateQueryForExtensions(int pageNumber, int pageSize, List<String> skus, string vsVersion)
        {
            ExtensionQuery query = new ExtensionQuery();
            query.Filters = new List<QueryFilter>()
            {
                new QueryFilter()
                {
                    PageNumber = pageNumber,
                    Criteria = new List<FilterCriteria>()
                    {
                        new FilterCriteria()
                        {
                            FilterType = 15,
                            Value = vsVersion
                        },
                        new FilterCriteria()
                        {
                            FilterType = 14,
                            Value = "1033"
                        }
                    },
                    PageSize = pageSize,
                    SortBy = 1,
                    SortOrder = 2
                }
            };
            //assuming skus are like Ultimate, Community etc. we will add prefix as needed by Marketplace
            foreach (string sku in skus)
            {
                query.Filters[0].Criteria.Add(new FilterCriteria()
                {
                    FilterType = 8,
                    Value = "Microsoft.VisualStudio." + sku
                });
            }
            return query;;
        }
    }
}
