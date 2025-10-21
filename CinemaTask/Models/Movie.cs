namespace CinemaTask.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }
        public string? MainImage { get; set; }
        public string? SubImage  { get; set; }
        public DateTime DateTime { get; set; }
        public List<Actor>? Actors { get; set; }
        public int CinemaId { get; set; }
        public int CategoryId { get; set; }
        public int ActorId { get; set; }

    }
}
