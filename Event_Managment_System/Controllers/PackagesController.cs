using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Event_Managment_System.Data;
using Event_Managment_System.Models;

namespace Event_Managment_System.Controllers
{
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PackagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Populate ViewBag with admin info from session
        private void SetAdminViewBag()
        {
            ViewBag.AdminId = HttpContext.Session.GetInt32("AdminId");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");
            ViewBag.AdminImage = HttpContext.Session.GetString("AdminImage");
        }


        // GET: Packages
        public async Task<IActionResult> Index()
        {
            SetAdminViewBag();

            var applicationDbContext = _context.Packages.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Packages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var packages = await _context.Packages
                .Include(p => p.Category)
                .Include(p => p.PackageFeatures)
                .FirstOrDefaultAsync(m => m.PackageId == id);
            if (packages == null)
            {
                return NotFound();
            }

            return View(packages);
        }

 
        // GET: Packages/Create
        public IActionResult Create()
        {
            SetAdminViewBag();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Packages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Packages packages)
        {
            try
            {
                _context.Packages.Add(packages);

                if (packages.PackageFeatures != null)
                {
                    foreach (var feature in packages.PackageFeatures)
                    {
                        feature.PackageId = packages.PackageId;
                        _context.PackageFeatures.Add(feature);
                    }
                }

                _context.SaveChanges();
                TempData["SuccessMsg"] = "Package created successfully!";
            }
            catch (Exception ex)
            {
                TempData["FailureMsg"] = "Error creating package: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Packages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var packages = await _context.Packages
                .Include(p => p.PackageFeatures)
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (packages == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", packages.CategoryId);

            return View(packages);
        }


        // POST: Packages/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Packages model)
        {
            try
            {
                if (id != model.PackageId)
                {
                    TempData["FailureMsg"] = "Package not found!";
                    return RedirectToAction(nameof(Index));
                }

                var package = await _context.Packages
                    .Include(p => p.PackageFeatures)
                    .FirstOrDefaultAsync(p => p.PackageId == id);

                if (package == null)
                {
                    TempData["FailureMsg"] = "Package not found!";
                    return RedirectToAction(nameof(Index));
                }

                // Update package fields
                package.Name = model.Name;
                package.price = model.price;
                package.CategoryId = model.CategoryId;

                // Delete old features
                _context.PackageFeatures.RemoveRange(package.PackageFeatures);

                // Add new features
                if (model.PackageFeatures != null)
                {
                    foreach (var feature in model.PackageFeatures)
                    {
                        package.PackageFeatures.Add(new PackageFeatures
                        {
                            Name = feature.Name,
                            PackageId = id
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Package updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["FailureMsg"] = "Error updating package: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Packages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            SetAdminViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var packages = await _context.Packages
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PackageId == id);
            if (packages == null)
            {
                return NotFound();
            }

            return View(packages);
        }

        // POST: Packages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var packages = await _context.Packages.FindAsync(id);
                if (packages != null)
                {
                    _context.Packages.Remove(packages);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Package deleted successfully!";
                }
                else
                {
                    TempData["FailureMsg"] = "Package not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["FailureMsg"] = "Error deleting package: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PackagesExists(int id)
        {
            return _context.Packages.Any(e => e.PackageId == id);
        }
    }
}
