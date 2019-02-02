namespace TripAdvisorScapage
{
    internal static class StringExtensions
    {
        internal const string replaceWith = "";
        public static string ReplaceLineFeeds(this string s)
        {
            return s.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
        }
    }
}
