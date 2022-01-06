using System.Net;
using System.Text.Json;

namespace MakeAGETRequest_charp
{
    /// <summary>
    /// Bet Right Technical Task, author: Quinn Chan
    /// </summary>


    //classes for deserialisation of json
    public class Race1
    {
        public int EventTypeId { get; set; }
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public object Venue { get; set; }
        public object CountryCode { get; set; }
        public int RaceNumber { get; set; }
        public string AdvertisedStartTime { get; set; }
        public int ResultStatusId { get; set; }
        public int SecondsToJump { get; set; }
        public bool HasFixedMarkets { get; set; }
        public bool IsOpenForBetting { get; set; }
        public object MarketShortcuts { get; set; }
        public object Results { get; set; }
        public object MasterCategoryName { get; set; }
        public object EventName { get; set; }

        public Int64 RaceTime;
    }

    public class Race
    {
        public int VenueId { get; set; }
        public string Venue { get; set; }
        public Race1 Race1 { get; set; }
        public string CountryCode { get; set; }
        public string MasterCategoryName { get; set; }
    }

    public class Throughbred : Race
    {
       
    }

    public class Harness : Race
    {
        
    }

    public class Greyhound : Race
    {
        
    }

    public class Root
    {
        public List<Race> Throughbred { get; set; }
        public List<Race> Harness { get; set; }
        public List<Race> Greyhound { get; set; }
    }


    class Class1
    {
        static void Main(string[] args)
        {
            //Make GET request and obtain JSON string
            string sURL;
            sURL = "https://www.betright.com.au/api/racing/todaysracing";

            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(sURL);

            Stream objStream;
            objStream = wrGETURL.GetResponse().GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);

            string jsonString = objReader.ReadToEnd();


            //Deserialise Json
            Root myDeserializedClass = JsonSerializer.Deserialize<Root>(jsonString);


            //Extract races from the root object and store in a list for query
            var allRaces = new List<Race>();
            allRaces.AddRange(myDeserializedClass.Throughbred);
            allRaces.AddRange(myDeserializedClass.Harness);
            allRaces.AddRange(myDeserializedClass.Greyhound);


            //Convert the AdvertisedStartTime to long type
            foreach (var p in allRaces)
            {
                string AST = p.Race1.AdvertisedStartTime;
                int pFrom = AST.IndexOf("(") + 1;
                int pTo = AST.LastIndexOf(")");
                p.Race1.RaceTime = Int64.Parse(AST.Substring(pFrom, pTo - pFrom))/1000;
            }

            //Sort the list by race time
            List<Race> query = allRaces.OrderBy(x => x.Race1.RaceTime).ToList();
            /*foreach (Race r in query)
            {
                Console.WriteLine("{0} - {1}", r.Venue, r.Race1.RaceTime);
            }*/

            
            //get local time and convert to GMT unix time
            DateTime foo = DateTime.Now;
            long unixTimeNow = ((DateTimeOffset)foo).ToUnixTimeSeconds() - 36000;
            Console.WriteLine("Unix epoch time now : {0} \n", unixTimeNow);
                      
            

            //-	Next 5 races ordered by AdvertisedStartTime
            Console.WriteLine("\n--------------------------------\n" +
                                "Next 5 races\n" +
                                "--------------------------------");
            foreach (var p in query.Where(t => t.Race1.RaceTime > unixTimeNow).Take(5))
            {
                Console.WriteLine("{0} Race {1}", p.Venue, p.Race1.RaceNumber);
            }


            //-	Count of venues grouped by CountryCode
            Console.WriteLine("\n--------------------------------\n" +
                                "Count of venues grouped by CountryCode\n" +
                                "--------------------------------");
            foreach (var p in query.GroupBy(t => t.CountryCode)
                        .Select(group => new {
                            CC = group.Key,
                            Count = group.Select(q => q.Venue).Distinct().Count()
                        })
                        .OrderBy(x => x.CC))
            {
                Console.WriteLine("{0} {1}", p.CC, p.Count);
            }


            //-	Filtered list of all items with RaceNumber == 1, 
            Console.WriteLine("\n--------------------------------\n" +
                                "Filtered list of all items with RaceNumber == 1\n" +
                                "--------------------------------");
            foreach (var p in query.Where(t => t.Race1.RaceNumber == 1))
            {
                Console.Write("{0} {1}", p.Venue, p.Race1.RaceNumber);
                if (p.Race1.RaceTime > unixTimeNow)
                {
                    Console.WriteLine(" -- yet to commence racing");
                }
                else {
                    Console.WriteLine("");
                }
                
            }


        }
    }
}

