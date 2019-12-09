using System;
using System.Collections.Generic;

namespace MoviesDB
{
    public class Tag
    {
        public string tag { get; set; }
        public string tagId { get; set; }
        public HashSet<MovieTag> MovieTags { get; set; }

        public Tag()
        {
            MovieTags = new HashSet<MovieTag>();
        }
    }
}
