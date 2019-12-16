using System;
namespace MoviesDB
{
    public class MovieTag
    {
        public string TagId { get; set; }
        public Tag Tag { get; set; }

        public string MovieTconstId { get; set; }
        public Movie Movie { get; set; }
    }
}
