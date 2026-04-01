using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_vcmadu.Data;
using Spring2026_Project3_vcmadu.Models;
using Spring2026_Project3_vcmadu.Services;
using Spring2026_Project3_vcmadu.ViewModels;

namespace Spring2026_Project3_vcmadu.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAI;
        private readonly SentimentService _sentiment;

        public ActorsController(ApplicationDbContext context,
            OpenAIService openAI, SentimentService sentiment)
        {
            _context = context;
            _openAI = openAI;
            _sentiment = sentiment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                    .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null) return NotFound();

            var movieTitles = actor.ActorMovies.Select(am => am.Movie.Title).ToList();

            var tweetTexts = await _openAI.GenerateActorTweetsAsync(actor.Name);

            var tweets = tweetTexts.Any()
                ? tweetTexts.Select(text =>
                {
                    var (label, compound) = _sentiment.Analyze(text);
                    return new TweetWithSentiment
                    {
                        TweetText = text,
                        Sentiment = label,
                        CompoundScore = compound
                    };
                }).ToList()
                : new List<TweetWithSentiment>
                {
                    new TweetWithSentiment
                    {
                        TweetText = "AI tweets unavailable at this time.",
                        Sentiment = "Neutral",
                        CompoundScore = 0
                    }
                };
            double avg = tweets.Any() ? tweets.Average(t => t.CompoundScore) : 0;

            var vm = new ActorDetailsViewModel
            {
                Actor = actor,
                MovieTitles = movieTitles,
                Tweets = tweets,
                AverageCompoundScore = avg,
                OverallSentiment = _sentiment.GetOverallLabel(avg)
            };

            return View(vm);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Gender,Age,ImdbLink")] Actor actor,
            IFormFile? photoFile)
        {
            if (photoFile != null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                actor.Photo = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Gender,Age,ImdbLink")] Actor actor,
            IFormFile? photoFile)
        {
            if (id != actor.Id) return NotFound();

            // Keep existing photo if no new one uploaded
            var existing = await _context.Actors.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (photoFile != null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                actor.Photo = ms.ToArray();
            }
            else
            {
                actor.Photo = existing?.Photo;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Actors.Any(e => e.Id == actor.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovies = _context.ActorMovies.Where(am => am.ActorId == id);
            _context.ActorMovies.RemoveRange(actorMovies);

            var actor = await _context.Actors.FindAsync(id);
            if (actor != null) _context.Actors.Remove(actor);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Serve actor photo as image
        public async Task<IActionResult> Photo(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor?.Photo == null) return NotFound();
            return File(actor.Photo, "image/jpeg");
        }
    }
}