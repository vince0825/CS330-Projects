using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_vcmadu.Data;
using Spring2026_Project3_vcmadu.Models;
using Spring2026_Project3_vcmadu.Services;
using Spring2026_Project3_vcmadu.ViewModels;

namespace Spring2026_Project3_vcmadu.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAI;
        private readonly SentimentService _sentiment;

        public MoviesController(ApplicationDbContext context,
            OpenAIService openAI, SentimentService sentiment)
        {
            _context = context;
            _openAI = openAI;
            _sentiment = sentiment;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            // Call OpenAI once, get 5 reviews
            var reviewTexts = await _openAI.GenerateMovieReviewsAsync(movie.Title);

            var reviews = reviewTexts.Any()
                ? reviewTexts.Select(text =>
                {
                    var (label, compound) = _sentiment.Analyze(text);
                    return new ReviewWithSentiment
                    {
                        ReviewText = text,
                        Sentiment = label,
                        CompoundScore = compound
                    };
                }).ToList()
                : new List<ReviewWithSentiment>
                {
                    new ReviewWithSentiment
                    {
                        ReviewText = "AI reviews unavailable at this time.",
                        Sentiment = "Neutral",
                        CompoundScore = 0
                    }
                };
            double avg = reviews.Any() ? reviews.Average(r => r.CompoundScore) : 0;

            var vm = new MovieDetailsViewModel
            {
                Movie = movie,
                Reviews = reviews,
                AverageCompoundScore = avg,
                OverallSentiment = _sentiment.GetOverallLabel(avg)
            };

            return View(vm);
        }

        // GET: Movies/Create
        public IActionResult Create() => View();

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Genre,ReleaseDate,ImdbLink,Description")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Title,Genre,ReleaseDate,ImdbLink,Description")] Movie movie)
        {
            if (id != movie.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Movies.Any(e => e.Id == movie.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Remove related ActorMovies first to prevent crash
            var actorMovies = _context.ActorMovies.Where(am => am.MovieId == id);
            _context.ActorMovies.RemoveRange(actorMovies);

            var movie = await _context.Movies.FindAsync(id);
            if (movie != null) _context.Movies.Remove(movie);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}