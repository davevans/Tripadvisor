using HtmlAgilityPack;
using System;
using System.Linq;
using System.Web;

namespace TripAdvisorScapage
{
    public static class HtmlNodeExtensions
    {
        public static Review GetReview(this HtmlNode node)
        {
            var id = node.GetAttributeValue("data-reviewId", null);

            // review date
            DateTime ratingDate = DateTime.MinValue;
            int rating = 0;
            string title = string.Empty;

            var ratingDateNode = node.Descendants().FirstOrDefault(n => n.HasClass("ratingDate") && n.Attributes["title"] != null);
            if (ratingDateNode != null)
            {
                DateTime.TryParseExact(ratingDateNode.Attributes["title"].Value, "d MMMM yyyy", //https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings?view=netframework-4.7.2
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out ratingDate);


                var ratingParent = ratingDateNode.ParentNode;
                var ratingNode = ratingParent
                                    .Descendants()
                                    .FirstOrDefault(x => x.Attributes.Any(a => a.Value.StartsWith("ui_bubble_rating bubble_", StringComparison.OrdinalIgnoreCase)));

                if (ratingNode != null)
                {
                    // review rating
                    var ratingString = ratingNode.Attributes["class"]?.Value;
                    if (!string.IsNullOrWhiteSpace(ratingString))
                    {
                        int.TryParse(ratingString.Substring(ratingString.Length - 2), out rating);
                    }

                    if (rating > 0)
                    {
                        rating = rating / 10;
                    }
                }

                // review title
                var titleNode = ratingParent.Descendants().FirstOrDefault(n => n.HasClass("noQuotes"));
                title = HttpUtility.HtmlDecode(titleNode?.InnerText?.Trim() ?? string.Empty);

                // alternative title DOM
                if (string.IsNullOrWhiteSpace(title))
                {
                    titleNode = ratingParent.Descendants().FirstOrDefault(n => n.HasClass("title") && n.Attributes["id"]?.Value == "HEADING");
                    title = titleNode?.InnerText?.Trim();
                }

            }

            //review text
            var textNode = node.Descendants().FirstOrDefault(n => n.HasClass("partial_entry"));
            var reviewText = HttpUtility.HtmlDecode(textNode?.InnerText?.Trim() ?? string.Empty);

            //remove linefeeds
            reviewText = reviewText.ReplaceLineFeeds();

            //reviewer name
            var reviewerNameNode = node.Descendants()
                                        .FirstOrDefault(n => n.HasClass("member_info"))?
                                        .Descendants()?
                                        .FirstOrDefault(n => n.HasClass("info_text"))?
                                        .Descendants()?
                                        .FirstOrDefault(); //div containing reviewerName

            var reviewer = reviewerNameNode?.InnerText ?? string.Empty;

            //reviwer location
            var locationNode = node.Descendants()
                                    .FirstOrDefault(x => x.HasClass("userLoc"))?
                                    .Descendants()?
                                    .FirstOrDefault(); //<strong> containing location

            var location = locationNode?.InnerText ?? string.Empty;


            return new Review
            {
                Id = id,
                ReviewDate = ratingDate,
                Rating = rating,
                Title = title,
                Text = reviewText,
                Reviewer = reviewer,
                ReviewerLocation = location
            };
        }
    }
}
