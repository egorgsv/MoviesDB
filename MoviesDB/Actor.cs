using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoviesDB
{
    public class Actor
    {
        [Key]
        public string NconstId { get; set; }

        public HashSet<MovieActor> MovieActors { get; set; }

        public string Name { get; set; }

        public Actor()
        {
            MovieActors = new HashSet<MovieActor>();
        }
    }
}
