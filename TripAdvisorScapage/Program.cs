using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace TripAdvisorScapage
{
    class Program
    {
        private const string StartUrl = @"https://www.tripadvisor.com.au/ShowUserReviews-g580429-d547580-r236561551-Chartwell-Westerham_Sevenoaks_District_Kent_England.html";

        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            Console.WriteLine("Starting.");

            var parentPage = new ParentPage(StartUrl);
            var firstPageNode = await parentPage.GetFirstPage();

            var page = new Page(firstPageNode, 1);
            var reviews = page.GetReviews();

            foreach (var review in reviews)
            {
                Console.WriteLine($"Found review Id: {review.Id}. Date: {review.ReviewDate.ToShortDateString()}.");
            }
            
            Console.WriteLine("Complete");
        }
    }

    public class Page
    {       
        private HtmlNode _reviewsNode;
        private readonly int _pageNumber;

        public Page(HtmlNode reviewsNode, int pageNumber)
        {
            _reviewsNode = reviewsNode;
            _pageNumber = pageNumber;
        }

        //public async Task LoadFromUrlAsync()
        //{
        //    var web = new HtmlWeb();
        //    var htmlDoc = await web.LoadFromWebAsync(_url);
        //    _reviewsNode = htmlDoc.DocumentNode;
        //}

        public List<Review> GetReviews()
        {
            if (_reviewsNode == null)
                throw new InvalidOperationException("page not loaded.");

            Console.WriteLine($"Fetching reviews on page {_pageNumber}.");

            // find divs with class "review-container"
            return _reviewsNode
                    .Descendants()
                    .Where(n => n.HasClass("review-container"))
                    .Select(rc => rc.GetReview())
                    .ToList();
        }
    }

    public class ParentPage
    {
        private readonly Uri _url;

        public ParentPage(string url)
        {
            _url = new Uri(url);
        }
        public async Task<HtmlNode> GetFirstPage()
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(_url.ToString());

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

                Console.WriteLine($"Found link to first page. {href}.");

                var firstPageUrl = $"{_url.Scheme}://{_url.Host}{href}";
                var firstPage = await web.LoadFromWebAsync(firstPageUrl);
                return firstPage?.DocumentNode;
            }

            return null;
        }
    }

    public class Review
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime ReviewDate { get; set; }
        public string Reviewer { get; set; }
    }

    public static class HtmlNodeExtensions
    {
        public static Review GetReview(this HtmlNode node)
        {
            var id = node.GetAttributeValue("data-reviewId", null);

            // review date
            DateTime ratingDate = DateTime.MinValue;
            //int rating = 0;

            var ratingDateNode = node.Descendants().FirstOrDefault(n => n.HasClass("ratingDate") && n.Attributes["title"] != null);
            if (ratingDateNode != null)
            {
                DateTime.TryParseExact(ratingDateNode.Attributes["title"].Value, "d MMMM yyyy", //https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings?view=netframework-4.7.2
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out ratingDate);



            }

            //review title
            string title = string.Empty;


            return new Review
            {
                Id = id,
                ReviewDate = ratingDate
            };
        }
    }
}
