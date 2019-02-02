using CommandLine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TripAdvisorScapage
{

    class Program
    {
        //private const string StartUrl = @"https://www.tripadvisor.com.au/ShowUserReviews-g580429-d547580-r236561551-Chartwell-Westerham_Sevenoaks_District_Kent_England.html";

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(o =>
                  {
                      Console.WriteLine(o.StartPage);
                      Console.WriteLine(o.SavePath);
                      Console.WriteLine(o.MaxPages);

                      RunAsync(o.StartPage, o.SavePath, o.MaxPages).Wait();
                  });
        }

        static async Task RunAsync(string startUrl, string SavePath, int? maxPages)
        {
            Console.WriteLine("Starting.");

            var parentPage = new ParentPage(startUrl);
            var firstPageNode = await parentPage.GetFirstPage();
            var page = new Page(firstPageNode, parentPage, 1);
            var reviews = new List<Review>();
            var pageCount = 0;

            while (page != null)
            {
                pageCount = page.PageNumber;

                if (maxPages > 0 && pageCount >= maxPages)
                {
                    break;
                }

                // get reviews
                var pageReviews = page.GetReviews();
                Console.WriteLine($"Found {pageReviews.Count} reviews on page {page.PageNumber}.");

                reviews.AddRange(pageReviews);

                // get next page
                page = await page.GetNextPageAsync();

            }

            Console.WriteLine($"Found {reviews.Count} reviews from {pageCount} pages.");

            //save to CSV
            reviews.ToCSV(SavePath);
            Console.WriteLine("Complete");
            Console.ReadLine();
        }
    }
}
