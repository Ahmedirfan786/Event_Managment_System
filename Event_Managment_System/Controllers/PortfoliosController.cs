using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Event_Managment_System.Data;
using Event_Managment_System.Models;
using Microsoft.Extensions.Hosting;

namespace Event_Managment_System.Controllers
{
    public class PortfoliosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PortfoliosController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }


        // Helper: Populate ViewBag with admin info from session
        private void SetAdminViewBag()
        {
            ViewBag.AdminId = HttpContext.Session.GetInt32("AdminId");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");
            ViewBag.AdminImage = HttpContext.Session.GetString("AdminImage");
        }



        // GET: Portfolios
        public async Task<IActionResult> Index()
        {
            SetAdminViewBag();

            var applicationDbContext = _context.Portfolios.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Portfolios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var portfolios = await _context.Portfolios
                .Include(p => p.Category)
                .Include(p => p.PortfolioImages)
                .FirstOrDefaultAsync(m => m.PortfolioId == id);
            if (portfolios == null)
            {
                return NotFound();
            }

            return View(portfolios);
        }

        // GET: Portfolios/Create
        public IActionResult Create()
        {
            SetAdminViewBag();

            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Portfolios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Portfolios portfolio, List<IFormFile> AdditionalImages)
        {
            ModelState.Remove("ImagePath");
            ModelState.Remove("Category");
            ModelState.Remove("PortfolioImages");

            if (ModelState.IsValid)
            {
                try
                {
                    // --- Main Image ---
                    if (portfolio.Image != null)
                    {
                        string folder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "PortfolioImages");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8) + "_" + portfolio.Image.FileName;
                        string filePath = Path.Combine(folder, fileName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                            portfolio.Image.CopyTo(fs);

                        portfolio.ImagePath = "/Dashboard/PortfolioImages/" + fileName;
                    }

                    // --- Save Portfolio ---
                    _context.Portfolios.Add(portfolio);
                    _context.SaveChanges();

                    // --- Additional Images ---
                    if (AdditionalImages != null)
                    {
                        foreach (var file in AdditionalImages)
                        {
                            if (file.Length > 0)
                            {
                                string folder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "PortfolioImages");
                                string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8) + "_" + file.FileName;
                                string filePath = Path.Combine(folder, fileName);

                                using (var fs = new FileStream(filePath, FileMode.Create))
                                    file.CopyTo(fs);

                                _context.PortfolioImages.Add(new PortfolioImages
                                {
                                    PortfolioId = portfolio.PortfolioId,
                                    ImagePath = "/Dashboard/PortfolioImages/" + fileName
                                });
                            }
                        }
                        _context.SaveChanges();
                    }

                    TempData["SuccessMsg"] = "Portfolio created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error creating portfolio: " + ex.Message;
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(portfolio);
                }
            }

            TempData["FailureMsg"] = "Invalid data! Please check your inputs.";

            ViewBag.Categories = _context.Categories.ToList(); // MUST repopulate
            return View(portfolio);
        }


        // GET: Portfolios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            SetAdminViewBag();

            if (id == null) return NotFound();

            var portfolio = await _context.Portfolios
                .Include(p => p.Category)
                .Include(p => p.PortfolioImages)
                .FirstOrDefaultAsync(m => m.PortfolioId == id);

            if (portfolio == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", portfolio.CategoryId);
            return View(portfolio);
        }

        // POST: Portfolios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Portfolios portfolio, List<IFormFile> NewAdditionalImages)
        {
            if (id != portfolio.PortfolioId)
            {
                TempData["FailureMsg"] = "Portfolio not found!";
                return NotFound();
            }

            ModelState.Remove("ImagePath");
            ModelState.Remove("Category");
            ModelState.Remove("PortfolioImages");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Portfolios
                        .Include(p => p.PortfolioImages)
                        .FirstOrDefaultAsync(p => p.PortfolioId == id);

                    if (existing == null)
                    {
                        TempData["FailureMsg"] = "Portfolio not found!";
                        return NotFound();
                    }

                    // Update basic fields
                    existing.Name = portfolio.Name;
                    existing.Description = portfolio.Description;
                    existing.Budget = portfolio.Budget;
                    existing.Location = portfolio.Location;
                    existing.EventDate = portfolio.EventDate;
                    existing.CategoryId = portfolio.CategoryId;

                    // Update main image if new one uploaded
                    if (portfolio.Image != null)
                    {
                        if (!string.IsNullOrEmpty(existing.ImagePath))
                        {
                            string oldPath = Path.Combine(_hostEnvironment.WebRootPath,
                                existing.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        string folder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "PortfolioImages");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8) + "_" + portfolio.Image.FileName;
                        string filePath = Path.Combine(folder, fileName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                            await portfolio.Image.CopyToAsync(fs);

                        existing.ImagePath = "/Dashboard/PortfolioImages/" + fileName;
                    }

                    // Add new additional images
                    if (NewAdditionalImages != null && NewAdditionalImages.Count > 0)
                    {
                        string folder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "PortfolioImages");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        foreach (var file in NewAdditionalImages)
                        {
                            if (file.Length > 0)
                            {
                                string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8) + "_" + file.FileName;
                                string filePath = Path.Combine(folder, fileName);

                                using (var fs = new FileStream(filePath, FileMode.Create))
                                    await file.CopyToAsync(fs);

                                existing.PortfolioImages.Add(new PortfolioImages
                                {
                                    PortfolioId = id,
                                    ImagePath = "/Dashboard/PortfolioImages/" + fileName
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Portfolio updated successfully!";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error updating portfolio: " + ex.Message;
                }
            }

            TempData["FailureMsg"] = "Invalid data! Please check your inputs.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", portfolio.CategoryId);
            portfolio.PortfolioImages = (await _context.Portfolios.Include(p => p.PortfolioImages)
                .FirstOrDefaultAsync(p => p.PortfolioId == id))?.PortfolioImages ?? new List<PortfolioImages>();

            return View(portfolio);
        }


        // POST: Delete single portfolio image (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeletePortfolioImage(int imageId)
        {
            try
            {
                var image = await _context.PortfolioImages.FindAsync(imageId);

                if (image == null)
                    return Json(new { success = false, message = "Image not found!" });

                // Delete from folder
                if (!string.IsNullOrEmpty(image.ImagePath))
                {
                    string fullPath = Path.Combine(_hostEnvironment.WebRootPath,
                        image.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.PortfolioImages.Remove(image);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Image deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting image: " + ex.Message });
            }
        }

        // POST: Edit single portfolio image (AJAX)
        [HttpPost]
        public async Task<IActionResult> EditPortfolioImage(int imageId, IFormFile newImage)
        {
            try
            {
                var image = await _context.PortfolioImages.FindAsync(imageId);

                if (image == null)
                    return Json(new { success = false, message = "Image not found!" });

                if (newImage != null && newImage.Length > 0)
                {
                    // Delete old file
                    if (!string.IsNullOrEmpty(image.ImagePath))
                    {
                        string oldPath = Path.Combine(_hostEnvironment.WebRootPath,
                            image.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    string folder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "PortfolioImages");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8) + "_" + newImage.FileName;
                    string filePath = Path.Combine(folder, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                        await newImage.CopyToAsync(fs);

                    image.ImagePath = "/Dashboard/PortfolioImages/" + fileName;
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Image updated successfully!",
                        newImagePath = image.ImagePath
                    });
                }

                return Json(new { success = false, message = "No image provided!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating image: " + ex.Message });
            }
        }


        // GET: Portfolios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var portfolios = await _context.Portfolios
                .Include(p => p.Category)
                .Include(p => p.PortfolioImages)
                .FirstOrDefaultAsync(m => m.PortfolioId == id);
            if (portfolios == null)
            {
                return NotFound();
            }

            return View(portfolios);
        }

        // POST: Portfolios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.PortfolioImages)
                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (portfolio != null)
            {
                string webRoot = _hostEnvironment.WebRootPath;

                // --- Delete Main Portfolio Image ---
                if (!string.IsNullOrEmpty(portfolio.ImagePath))
                {
                    string mainImageFullPath = Path.Combine(webRoot, portfolio.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(mainImageFullPath))
                        System.IO.File.Delete(mainImageFullPath);
                }

                // --- Delete Additional Images ---
                foreach (var img in portfolio.PortfolioImages)
                {
                    if (!string.IsNullOrEmpty(img.ImagePath))
                    {
                        string imgFullPath = Path.Combine(webRoot, img.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(imgFullPath))
                            System.IO.File.Delete(imgFullPath);
                    }
                }

                _context.PortfolioImages.RemoveRange(portfolio.PortfolioImages);
                _context.Portfolios.Remove(portfolio);

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Portfolio deleted successfully!";
            }
            else
            {
                TempData["FaliureMsg"] = "Portfolio not found!";
            }

            return RedirectToAction(nameof(Index));
        }


        private bool PortfoliosExists(int id)
        {
            return _context.Portfolios.Any(e => e.PortfolioId == id);
        }
    }
}
