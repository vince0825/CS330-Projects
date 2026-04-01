using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_vcmadu.Data;
using Spring2026_Project3_vcmadu.Models;

namespace Spring2026_Project3_vcmadu.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var actorMovies = await _context.ActorMovies
                .Include(am => am.Actor)
                .Include(am => am.Movie)
                .ToListAsync();
            return View(actorMovies);
        }

        public IActionResult Create()
        {
            ViewBag.Actors = new SelectList(_context.Actors, "Id", "Name");
            ViewBag.Movies = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ActorId, int MovieId)
        {
            if (ActorId == 0 || MovieId == 0)
            {
                ModelState.AddModelError("", "Please select both an actor and a movie.");
                ViewBag.Actors = new SelectList(_context.Actors, "Id", "Name");
                ViewBag.Movies = new SelectList(_context.Movies, "Id", "Title");
                return View();
            }

            bool exists = await _context.ActorMovies.AnyAsync(
                am => am.ActorId == ActorId && am.MovieId == MovieId);

            if (exists)
            {
                ModelState.AddModelError("", "This actor-movie relationship already exists.");
                ViewBag.Actors = new SelectList(_context.Actors, "Id", "Name");
                ViewBag.Movies = new SelectList(_context.Movies, "Id", "Title");
                return View();
            }

            var actorMovie = new ActorMovie { ActorId = ActorId, MovieId = MovieId };
            _context.Add(actorMovie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies
                .Include(am => am.Actor)
                .Include(am => am.Movie)
                .FirstOrDefaultAsync(am => am.Id == id);

            if (actorMovie == null) return NotFound();
            return View(actorMovie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie != null) _context.ActorMovies.Remove(actorMovie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}