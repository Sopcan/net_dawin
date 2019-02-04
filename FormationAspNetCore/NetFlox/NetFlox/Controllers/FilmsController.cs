using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetFlox.DAL;

namespace NetFlox.Controllers
{
    public class FilmsController : Controller
    {
        private readonly NetFloxEntities _context;

        public FilmsController(NetFloxEntities context)
        {
            _context = context;
        }

        // GET: Films
        public async Task<IActionResult> Index()
        {
            var films = await _context.Films
                .OrderByDescending(f => f.Annee)
                .ToListAsync();
            return View(films);
        }

        // GET: Films/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.RoleCelebriteFilms)
                .ThenInclude(rcf => rcf.Role)
                .Include(f => f.RoleCelebriteFilms)
                .ThenInclude(rcf => rcf.Celebrite)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        public async Task<IActionResult> Search([FromQuery]String film)
        {

            var movies = from m in _context.Films
                         select m;

            if (!String.IsNullOrEmpty(film))
            {
                movies = movies.Where(s => EF.Functions.Like(s.Titre, "%" + film + "%"));
            }
            var nb = movies.Count();
            
            ViewData["nb"] = nb;
            return View(await movies.ToListAsync());
        }
    }
}
