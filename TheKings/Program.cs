using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace TheKings
{
    class Program
    {
        static void Main(string[] args)
        {
            var monarchsUrl = "http://mysafeinfo.com/api/data?list=englishmonarchs&format=json&token=TLuYP52OhIw24TQSYqexw0q5k1zoPS2P";

            var monarchs = GetMonarchsFromUrl(monarchsUrl);
            if (!monarchs.Any())
            {
                // Logging
                Console.WriteLine("There was no monarchs to be found!");
            }

            Console.WriteLine($"Monarchs fetched: {monarchs.Count}");

            var longestLastMonarch = monarchs.OrderByDescending(x => x.YearsActive).FirstOrDefault();
            Console.WriteLine($"Monarch that ruled the longest: {longestLastMonarch.City}. {longestLastMonarch.YearsActive} years active");

            Console.WriteLine($"House that ruled the longest: {longestLastMonarch.House}. {longestLastMonarch.YearsActive} years active");

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
                        Console.WriteLine("No data");
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

                var yearRangeArray = YearRange.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (yearRangeArray.Length == 1)
                {
                    _yearsActive = 1;
                }

                _yearsActive = int.Parse(yearRangeArray.LastOrDefault()) -
                               int.Parse(yearRangeArray.FirstOrDefault());

                return _yearsActive.Value;
            }
        }
    }
}