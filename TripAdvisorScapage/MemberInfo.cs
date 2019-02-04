using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace TripAdvisorScapage
{
    internal class MemberInfo
    {
        public static HtmlWeb Web = new HtmlWeb();
        private static Regex ageSexRegEx = new Regex(@"(?<age>[0-9\+\-]+)\s(?<sex>[a-z]*).*$", RegexOptions.Compiled);

        public static async Task<(string ageRange, string sex)> GetAgeRangeAndSex(string uid, string memberId)
        {
            string url = $"https://www.tripadvisor.com.au/MemberOverlay?Mode=owa&uid={uid}&c=&src={memberId}&fus=false&partner=false&LsoId=&metaReferer=ShowUserReviewsAttractions";

            var doc = await Web.LoadFromWebAsync(url);
            var memberdescriptionReviewEnhancementsNode = doc.DocumentNode.Descendants().FirstOrDefault(x => x.HasClass("memberdescriptionReviewEnhancements"));

            if (memberdescriptionReviewEnhancementsNode == null)
            {
                return (null, null);
            }


            var liNodes = memberdescriptionReviewEnhancementsNode?
                            .Descendants()?
                            .Where(n => n.Name.Equals("li", System.StringComparison.OrdinalIgnoreCase))?
                            .ToList(); // <li> nodes

            if (liNodes != null && liNodes.Any() && liNodes.Count >= 2)
            {
                //get 2nd liNode
                var ageSexNode = liNodes[1];
                if (ageSexNode != null)
                {
                    var match = ageSexRegEx.Match(ageSexNode.InnerText);
                    if (match.Success)
                    {
                        var ageRange = match.Groups["age"].Value;
                        var sex = match.Groups["sex"].Value;

                        return (ageRange, sex);
                    }
                }
            }

            return (null, null);
        }
    }
}
