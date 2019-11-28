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

        //public Actor(string nconstId, string name)
        //{
        //    NconstId = nconstId;
        //    Name = name;
        //}

        //public Actor(string nconstId, string name, Movie movie)
        //{
        //    NconstId = nconstId;
        //    Name = name;
        //}

        
        //private string primaryProfession;
        //private string knownForTitles;
        //private string[] profession;


    }
}
