using System.ComponentModel.DataAnnotations;

namespace MoviesDB
{
    public class links_IMDB_MovieLens
    {
        [Key]
        public string movieId { get; set; }
        public string TconstId { get; set; }
        
        public string tmdbId { get; set; }
    }
}