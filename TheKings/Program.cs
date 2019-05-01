using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TheKings
{
    class Program
    {
        static void Main(string[] args)
        {
            var monarchsUrl = "http://mysafeinfo.com/api/data?list=englishmonarchs&format=json&token=TLuYP52OhIw24TQSYqexw0q5k1zoPS2P";

            var monarchs = GetMonarchsFromUrl(monarchsUrl);
            if (monarchs == null || !monarchs.Any())
            {
                // Logging
                Console.WriteLine("There was no monarchs to be found");
            }

            Console.WriteLine($"Monarchs fetched: {monarchs.Count}");

            var longestLastingMonarch = monarchs.OrderByDescending(x => x.YearsActive).FirstOrDefault();
            Console.WriteLine($"Monarch that ruled the longest: {longestLastingMonarch.Name}. {longestLastingMonarch.YearsActive} years active");

            var longestLastingHouse = monarchs.GroupBy(x => x.House).OrderByDescending(x => x.Sum(y => y.YearsActive)).FirstOrDefault();
            var longestLastingHouseActiveYearsTotal = monarchs.Where(x => x.House == longestLastingHouse.Key).Sum(y => y.YearsActive);
            Console.WriteLine($"House that ruled the longest: {longestLastingHouse.Key}. {longestLastingHouseActiveYearsTotal} years active");

            var mostCommonFirstName = monarchs.GroupBy(x => x.Name.Split(' ').First()).OrderByDescending(x => x.Count()).FirstOrDefault();
            Console.WriteLine($"Most common first name: {mostCommonFirstName.Key}");
            Console.ReadLine();
        }

        private static List<Monarch> GetMonarchsFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                // Logging
                Console.WriteLine("Url was empty");
                return null;
            }

            using (var client = new HttpClient())
            {
                using (var res = client.GetAsync(url).Result)
                {
                    var data = res.Content.ReadAsStringAsync().Result;
                    if (data == null)
                    {
                        // Logging
                        Console.WriteLine("No data returned from url");
                        return null;
                    }

                    return JsonConvert.DeserializeObject<List<Monarch>>(data);
                }
            }
        }
    }

    public class Monarch
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("nm")]
        public string Name { get; set; }

        [JsonProperty("cty")]
        public string City { get; set; }

        [JsonProperty("hse")]
        public string House { get; set; }

        [JsonProperty("yrs")]
        public string YearRange { get; set; }

        private int? _yearsActive;
        public int YearsActive
        {
            get
            {
                if (_yearsActive.HasValue)
                {
                    return _yearsActive.Value;
                }

                switch (YearRange)
                {
                    case var fullRange when Regex.IsMatch(YearRange, "^[0-9+]+\\-[0-9]+$"): // matches "anyNum-anyNum"
                        var yearRangeArray = fullRange.Split('-', StringSplitOptions.RemoveEmptyEntries);
                        _yearsActive = int.Parse(yearRangeArray.LastOrDefault()) -
                                       int.Parse(yearRangeArray.FirstOrDefault());
                        break;

                    case var _ when Regex.IsMatch(YearRange, "^[0-9+]+$"): // matches any number
                        _yearsActive = 1;
                        break;

                    case var looseEndRange when Regex.IsMatch(YearRange, "^[0-9+]+\\-$"): // matches "anyNum-"
                        var startYear = looseEndRange.TrimEnd('-');
                        _yearsActive = DateTime.UtcNow.Year - int.Parse(startYear);
                        break;

                    default:
                        // Logging
                        Console.WriteLine($"Monarch with ID: {Id} has an invalid year range: {YearRange}");
                        break;
                }

                return _yearsActive.Value;
            }
        }
    }
}