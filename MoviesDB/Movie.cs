using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace MoviesDB
{
    //Класс Movie имеет члены: название фильма, множество (set) участвующих в нем актеров,
    //режиссер, множество тэгов этого фильма, рейтинг фильма.
    public class Movie
    {
        public Movie()
        {
            MovieActors = new HashSet<MovieActor>();
            MovieTags = new HashSet<MovieTag>();
        }

        [Key]
        public string TconstId { get; set; }

        public string Title { get; set; }

        public HashSet<MovieActor> MovieActors { get; set; }
        public HashSet<MovieTag> MovieTags { get; set; }
        public string Rating { get; set; }
        public string MovieId { get; set; }   
        
        public links_IMDB_MovieLens link { get; set; }

        public HashSet<Movie> similar()
        {
            HashSet<Movie> simular_movies = new HashSet<Movie>();
            var client = new RestClient($"https://api.themoviedb.org/3/movie/{MovieId}/similar?page=1&language=en-US&api_key=7869fcb463cd3b1f40161bb8d85c011f");
            var request = new RestRequest(Method.GET);
            request.AddParameter("undefined", "{}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                var content = response.Content;
                var data = JsonConvert.DeserializeObject<JObject>(content);
                var results = data["results"];
                foreach (var movie in results)
                {
                    simular_movies.Add(new Movie() {MovieId = movie["id"].ToString(), Title = movie["title"].ToString()});
                }
            }

            return simular_movies;
        }
    }
}
