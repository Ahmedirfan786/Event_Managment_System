using Event_Managment_System.Data;
using Event_Managment_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Event_Managment_System.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DashboardController(ApplicationDbContext _db, IWebHostEnvironment hostEnvironment)
        {
            db = _db;
            _hostEnvironment = hostEnvironment;
        }


        // Login Page
        public IActionResult Login()
        {
            // if session Admin exists
            if (HttpContext.Session.GetString("AdminEmail") != null)
            {
                TempData["SuccessMsg"] = "Admin You Are Already Logged In !";
                return RedirectToAction("Index","Dashboard");
            }

            // If session User Exists (user is logged in restrict redirection to admin login page)
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                TempData["FaliureMsg"] = "Dear User You Cannot Access Admin Dashboard !";
                return RedirectToAction("Index","Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(Admin admin)
        {
            var Myadmin = db.Admins.FirstOrDefault(x => x.Email == admin.Email);

            if (Myadmin == null)
            {
                TempData["InvalidEmail"] = "Invalid Email";
                return RedirectToAction("Login", "Dashboard");
            }

            //Password verification
            var hasher = new PasswordHasher<Admin>();
            var result = hasher.VerifyHashedPassword(Myadmin, Myadmin.PasswordHash, admin.PasswordHash);
            if (result != PasswordVerificationResult.Success)
            {
                TempData["InvalidPassword"] = "Invalid Password";
                return RedirectToAction("Login", "Dashboard");
            }

            // Set session values
            HttpContext.Session.SetString("AdminEmail", Myadmin.Email);
            HttpContext.Session.SetInt32("AdminId", Myadmin.AdminId);
            HttpContext.Session.SetString("AdminName", Myadmin.Name);
            HttpContext.Session.SetString("AdminImage", Myadmin.ProfileImagePath ?? "default-user.png");

            TempData["SuccessMsg"] = "Admin Logged in Successfully";
            return RedirectToAction("Index", "Dashboard");
        }


        // Logout Logic
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMsg"] = "Admin Logged out Successfully";
            return RedirectToAction("Login");
        }

        // Helper: Populate ViewBag with admin info from session
        private void SetAdminViewBag()
        {
            ViewBag.AdminId = HttpContext.Session.GetInt32("AdminId");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");
            ViewBag.AdminImage = HttpContext.Session.GetString("AdminImage");
        }


        // Manage Profile
        public IActionResult ManageProfile()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");

            if (adminId != null)
            {
                var admin = db.Admins.FirstOrDefault(x => x.AdminId == adminId);
                if (admin != null)
                {
                    ViewBag.AdminId = admin.AdminId;
                    ViewBag.AdminName = admin.Name;
                    ViewBag.AdminEmail = admin.Email;
                    ViewBag.AdminImage = admin.ProfileImagePath;
                    return View(admin);
                }
            }

            TempData["FaliureMsg"] = "Admin Not Exists";
            return RedirectToAction("Login","Dashboard");
        }

        [HttpPost]
        public IActionResult ManageProfile(Admin admin)
        {
            // We remove PasswordHash and ProfileImage from validation because they can be null
            ModelState.Remove("PasswordHash");
            ModelState.Remove("ProfileImage");

            if (ModelState.IsValid)
            {
                var existingAdmin = db.Admins.FirstOrDefault(x => x.AdminId == admin.AdminId);
                if (existingAdmin != null)
                {
                    existingAdmin.Name = admin.Name;
                    existingAdmin.Email = admin.Email;

                    // --- Image Upload Logic ---
                    if (admin.ProfileImage != null)
                    {
                        // 1. Validate Extension
                        var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
                        var fileExt = Path.GetExtension(admin.ProfileImage.FileName).ToLower();

                        if (!supportedTypes.Contains(fileExt))
                        {
                            ModelState.AddModelError("ProfileImage", "Only JPG, JPEG, PNG, and JFIF are allowed.");
                            return View(admin);
                        }

                        // 2. Validate Size (1MB = 1048576 bytes)
                        if (admin.ProfileImage.Length > 1048576)
                        {
                            ModelState.AddModelError("ProfileImage", "Image size must be less than 1MB.");
                            return View(admin);
                        }

                        // 3. Generate Path: 15 random chars + _ + filename
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Dashboard", "AdminImages");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 11) + "abcd"; // Close to 15 chars
                        string fileName = randomPrefix + "_" + Path.GetFileName(admin.ProfileImage.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        // 4. Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingAdmin.ProfileImagePath))
                        {
                            string oldPath = Path.Combine(uploadsFolder, existingAdmin.ProfileImagePath);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        // 5. Save New File
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            admin.ProfileImage.CopyTo(fileStream);
                        }

                        existingAdmin.ProfileImagePath = fileName;
                    }

                    // --- Password Logic ---
                    if (!string.IsNullOrEmpty(admin.PasswordHash))
                    {
                        var passwordHasher = new PasswordHasher<Admin>();
                        existingAdmin.PasswordHash = passwordHasher.HashPassword(existingAdmin, admin.PasswordHash);
                    }

                    db.SaveChanges();
                    TempData["SuccessMsg"] = "Admin Profile Updated Successfully";
                }
                return RedirectToAction("Logout");
            }
            return View(admin);
        }


        // Main Dashboard Page
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("AdminEmail") != null)
            {
                ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");
                ViewBag.AdminId = HttpContext.Session.GetInt32("AdminId");
                ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminImage = HttpContext.Session.GetString("AdminImage");

                ViewBag.CategoriesCount = db.Categories.Count();
                ViewBag.PackagesCount = db.Packages.Count();
                ViewBag.UsersCount = db.Users.Count();
                ViewBag.MessagesCount = db.Messages.Count();
                ViewBag.PendingBookingsCount = db.Bookings.Where(b => b.Status == "Pending").Count();
                ViewBag.ApprovedBookingsCount = db.Bookings.Where(b => b.Status == "Approved").Count();
                ViewBag.RejectedBookingsCount = db.Bookings.Where(b => b.Status == "Rejected").Count();
                ViewBag.PaidBookingsCount = db.Bookings.Where(b => b.Status == "Paid").Count();
                ViewBag.CompletedBookingsCount = db.Bookings.Where(b => b.Status == "Completed").Count();
                ViewBag.BlogsCount = db.Blogs.Count();
                ViewBag.PortfoliosCount = db.Portfolios.Count();

                var latestBookings = db.Bookings
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BookingId)
                    .Take(10);


                return View(latestBookings);
            }

            // else
            return RedirectToAction("Login");
        }


        // View Website Users
        public IActionResult DisplayUsers()
        {
            SetAdminViewBag();
            var users = db.Users.ToList(); 
            return View(users);
        }

        // View Users Messages
        public IActionResult DisplayMessages()
        {
            SetAdminViewBag();
            var messages = db.Messages.ToList();
            return View(messages);
        }
    }
}
