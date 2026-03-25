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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _hostEnvironment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
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




        // GET: Categories
        public async Task<IActionResult> Index()
        {
            SetAdminViewBag();
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            SetAdminViewBag();
            if (id == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            SetAdminViewBag();
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categories categories)
        {
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                try
                {
                    // --- Image Upload Logic ---
                    if (categories.Image != null)
                    {
                        var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
                        var fileExt = Path.GetExtension(categories.Image.FileName).ToLower();

                        if (!supportedTypes.Contains(fileExt))
                        {
                            TempData["FailureMsg"] = "Only JPG, JPEG, PNG, and JFIF images allowed!";
                            return View(categories);
                        }

                        if (categories.Image.Length > 1048576)
                        {
                            TempData["FailureMsg"] = "Image size must be less than 1MB!";
                            return View(categories);
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "CategoryImages");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 11) + "abcd";
                        string fileName = randomPrefix + "_" + Path.GetFileName(categories.Image.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            categories.Image.CopyTo(fileStream);
                        }

                        categories.ImagePath = fileName;
                    }

                    _context.Categories.Add(categories);
                    _context.SaveChanges();

                    TempData["SuccessMsg"] = "Category created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error creating category: " + ex.Message;
                    return View(categories);
                }
            }

            TempData["FailureMsg"] = "Invalid data submitted!";
            return View(categories);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            SetAdminViewBag(); 

            if (id == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.FindAsync(id);
            if (categories == null)
            {
                return NotFound();
            }
            return View(categories);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Categories categories)
        {
            if (id != categories.CategoryId)
            {
                TempData["FailureMsg"] = "Category not found!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = _context.Categories.FirstOrDefault(x => x.CategoryId == id);

                    if (existingCategory != null)
                    {
                        existingCategory.Name = categories.Name;

                        if (categories.Image != null)
                        {
                            var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
                            var fileExt = Path.GetExtension(categories.Image.FileName).ToLower();

                            if (!supportedTypes.Contains(fileExt))
                            {
                                TempData["FailureMsg"] = "Only JPG, JPEG, PNG, and JFIF images allowed!";
                                return View(categories);
                            }

                            if (categories.Image.Length > 1048576)
                            {
                                TempData["FailureMsg"] = "Image must be less than 1MB!";
                                return View(categories);
                            }

                            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "CategoryImages");
                            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                            if (!string.IsNullOrEmpty(existingCategory.ImagePath))
                            {
                                string oldImagePath = Path.Combine(uploadsFolder, existingCategory.ImagePath);
                                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                            }

                            string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 10);
                            string fileName = randomPrefix + "_" + Path.GetFileName(categories.Image.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                categories.Image.CopyTo(stream);
                            }

                            existingCategory.ImagePath = fileName;
                        }

                        _context.SaveChanges();
                        TempData["SuccessMsg"] = "Category updated successfully!";
                    }
                    else
                    {
                        TempData["FailureMsg"] = "Category not found!";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error updating category: " + ex.Message;
                    return View(categories);
                }
            }

            TempData["FailureMsg"] = "Invalid data submitted!";
            return View(categories);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(category.ImagePath))
                    {
                        string folderPath = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "CategoryImages");
                        string filePath = Path.Combine(folderPath, category.ImagePath);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }

                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMsg"] = "Category deleted successfully!";
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error deleting category: " + ex.Message;
                }
            }
            else
            {
                TempData["FailureMsg"] = "Category not found!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool CategoriesExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
