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
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BlogsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
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

        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            SetAdminViewBag();
            return View(await _context.Blogs.ToListAsync());
        }


        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var blogs = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blogs == null)
            {
                return NotFound();
            }

            return View(blogs);
        }

        // GET: Blogs/Create
        public IActionResult Create()
        {
            SetAdminViewBag();
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blogs blogs)
        {
            ModelState.Remove("ImagePath");
            if (ModelState.IsValid)
            {
                try
                {
                    // Set current date & time here
                    blogs.Date = DateTime.Now;

                    // ---- Image Upload Logic ----
                    if (blogs.Image != null)
                    {
                        var supportedTypes = new[] { ".jpg", ".jepg", ".png", ".jfif" };
                        var fileExt = Path.GetExtension(blogs.Image.FileName).ToLower();

                        if (!supportedTypes.Contains(fileExt))
                        {
                            TempData["FaliureMsg"] = "Only JPG, JPEG, PNG and JFIF images allowed";
                            return View(blogs);
                        }

                        if (blogs.Image.Length > 1048576)
                        {
                            TempData["FaliureMsg"] = "Image size must be less than 1MB!";
                            return View(blogs);
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "BlogImages");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 11) + "abcd";
                        string fileName = randomPrefix + "_" + Path.GetFileName(blogs.Image.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            blogs.Image.CopyTo(fileStream);
                        }

                        blogs.ImagePath = fileName;
                    }

                    _context.Add(blogs);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMsg"] = "Blog Created Successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["FaliureMsg"] = "Error creating Blog" + ex;
                    return View(blogs);
                }
            }

            TempData["FaliureMsg"] = "Invalid Data Submitted";
            return View(blogs);
        }

        // GET: Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var blogs = await _context.Blogs.FindAsync(id);
            if (blogs == null)
            {
                return NotFound();
            }
            return View(blogs);
        }

        // POST: Blogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Blogs blog)
        {
            if (id != blog.BlogId)
            {
                TempData["FailureMsg"] = "Blog not found!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = _context.Blogs.FirstOrDefault(x => x.BlogId == id);

                    if (existingBlog != null)
                    {
                        existingBlog.Title = blog.Title;
                        existingBlog.Description = blog.Description;

                        if (blog.Image != null)
                        {
                            var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
                            var fileExt = Path.GetExtension(blog.Image.FileName).ToLower();

                            if (!supportedTypes.Contains(fileExt))
                            {
                                TempData["FailureMsg"] = "Only JPG, JPEG, PNG, and JFIF images allowed!";
                                return View(blog);
                            }

                            if (blog.Image.Length > 1048576)
                            {
                                TempData["FailureMsg"] = "Image must be less than 1MB!";
                                return View(blog);
                            }

                            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "BlogImages");
                            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                            if (!string.IsNullOrEmpty(existingBlog.ImagePath))
                            {
                                string oldImagePath = Path.Combine(uploadsFolder, existingBlog.ImagePath);
                                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                            }

                            string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 10);
                            string fileName = randomPrefix + "_" + Path.GetFileName(blog.Image.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                blog.Image.CopyTo(stream);
                            }

                            existingBlog.ImagePath = fileName;
                        }

                        _context.SaveChanges();
                        TempData["SuccessMsg"] = "Blog updated successfully!";
                    }
                    else
                    {
                        TempData["FailureMsg"] = "Blog not found!";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error updating Blog: " + ex.Message;
                    return View(blog);
                }
            }

            TempData["FailureMsg"] = "Invalid data submitted!";
            return View(blog);
        }

        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            SetAdminViewBag();

            if (id == null)
            {
                return NotFound();
            }

            var blogs = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blogs == null)
            {
                return NotFound();
            }

            return View(blogs);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(blog.ImagePath))
                    {
                        string folderPath = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "BlogImages");
                        string filePath = Path.Combine(folderPath, blog.ImagePath);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }

                    _context.Blogs.Remove(blog);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMsg"] = "Blog deleted successfully!";
                }
                catch (Exception ex)
                {
                    TempData["FailureMsg"] = "Error deleting Blog: " + ex.Message;
                }
            }
            else
            {
                TempData["FailureMsg"] = "Blog not found!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlogsExists(int id)
        {
            return _context.Blogs.Any(e => e.BlogId == id);
        }
    }
}
