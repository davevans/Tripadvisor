using FileHelpers;
using System.Collections.Generic;

namespace TripAdvisorScapage
{
    internal static class ReviewListExtensions
    {
        public static void ToCSV(this List<Review> reviews, string targetFilePath)
        {
            // turn reviews into CSV
            var engine = new FileHelperEngine<Review>(System.Text.Encoding.UTF8);
            engine.HeaderText = engine.GetFileHeader();


            // Save to disk
            engine.WriteFile(targetFilePath, reviews);
        }
    }
}
