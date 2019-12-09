using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MoviesDB
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Stopwatch stopwatch2 = Stopwatch.StartNew();

                stopwatch2.Stop();
                //Console.WriteLine(stopwatch2.ElapsedMilliseconds / 1000);



                Console.WriteLine("Обновить базу данных? да/нет");
                bool reload = Console.ReadLine().Equals("да");

                if (reload)
                {
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
                                DictMovieCodes_IMDB[$"tt{a[1]}"].MovieId = a[0];
                                db.Movies.Add(DictMovieCodes_IMDB[$"tt{a[1]}"]);
                            }
                        }
                    }

                    db.SaveChanges();
                    Console.WriteLine("Фильмы успешно сохранены");

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
                            if (Double.Parse(a[2], CultureInfo.InvariantCulture) > 0.99)
                            {
                                Tag tag = db.Tags.Find(a[1]); //ищем тег
                                List<Movie> movies = (from m in db.Movies.AsParallel()
                                    where m.MovieId == $"{a[0]}"
                                    select m).ToList(); //ищем все фильмы с нужным нам MovieId
                                foreach (var movie in movies) //создаём связи
                                {
                                    movie.MovieTags.Add(
                                        new MovieTag {MovieTconstId = movie.TconstId, TagId = tag.tagId});
                                }
                            }
                        }
                    }
                    db.SaveChanges();

                    Actor actor1 = new Actor
                    {
                        NconstId = "1235",
                        Name = "Dan",
                    };
                    Actor actor2 = new Actor
                    {
                        NconstId = "123574",
                        Name = "Kate",
                    };



                    //добавляем их в бд

                    db.Actors.AddRange(new HashSet<Actor> {actor1, actor2});
                    db.SaveChanges();



                    actor1.MovieActors.Add(new MovieActor
                        {ActorNconstId = actor1.NconstId, MovieTconstId = db.Movies.Find("tt0000001").TconstId});
                    db.SaveChanges();
                }

                Console.WriteLine("Загрузка завершена");

                String command = null, request = null;
                Object response = null;
                do
                {
                    Console.WriteLine("Введите комманду (film, actor, tag, exit):");
                    command = Console.ReadLine();

                    if (command.Equals("exit"))
                    {
                        continue;
                    }

                    if (command.Equals("film"))
                    {
                        Console.WriteLine("Введите название фильма:");
                        string title = Console.ReadLine();
                        List<Movie> movies = (from m in db.Movies.AsParallel()
                            where m.Title == title
                            select m).ToList();
                        foreach (var m in movies)
                        {
                            Console.WriteLine($"\nTitle: {m.Title}");
                            var actors = m.MovieActors.Select(ma => ma.Actor).ToList();
                            foreach (Actor a in actors)
                                Console.WriteLine($" Actor: {a.Name}");
                            var tags = m.MovieTags.Select(mt => mt.Tag).ToList();
                            foreach (Tag t in tags)
                                Console.WriteLine($" Tag: {t.tag}");
                        }
                    }
                    else if (command.Equals("actor"))
                    {
                        Console.WriteLine("Введите имя актёра:");
                        string name = Console.ReadLine();
                        List<Actor> actors = (from a in db.Actors.AsParallel()
                            where a.Name == name
                            select a).ToList();
                        foreach (Actor a in actors)
                        {
                            Console.WriteLine($"\nActor: {a.Name}");
                            var movies = a.MovieActors.Select(ma => ma.Movie).ToList();
                            foreach (Movie m in movies)
                                Console.WriteLine($" Movie: {m.Title}");
                        }
                    }
                    else if (command.Equals("tag"))
                    {
                        Console.WriteLine("Введите тег:");
                        string tag = Console.ReadLine();
                        List<Tag> tags = (from t in db.Tags.AsParallel()
                            where t.tag == tag
                            select t).ToList();
                        foreach (Tag t in tags)
                        {
                            Console.WriteLine($"\nTag: {t.tag}");
                            var movies = t.MovieTags.Select(mt => mt.Movie).ToList();
                            foreach (Movie m in movies)
                                Console.WriteLine($" Movie: {m.Title}");
                        }
                    }
                } while (!command.Equals("exit"));

                Console.WriteLine("Нажмите любую кнопку.");
                Console.ReadKey();
            }
        }
    }
    
}
