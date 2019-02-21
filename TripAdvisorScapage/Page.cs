using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace TripAdvisorScapage
{
    public class Page
    {
        private const string _fullReviewUrl = "https://www.tripadvisor.com.au/OverlayWidgetAjax?Mode=EXPANDED_HOTEL_REVIEWS_RESP";
        private HttpClient _httpClient;
        private HtmlNode _reviewsNode;
        private readonly ParentPage _parentPage;
        private readonly int _pageNumber;
        public int PageNumber => _pageNumber;

        public Page(HtmlNode reviewsNode, ParentPage parentPage, int pageNumber)
        {
            _reviewsNode = reviewsNode;
            _parentPage = parentPage;
            _pageNumber = pageNumber;

            var handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            _httpClient = new HttpClient(handler);
        }
        public async Task<List<Review>> GetReviewsAsync()
        {
            if (_reviewsNode == null)
                throw new InvalidOperationException("page not loaded.");

            // find divs with class "review-container"
            var reviewNodes = _reviewsNode
                    .Descendants()
                    .Where(n => n.HasClass("review-container"));


            if (reviewNodes != null)
            {
                //var reviewTasks = reviewNodes.Select(rc => rc.GetReviewAsync()).ToList();
                //await Task.WhenAll(reviewTasks);

                var reviewIds = reviewNodes.Select(rc => rc.GetReviewId()).ToList();

                return await GetFullReviewsByIds(reviewIds);
                //return reviewTasks.Select(x => x.Result).ToList();
            }

            return null;
        }

        public async Task<Page> GetNextPageAsync()
        {
            // find "next" link
            var nextLinkNode = _reviewsNode.Descendants()
                                    .FirstOrDefault(n => n.Name == "a" && n.Attributes["data-page-number"] != null && n.InnerText.Equals("Next", StringComparison.InvariantCultureIgnoreCase));

            if (nextLinkNode == null)
            {
                Console.WriteLine("Failed to find next node");
                return null;
            }

            var nextHref = nextLinkNode.Attributes["href"]?.Value;
            if (!string.IsNullOrWhiteSpace(nextHref))
            {
                var nextPageUrl = $"{_parentPage.Url.Scheme}://{_parentPage.Url.Host}{nextHref}";

                Console.WriteLine($"Next page URI is {nextPageUrl}.");

                var nextPage = await _parentPage.Web.LoadFromWebAsync(nextPageUrl);
                return new Page(nextPage?.DocumentNode, _parentPage, _pageNumber + 1);
            }

            // couldnt find next link
            return null;
        }

        private async Task<List<Review>> GetFullReviewsByIds(List<string> reviewIds)
        {
            var reviewIdsCsv = string.Join(",", reviewIds);
            var body = "reviews=" + HttpUtility.UrlEncode(reviewIdsCsv) + " & contextChoice=DETAIL";

            var request = new HttpRequestMessage(HttpMethod.Post, _fullReviewUrl)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            request.Headers.Add("X-Puid", "0");
            request.Headers.Referrer = _parentPage.Url;
            request.Headers.Add("Accept-Encoding", "gzip");

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var root = htmlDoc.DocumentNode;
            var reviewTasks = root.Descendants()?
                                  .Where(n => n.Attributes.Contains("data-reviewlistingid"))?
                                  .Select(n => n.GetReviewAsync())
                                  .ToList();

            await Task.WhenAll(reviewTasks);
            return reviewTasks.Select(x => x.Result).ToList();
        }
    }
}
