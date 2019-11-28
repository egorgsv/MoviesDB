using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoviesDB
{
    //Класс Movie имеет члены: название фильма, множество (set) участвующих в нем актеров,
    //режиссер, множество тэгов этого фильма, рейтинг фильма.
    public class Movie
    {
        //public Movie(string title, string tconstId, string movieId = "1") : this()
        //{
        //    Title = title;
        //    TconstId = tconstId;
        //    MovieId = movieId;
        //}

        //public Movie(string title = "1", string rating = "1", string tconstId = "1", string movieId = "1241")
        //{
        //    Title = title;
        //    Rating = rating;
        //    TconstId = tconstId;
        //    MovieId = movieId;
        //}

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
    }
}
