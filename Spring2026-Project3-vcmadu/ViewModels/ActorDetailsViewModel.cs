namespace Spring2026_Project3_vcmadu.ViewModels
{
    public class TweetWithSentiment
    {
        public string TweetText { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public double CompoundScore { get; set; }
    }

    public class ActorDetailsViewModel
    {
        public Models.Actor Actor { get; set; } = null!;
        public List<string> MovieTitles { get; set; } = new();
        public List<TweetWithSentiment> Tweets { get; set; } = new();
        public string OverallSentiment { get; set; } = string.Empty;
        public double AverageCompoundScore { get; set; }
    }
}