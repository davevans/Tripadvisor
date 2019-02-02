using HtmlAgilityPack;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace TripAdvisorScapage
{
    public class ParentPage
    {
        public readonly Uri Url;
        public HtmlWeb Web;

        public ParentPage(string url)
        {
            Url = new Uri(url);
            Web = new HtmlWeb();
        }
        public async Task<HtmlNode> GetFirstPage()
        {
            var doc = await Web.LoadFromWebAsync(Url.ToString());

            // find first page from pagination
            var firstPageAnchor = doc.DocumentNode
                                    .Descendants()
                                    .FirstOrDefault(n => n.HasClass("pageNumbers"))
                                        .Descendants()
                                        .FirstOrDefault(a => a.Attributes.FirstOrDefault(aa => aa.Name == "data-page-number" && aa.Value == "1") != null);

            if (firstPageAnchor != null)
            {
                var href = firstPageAnchor.Attributes["href"]?.Value;
                if (string.IsNullOrWhiteSpace(href))
                {
                    Console.WriteLine("Failed to find link to first page.");
                    return null;
                }

                var firstPageUrl = $"{Url.Scheme}://{Url.Host}{href}";
                var firstPage = await Web.LoadFromWebAsync(firstPageUrl);
                return firstPage?.DocumentNode;
            }

            return null;
        }
    }
}
