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
                        Movie movie = new Movie() { TconstId = a[0], Title = a[2]};
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
                        db.Tags.Add(new Tag { tagId = a[0], tag = a[1] });
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
                                movie.MovieTags.Add(new MovieTag { MovieTconstId = movie.TconstId, TagId = tag.tagId });
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

                //Tag tag1 = new Tag { tag = "'80", tagId = "1" };
                //Tag tag2 = new Tag { tag = "triller", tagId = "2" };

                //добавляем их в бд
                //db.Movies.AddRange(new HashSet<Movie> { movie1, movie2 } );
                db.Actors.AddRange(new HashSet<Actor> { actor1, actor2 } );
                //db.Tags.AddRange(new HashSet<Tag> { tag1, tag2 });
                db.SaveChanges();

                

                actor1.MovieActors.Add(new MovieActor { ActorNconstId = actor1.NconstId, MovieTconstId = db.Movies.Find("tt0000001").TconstId });
                //actor1.MovieActors.Add(new MovieActor { ActorNconstId = actor1.NconstId, MovieTconstId = movie2.TconstId });
                //actor2.MovieActors.Add(new MovieActor { ActorNconstId = actor2.NconstId, MovieTconstId = movie2.TconstId });

                //movie1.MovieTags.Add(new MovieTag { MovieTconstId = movie1.TconstId, TagId = tag2.tagId });
                //movie2.MovieTags.Add(new MovieTag { MovieTconstId = movie2.TconstId, TagId = tag1.tagId });
                //movie2.MovieTags.Add(new MovieTag { MovieTconstId = movie2.TconstId, TagId = tag2.tagId });

                db.SaveChanges();
                stopwatch2.Stop();
                Console.WriteLine(stopwatch2.ElapsedMilliseconds/1000);
                Console.WriteLine("Объекты успешно сохранены");

                var Movies = db.Movies.Include(movie => movie.MovieActors).ThenInclude(ma => ma.Actor).ToList();

                //получаем объекты из бд и выводим на консоль
                Console.WriteLine("Список объектов:");
                foreach (Movie m in Movies)
                {
                    Console.WriteLine($"\nMovie: {m.Title}");
                    var actors = m.MovieActors.Select(ma => ma.Actor).ToList();
                    foreach (Actor a in actors)
                        Console.WriteLine($" Actor: {a.Name}");
                    var tags = m.MovieTags.Select(mt => mt.Tag).ToList();
                    foreach (Tag t in tags)
                        Console.WriteLine($" Tag: {t.tag}");

                }

                var Actors = db.Actors.Include(actor => actor.MovieActors).ThenInclude(ma => ma.Movie).ToList();
                Console.WriteLine("Список объектов:");
                foreach (Actor a in Actors)
                {
                    Console.WriteLine($"\nActor: {a.Name}");
                    var movies = a.MovieActors.Select(ma => ma.Movie).ToList();
                    foreach (Movie m in movies)
                        Console.WriteLine($" Movie: {m.Title}");
                }
                Console.WriteLine();

                //// создаем два объекта Movie
                ////Movie movie1 = new Movie { Title = "Star Wars", TconstId = "1" };
                ////Movie movie2 = new Movie { Title = "Tor", TconstId = "2" };

                //Actor actor1 = new Actor
                //{
                //    NconstId = "1235",
                //    Name = "Dan",
                //};
                //Actor actor2 = new Actor
                //{
                //    NconstId = "123574",
                //    Name = "Kate",
                //};

                ////Tag tag1 = new Tag { tag = "'80", tagId = "1" };
                ////Tag tag2 = new Tag { tag = "triller", tagId = "2" };

                ////добавляем их в бд
                ////db.Movies.AddRange(new HashSet<Movie> { movie1, movie2 } );
                ////db.Actors.AddRange(new HashSet<Actor> { actor1, actor2 } );
                ////db.Tags.AddRange(new HashSet<Tag> { tag1, tag2 });
                //db.SaveChanges();



                //actor1.MovieActors.Add(new MovieActor { ActorNconstId = actor1.NconstId, MovieTconstId = movie1.TconstId });
                //actor1.MovieActors.Add(new MovieActor { ActorNconstId = actor1.NconstId, MovieTconstId = movie2.TconstId });
                //actor2.MovieActors.Add(new MovieActor { ActorNconstId = actor2.NconstId, MovieTconstId = movie2.TconstId });

                ////movie1.MovieTags.Add(new MovieTag { MovieTconstId = movie1.TconstId, TagId = tag2.tagId });
                ////movie2.MovieTags.Add(new MovieTag { MovieTconstId = movie2.TconstId, TagId = tag1.tagId });
                ////movie2.MovieTags.Add(new MovieTag { MovieTconstId = movie2.TconstId, TagId = tag2.tagId });

                //db.SaveChanges();

                //var Movies = db.Movies.Include(movie => movie.MovieActors).ThenInclude(ma => ma.Actor).ToList();
                //Console.WriteLine("Объекты успешно сохранены");

                ////получаем объекты из бд и выводим на консоль
                //Console.WriteLine("Список объектов:");
                //foreach (Movie m in Movies)
                //{
                //    Console.WriteLine($"\nMovie: {m.Title}");
                //    var actors = m.MovieActors.Select(ma => ma.Actor).ToList();
                //    foreach (Actor a in actors)
                //        Console.WriteLine($" Actor: {a.Name}");
                //    var tags = m.MovieTags.Select(mt => mt.Tag).ToList();
                //    foreach (Tag t in tags)
                //        Console.WriteLine($" Tag: {t.tag}");

                //}

                //var Actors = db.Actors.Include(actor => actor.MovieActors).ThenInclude(ma => ma.Movie).ToList();
                //Console.WriteLine("Список объектов:");
                //foreach (Actor a in Actors)
                //{
                //    Console.WriteLine($"\nActor: {a.Name}");
                //    var movies = a.MovieActors.Select(ma => ma.Movie).ToList();
                //    foreach (Movie m in movies)
                //        Console.WriteLine($" Movie: {m.Title}");
                //}

            }
            Console.Read();
        }
    }
    
}
