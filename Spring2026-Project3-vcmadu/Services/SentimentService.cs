using VaderSharp2;

namespace Spring2026_Project3_vcmadu.Services
{
    public class SentimentService
    {
        private readonly SentimentIntensityAnalyzer _analyzer = new();

        public (string label, double compound) Analyze(string text)
        {
            var results = _analyzer.PolarityScores(text);
            double compound = results.Compound;

            string label = compound >= 0.05 ? "Positive"
                         : compound <= -0.05 ? "Negative"
                         : "Neutral";

            return (label, compound);
        }

        public string GetOverallLabel(double averageCompound)
        {
            return averageCompound >= 0.05 ? "Overall Positive 😊"
                 : averageCompound <= -0.05 ? "Overall Negative 😞"
                 : "Overall Neutral 😐";
        }
    }
}