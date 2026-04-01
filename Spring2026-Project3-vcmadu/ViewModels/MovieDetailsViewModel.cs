namespace Spring2026_Project3_vcmadu.ViewModels
{
    public class ReviewWithSentiment
    {
        public string ReviewText { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public double CompoundScore { get; set; }
    }

    public class MovieDetailsViewModel
    {
        public Models.Movie Movie { get; set; } = null!;
        public List<ReviewWithSentiment> Reviews { get; set; } = new();
        public string OverallSentiment { get; set; } = string.Empty;
        public double AverageCompoundScore { get; set; }
    }
}