using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Reflection;

namespace FilmSniffer
{
    class Program
    {
        const string LOGO =
@"    ___
 __/_  `.  .-""""""-.
 \_,` | \-'  /   )`-')
  "") `""`    \  ((`""`
 ___Y  ,    .'7 /|
(_, ___/...-` (_/_/";

        class Config 
        {
            public string Key = "";
            public string Path = "films.txt";
            public bool SeperateFiles = false;
            public List<string> Films = new List<string>();
        };

        public partial class Movie
        {
            public string Title { get; set; }
            public string Year { get; set; }
            public string Rated { get; set; }
            public string Released { get; set; }
            public string Runtime { get; set; }
            public string Genre { get; set; }
            public string Director { get; set; }
            public string Writer { get; set; }
            public string Actors { get; set; }
            public string Plot { get; set; }
            public string Language { get; set; }
            public string Country { get; set; }
            public string Awards { get; set; }
            public string Poster { get; set; }
            public Rating[] Ratings { get; set; }
            public string Metascore { get; set; }
            public string ImdbRating { get; set; }
            public string ImdbVotes { get; set; }
            public string ImdbId { get; set; }
            public string Type { get; set; }
            public string Dvd { get; set; }
            public string BoxOffice { get; set; }
            public string Production { get; set; }
            public string Website { get; set; }
            public string Response { get; set; }
        }

        public partial class Rating
        {
            public string Source { get; set; }
            public string Value { get; set; }
        }

        static void Main(string[] args)
        {
            Config conf = new Config();
            List<Movie> movies = new List<Movie>();

            Console.Write(LOGO);
            Console.WriteLine("      FilmSniffer (v1) - Film lookup tool");
            Console.WriteLine();

            // Parse arguments
            ParseArguments(args, ref conf);

            // Check key is present
            if (conf.Key == "")
            {
                Console.WriteLine("Couldn't find an OMDb API key, specify key with '/key'");
                Console.WriteLine("(You can request a free API key here http://www.omdbapi.com/apikey.aspx)");
                return;
            }

            // Read films from file
            Console.WriteLine("Reading films from {0} ...", conf.Path);
            ReadFilmsFromFile(ref conf);

            // Exit if you can't find any films
            if (conf.Films.Count == 0)
            {
                Console.WriteLine("Couldn't find any films, exiting...");
                return;
            }

            // Lookup films 
            Console.WriteLine("Querying OMDb...");
            for (int i = 0; i < conf.Films.Count; i++)
            {
                Console.Write("Lookup {0} : ", conf.Films[i]);

                Movie m = LookupFilm(conf.Films[i], conf);

                ConsoleColor color = Console.ForegroundColor;
                if (m == null || m.Response == "False")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No film info found");
                    Console.ForegroundColor = color;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write(m.Title + " ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(m.Year + " ");
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.Write(m.Genre + " ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(m.Runtime);
                    Console.ForegroundColor = color;
                }

                // So we don't hit the nice free API too hard
                Thread.Sleep(500);
            }

            // Save to files
            Console.WriteLine("Writing results to file...");
            WriteFilmInfoToFiles(conf, movies);
        }

        static void ParseArguments(string[] args, ref Config conf)
        {

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        case "--key":
                        case "-key":
                        case "/key":
                            conf.Key = args[i + 1];
                            break;
                        case "--path":
                        case "-path":
                        case "/path":
                            conf.Path = args[i + 1];
                            break;
                        case "--seperate_files":
                        case "-seperate_files":
                        case "/seperate_files":
                        case "--seperatefiles":
                        case "-seperatefiles":
                        case "/seperatefiles":
                            conf.SeperateFiles = true;
                            break;
                        case "--help":
                        case "-help":
                        case "/help":
                        case "--h":
                        case "-h":
                        case "/h":
                        case "--?":
                        case "-?":
                        case "/?":
                            Console.WriteLine("Available args:");
                            Console.WriteLine(" /key            <string>  - (REQUIRED) OMDb API key (You can request a key here http://www.omdbapi.com/apikey.aspx)");
                            Console.WriteLine(" /path           <string>  - Path to films file (one film on each line)");
                            Console.WriteLine(" /seperatefiles            - Save some film stats to seperate CSV files");
                            Console.WriteLine(" /help                     - Displays this help text");
                            break;
                    }

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("WARNING: Problem reading argument {0} ({1}:{2}",
                        i + 1,
                        e.GetType().ToString(),
                        e.Message.ToString());
                }
            }
        }

        static Movie LookupFilm(string name, Config conf)
        {
            // Replace spaces with '+' for query
            string filteredName = name.Replace(' ', '+');

            using (WebClient client = new WebClient())
            {
                try
                {
                    string result = client.DownloadString(
                        $"http://www.omdbapi.com/?t={filteredName}&apikey={conf.Key}");
                    //var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(result);
                    Movie m = JsonSerializer.Deserialize<Movie>(result);

                    return m;
                }
                catch (Exception e)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Encountered exception fetching webrequest: {0}:{1}",
                        e.GetType().ToString(),
                        e.Message.ToString());
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }

            return null;
        }

        static void ReadFilmsFromFile(ref Config conf)
        {
            string line = "";
            using (StreamReader file = new StreamReader(conf.Path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    conf.Films.Add(line);
                }
            }
        }

        static void WriteFilmInfoToFiles(Config config, List<Movie> movies)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            // Get path (based on current location
            string path = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            // Print everything to a csv
            try
            {
                using (StreamWriter file = new StreamWriter(Path.Combine(path, "full.csv")))
                {
                    file.WriteLine("Title,Year,Rated,Released,Runtime,Genre,Director,Writer,Actors,Plot,Language,Country,Awards,Poster,Metascore,ImdbRating,Type,Dvd,BoxOffice,Production,Website");
                    for (int i = 0; i < movies.Count; i++)
                    {
                        file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}",
                            movies[i].Title,
                            movies[i].Year,
                            movies[i].Rated,
                            movies[i].Released,
                            movies[i].Runtime,
                            movies[i].Genre,
                            movies[i].Director,
                            movies[i].Writer,
                            movies[i].Actors,
                            movies[i].Plot,
                            movies[i].Language,
                            movies[i].Country,
                            movies[i].Awards,
                            movies[i].Poster,
                            movies[i].Metascore,
                            movies[i].ImdbRating,
                            movies[i].Type,
                            movies[i].Dvd,
                            movies[i].BoxOffice,
                            movies[i].Production,
                            movies[i].Website);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"All film info written to {Path.Combine(path, "full.csv")}");
                Console.ForegroundColor = originalColor;
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Encountered exception while writing to full.csv: {0}:{1}",
                    e.GetType().ToString(),
                    e.Message.ToString());
                Console.BackgroundColor = ConsoleColor.Black;
            }

            // Print some stats to seperate files
            if (config.SeperateFiles)
            {
                try
                {
                    using (StreamWriter file = new StreamWriter(Path.Combine(path, "titles.csv")))
                    {
                        file.WriteLine("Title");
                        for (int i = 0; i < movies.Count; i++)
                        {
                            file.WriteLine("{0}", movies[i].Title);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Film titles written to {Path.Combine(path, "titles.csv")}");
                    Console.ForegroundColor = originalColor;

                    using (StreamWriter file = new StreamWriter(Path.Combine(path, "years.csv")))
                    {
                        file.WriteLine("Year");
                        for (int i = 0; i < movies.Count; i++)
                        {
                            file.WriteLine("{0}", movies[i].Year);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Film years written to {Path.Combine(path, "years.csv")}");
                    Console.ForegroundColor = originalColor;

                    using (StreamWriter file = new StreamWriter(Path.Combine(path, "runtimes.csv")))
                    {
                        file.WriteLine("Runtime");
                        for (int i = 0; i < movies.Count; i++)
                        {
                            file.WriteLine("{0}", movies[i].Runtime);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Film runtimes written to {Path.Combine(path, "runtimes.csv")}");
                    Console.ForegroundColor = originalColor;

                    using (StreamWriter file = new StreamWriter(Path.Combine(path, "genres.csv")))
                    {
                        file.WriteLine("Genres");
                        for (int i = 0; i < movies.Count; i++)
                        {
                            file.WriteLine("{0}", movies[i].Genre);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Film genres written to {Path.Combine(path, "genres.csv")}");
                    Console.ForegroundColor = originalColor;

                    using (StreamWriter file = new StreamWriter(Path.Combine(path, "imdb_rating.csv")))
                    {
                        file.WriteLine("imdb_ratings");
                        for (int i = 0; i < movies.Count; i++)
                        {
                            file.WriteLine("{0}", movies[i].ImdbRating);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Film IMDB ratings written to {Path.Combine(path, "imdb_rating.csv")}");
                    Console.ForegroundColor = originalColor;
                }
                catch (Exception e)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Encountered exception while writing to seperate files: {0}:{1}",
                        e.GetType().ToString(),
                        e.Message.ToString());
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }

        }
    }
}
