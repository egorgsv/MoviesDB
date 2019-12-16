using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MoviesDB
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {


                Console.WriteLine("Would you like to reload database? (y/n)");
                bool reload = Console.ReadLine().Equals("y");

                Stopwatch stopwatch2 = Stopwatch.StartNew();
                if (false/*reload*/)
                {
                    Console.WriteLine("Creating new Database. Please, don't stop the program.");
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    //movies
                    string MovieCodes_IMDB = @"/Users/egorgusev/ml-latest/MovieCodes_IMDB.tsv";
                    Dictionary<string, Movie> DictMovieCodes_IMDB = new Dictionary<string, Movie>();
                    using (StreamReader sr = File.OpenText(MovieCodes_IMDB))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split("	");
                            Movie movie = new Movie() {TconstId = a[0], Title = a[2]};
                            if ((a[3] == "RU" || a[3] == "US") && !DictMovieCodes_IMDB.ContainsKey(a[0]))
                            {
                                DictMovieCodes_IMDB.Add(a[0], movie);
                            }

                        }
                    }

                    string links_IMDB_MovieLens = @"/Users/egorgusev/ml-latest/links_IMDB_MovieLens.csv";
                    using (StreamReader sr = File.OpenText(links_IMDB_MovieLens))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split(",");

                            if (DictMovieCodes_IMDB.ContainsKey($"tt{a[1]}"))
                            {
                                var link = new links_IMDB_MovieLens()
                                    {movieId = a[0], TconstId = $"tt{a[1]}", tmdbId = a[2]};
                                db.links_IMDB_MovieLens.Add(link);
                                DictMovieCodes_IMDB[$"tt{a[1]}"].MovieId = a[0];
                                DictMovieCodes_IMDB[$"tt{a[1]}"].tmdbId = a[2];
                                DictMovieCodes_IMDB[$"tt{a[1]}"].link = link;
                            }
                        }
                    }

                    foreach (var movie in DictMovieCodes_IMDB)
                    {
                        db.Movies.Add(movie.Value);
                    }

                    db.SaveChanges();
                    Console.WriteLine("Movies downloaded successfully");

                    
                    string ActorsDirectorsNames_IMDB = @"/Users/egorgusev/ml-latest/ActorsDirectorsNames_IMDB.txt";
                    using (StreamReader sr = new StreamReader(ActorsDirectorsNames_IMDB))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split("	");
                            db.Actors.Add(new Actor() {NconstId = a[0], Name = a[1]});
                        }
                    }
                    db.SaveChanges();


                    string actorsDirectorsCodes_IMDB =
                        @"/Users/egorgusev/ml-latest/ActorsDirectorsCodes_IMDB.tsv";
                    Dictionary<string, string> dictMovieActors = new Dictionary<string, string>();
                    using (StreamReader sr = new StreamReader(actorsDirectorsCodes_IMDB))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split("	");
                            if (a[3] == "actor")
                            {
                                if (db.Movies.Find(a[0]) != null && db.Actors.Find(a[2]) != null && !dictMovieActors.ContainsKey($"{a[0]}{a[2]}")) 
                                {
                                    var actor = db.Actors.Find(a[2]);
                                    var movie = db.Movies.Find(a[0]);
                                    var movieactor = new MovieActor ()
                                        {ActorNconstId = actor.NconstId, MovieTconstId = movie.TconstId};
                                    dictMovieActors.Add($"{a[0]}{a[2]}", a[2]);
                                    movie.MovieActors.Add(movieactor);
                                    
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                    Console.WriteLine("Actors downloaded successfully");

                    //загружаем теги
                    string TagCodes_MovieLens = @"/Users/egorgusev/ml-latest/TagCodes_MovieLens.csv";
                    using (StreamReader sr = File.OpenText(TagCodes_MovieLens))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split(",");
                            db.Tags.Add(new Tag {tagId = a[0], tag = a[1]});
                        }
                    }

                    db.SaveChanges();

                    string TagScores_MovieLens = @"/Users/egorgusev/ml-latest/TagScores_MovieLens.csv";
                    using (StreamReader sr = File.OpenText(TagScores_MovieLens))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split(",");
                            if (Double.Parse(a[2], CultureInfo.InvariantCulture) > 0.5)
                            {
                                Tag tag = db.Tags.Find(a[1]); //ищем тег
                                if (db.links_IMDB_MovieLens.Find(a[0]) != null)
                                {
                                    Movie movie = db.Movies.Find(db.links_IMDB_MovieLens.Find(a[0]).TconstId);
                                    movie.MovieTags.Add(
                                        new MovieTag {MovieTconstId = movie.TconstId, TagId = tag.tagId});
                                }

                            }
                        }
                    }

                    db.SaveChanges();
                    Console.WriteLine("Tags downloaded successfully");

                    string Ratings_IMDB = @"/Users/egorgusev/ml-latest/Ratings_IMDB.tsv";
                    using (StreamReader sr = File.OpenText(Ratings_IMDB))
                    {
                        string line;
                        sr.ReadLine(); //первая строка с названиями 
                        while ((line = sr.ReadLine()) != null)
                        {
                            var a = line.Split("	");
                            if (DictMovieCodes_IMDB.ContainsKey(a[0]))
                            {
                                db.Movies.Find(a[0]).Rating = a[1];
                            }
                        }
                    }
                }

                db.SaveChanges();

                Console.WriteLine("Done.");
                stopwatch2.Stop();
                Console.WriteLine($"Seconds: {stopwatch2.ElapsedMilliseconds / 1000}");
                Console.WriteLine($"Minutes: {stopwatch2.ElapsedMilliseconds / 60000}");

                String command;
                do
                {
                    Console.WriteLine("Input comand (film, person, tag, exit):");
                    command = Console.ReadLine();

                    if (command.Equals("exit"))
                    {
                        continue;
                    }

                    if (command.Equals("film"))
                    {
                        Console.WriteLine("Enter the title:");
                        string title = Console.ReadLine();
                        var movies = (from m in db.Movies
                            where m.Title == title
                            select m).ToList();
                        foreach (var m in movies)
                        {
                            Console.WriteLine($"\nTitle: {m.Title}");
                            if (m.Rating != null)
                            {
                                Console.WriteLine($" Reting: {m.Rating}");
                            }

                            var actors = m.MovieActors.Select(ma => ma.Actor).ToList();
                            foreach (Actor a in actors)
                                Console.WriteLine($" Actor: {a.Name}");
                            var tags = m.MovieTags.Select(mt => mt.Tag).ToList();
                            foreach (Tag t in tags)
                                Console.WriteLine($" Tag: {t.tag}");
                            Console.WriteLine($"____________________________");
                        }

                        Console.WriteLine("Show similar movies? (y/n)");
                        if (Console.ReadLine().Equals("y"))
                        {
                            foreach (var m in movies)
                            {
                                Console.WriteLine($"\nTitle: {m.Title}");
                                Console.WriteLine(" Similar:");
                                var similar = m.similar();
                                foreach (var VARIABLE in similar)
                                {
                                    Console.WriteLine($"\n   Title: {VARIABLE.Title}");
                                }
                            }
                        }
                    }
                    else if (command.Equals("actor"))
                    {
                        Console.WriteLine("Enter the name:");
                        string name = Console.ReadLine();
                        List<Actor> actors = (from a in db.Actors
                            where a.Name == name
                            select a).ToList();
                        foreach (Actor a in actors)
                        {
                            Console.WriteLine($"\nActor: {a.Name}");
                            var movies = a.MovieActors.Select(ma => ma.Movie).ToList();
                            foreach (Movie m in movies)
                            {
                                Console.WriteLine($" Movie: {m.Title} (rating: {m.Rating})");
                            }

                            Console.WriteLine($"____________________________");
                        }
                    }
                    else if (command.Equals("tag"))
                    {
                        Console.WriteLine("Enter the tag:");
                        string tag = Console.ReadLine();
                        List<Tag> tags = (from t in db.Tags
                            where t.tag == tag
                            select t).ToList();
                        foreach (Tag t in tags)
                        {
                            Console.WriteLine($"\nTag: {t.tag}");
                            var movies = t.MovieTags.Select(mt => mt.Movie).ToList();
                            foreach (Movie m in movies)
                                Console.WriteLine($" Movie: {m.Title} (rating: {m.Rating})");
                            Console.WriteLine($"____________________________");
                        }
                    }
                } while (!command.Equals("exit"));

                Console.WriteLine("Press any key.");
                Console.ReadKey();
            }
        }
    }
}
