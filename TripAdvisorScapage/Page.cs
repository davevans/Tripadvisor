using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TripAdvisorScapage
{
    public class Page
    {       
        private HtmlNode _reviewsNode;
        private readonly ParentPage _parentPage;
        private readonly int _pageNumber;
        public int PageNumber => _pageNumber;

        public Page(HtmlNode reviewsNode, ParentPage parentPage, int pageNumber)
        {
            _reviewsNode = reviewsNode;
            _parentPage = parentPage;
            _pageNumber = pageNumber;
        }
        public List<Review> GetReviews()
        {
            if (_reviewsNode == null)
                throw new InvalidOperationException("page not loaded.");

            // find divs with class "review-container"
            return _reviewsNode
                    .Descendants()
                    .Where(n => n.HasClass("review-container"))
                    .Select(rc => rc.GetReview())
                    .ToList();
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
    }
}
