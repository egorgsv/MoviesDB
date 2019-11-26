using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesDB
{
    public class MovieActor
    {
        public string ActorNconstId { get; set; }
        public Actor Actor { get; set; }

        public string MovieTconstId { get; set; }
        public Movie Movie { get; set; }
        public MovieActor()
        {
        }
    }
}
