using FileHelpers;
using System;

namespace TripAdvisorScapage
{
    [DelimitedRecord("|")]
    public class Review
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }

        [FieldConverter(ConverterKind.Date, "dd MMMM yyyy")]
        public DateTime ReviewDate { get; set; }
        public string Reviewer { get; set; }
        public string ReviewerLocation { get; set; }
        public string AgeRange { get; set; }
        public string Sex { get; set; }
    }
}
