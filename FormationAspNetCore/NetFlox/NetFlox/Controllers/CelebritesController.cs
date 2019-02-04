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
    public class CelebritesController : Controller
    {
        private readonly NetFloxEntities _context;

        public CelebritesController(NetFloxEntities context)
        {
            _context = context;
        }

        // GET: Celebrites
        public async Task<IActionResult> Index()
        {
            var celebrites = await _context.Celebrites
                .OrderBy(c => c.Nom)
                .ToListAsync();
            return View(celebrites);
        }

        // GET: Celebrites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var celebrite = await _context.Celebrites
                .Include(c => c.RoleCelebriteFilms)
                .ThenInclude(rcf => rcf.Role)
                .Include(c => c.RoleCelebriteFilms)
                .ThenInclude(rcf => rcf.Film)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (celebrite == null)
            {
                return NotFound();
            }

            return View(celebrite);
        }

        public async Task<IActionResult> Search([FromQuery] String celeb)
        {

            var celebs = from m in _context.Celebrites
                         select m;

            if (!String.IsNullOrEmpty(celeb))
            {
                celebs = celebs.Where(s => EF.Functions.Like(s.Nom, "%" + celeb + "%"));
            }

            var nb = celebs.Count();
            ViewData["nb"] = nb;
            return View(await celebs.ToListAsync());
        }
    }
}
