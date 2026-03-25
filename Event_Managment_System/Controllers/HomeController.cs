using Event_Managment_System.Data;
using Event_Managment_System.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Linq;

namespace Event_Managment_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly StripeSettings _stripeSettings;
        public HomeController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IOptions<StripeSettings> stripeSettings)
        {
            _db = db;
            _hostEnvironment = webHostEnvironment;
            _stripeSettings = stripeSettings.Value;
        }

        public IActionResult Index()
        {
            var categoriesObj = _db.Categories.ToList();
            var blogsObj = _db.Blogs
                .OrderByDescending(b => b.Date)
                .Take(3)
                .ToList();

            var model = Tuple.Create(categoriesObj, blogsObj);
            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            var categoriesObj = _db.Categories.ToList();
            return View(categoriesObj);
        }

        public IActionResult Blogs(int page = 1)
        {
            int pageSize = 6;
            var totalBlogs = _db.Blogs.Count();

            var blogsObj = _db.Blogs
                .OrderByDescending(b => b.Date)
                .Skip((page -1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

            return View(blogsObj);
        }

        public IActionResult BlogDetail(int? id)
        {
            if (id == null)
            {
                TempData["FaliureMsg"] = "Blog with this Id doesn't exist!";
                return RedirectToAction("Blogs", "Home");
            }

            var blogdata = _db.Blogs.FirstOrDefault(b => b.BlogId == id);

            if (blogdata == null)
            {
                TempData["FaliureMsg"] = "Blog not found!";
                return RedirectToAction("Blogs", "Home");
            }

            return View(blogdata);
        }

        public IActionResult Packages(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = _db.Categories.FirstOrDefault(c => c.CategoryId == id);

            var packagesObj = _db.Packages
                .Include(p => p.Category)
                .Include(p => p.PackageFeatures)
                .Where(p => p.CategoryId == id)
                .ToList();

            ViewBag.CategoryName = category?.Name;

            return View(packagesObj);
        }

        public IActionResult Portfolios(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = _db.Categories.FirstOrDefault(c => c.CategoryId == id);

            var portfoliosObj = _db.Portfolios
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .ToList();

            ViewBag.CategoryName = category?.Name;

            return View(portfoliosObj);
        }

        public IActionResult PortfolioDetail(int? id)
        {
            if (id == null)
                return NotFound();

            // Include Category
            var portfolio = _db.Portfolios
                .Include(p => p.Category)
                .FirstOrDefault(p => p.PortfolioId == id);

            if (portfolio == null)
                return NotFound();

            var portfolioImagesObj = _db.PortfolioImages
                .Where(p => p.PortfolioId == id)
                .ToList();

            ViewBag.CategoryName = portfolio.Category?.Name;
            ViewBag.PortfolioName = portfolio.Name;
            ViewBag.PortfolioDescription = portfolio.Description;
            ViewBag.PortfolioBudget = portfolio.Budget;
            ViewBag.PortfolioLocation = portfolio.Location;

            return View(portfolioImagesObj);
        }
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(Messages msg)
        {
            if (ModelState.IsValid)
            {
                _db.Messages.Add(msg);
                _db.SaveChanges();
                TempData["SuccessMsg"] = "Message Send Succesfully !";
            }
             
            return View("Contact");
            
        }

        public IActionResult Booking(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var package = _db.Packages
                .Include(p => p.Category)
                .FirstOrDefault(p => p.PackageId == id);

            if (package == null)
            {
                return NotFound();
            }

            // Fetch package features
            var features = _db.PackageFeatures
                .Where(f => f.PackageId == id)
                .Select(f => f.Name)
                .ToList();

            string featureList = string.Join(", ", features);

            Bookings booking = new Bookings()
            {
                PackageName = package.Name,
                PackagePrice = package.price,
                PackageCategory = package.Category.Name,
                PackageFeatures = featureList,
                Status = "Pending"
            };

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Booking(Bookings booking)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                booking.UserId = userId.Value;
                booking.Status = "Pending";

                _db.Bookings.Add(booking);
                _db.SaveChanges();

                TempData["SuccessMsg"] = "Booking Created Successfully !";
                return RedirectToAction("Index");
            }

            return View(booking);
        }

        public IActionResult TrackBookings(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get session values
            var userId = HttpContext.Session.GetInt32("UserId");
            var adminId = HttpContext.Session.GetInt32("AdminId");


            if (adminId != null)
            {
                return NotFound();
            }

            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (userId != id)
            {
                return RedirectToAction("TrackBookings", "Home", new { id = userId });
            }

            var userBookings = _db.Bookings
                .Where(b => b.UserId == id)
                .ToList();

            return View(userBookings);
        }

        public IActionResult Register()
        {
            // If Admin User Exists (Admin is logged in restrict redirection to User login or register page)
            if (HttpContext.Session.GetString("AdminEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Users user)
        {
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                // --- Image Upload Logic ---
                if (user.Image != null)
                {
                    // 1. Validate Extension
                    var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
                    var fileExt = Path.GetExtension(user.Image.FileName).ToLower();

                    if (!supportedTypes.Contains(fileExt))
                    {
                        ModelState.AddModelError("Image", "Only JPG, JPEG, PNG, and JFIF are allowed.");
                        return View(user);
                    }

                    // 2. Validate Size (1MB)
                    if (user.Image.Length > 1048576)
                    {
                        ModelState.AddModelError("Image", "Image size must be less than 1MB.");
                        return View(user);
                    }

                    // 3. Folder Path
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Website", "UserImages");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // 4. Unique File Name
                    string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 11) + "user";
                    string fileName = randomPrefix + "_" + Path.GetFileName(user.Image.FileName);

                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // 5. Save Image
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        user.Image.CopyTo(fileStream);
                    }

                    // 6. Save Image Name
                    user.ImagePath = fileName;
                }

                // --- Password Hash Logic (Same as Admin) ---
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    var passwordHasher = new PasswordHasher<Users>();
                    user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);
                }

                _db.Users.Add(user);
                _db.SaveChanges();
                TempData["SuccessMsg"] = "User Registered Successfully!";

                return RedirectToAction("Login");
            }

            return View(user);
        }

        public IActionResult Login()
        {
            // If Admin User Exists (Admin is logged in restrict redirection to User login or register page)
            if (HttpContext.Session.GetString("AdminEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Users user)
        {
            ModelState.Clear();

            var MyUser = _db.Users.FirstOrDefault(u => u.Email == user.Email);

            if (MyUser == null)
            {
                TempData["InvalidEmail"] = "Invalid Email";
                return RedirectToAction("Login", "Home");
            }

            var hasher = new PasswordHasher<Users>();
            var result = hasher.VerifyHashedPassword(MyUser, MyUser.PasswordHash, user.PasswordHash);

            if (result != PasswordVerificationResult.Success)
            {
                TempData["InvalidPassword"] = "Invalid Password";
                return RedirectToAction("Login", "Home");
            }

            HttpContext.Session.SetString("UserEmail", MyUser.Email);
            HttpContext.Session.SetInt32("UserId", MyUser.UserId);
            HttpContext.Session.SetString("UserName", MyUser.Name);
            HttpContext.Session.SetString("UserImage", MyUser.ImagePath ?? "default-user.png");

            TempData["SuccessMsg"] = "Logged In Successfully !";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ManageProfile(int? id)
        {
            var userId = id;

            if (userId != null)
            {
                var user = _db.Users.FirstOrDefault(x => x.UserId == userId);
                if (user != null)
                {
                    ViewBag.UserId = user.UserId;
                    ViewBag.UserName = user.Name;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserImage = user.ImagePath;
                    return View(user);
                }
            }
            return RedirectToAction("Login", "Dashboard");
        }

        [HttpPost]
        public IActionResult ManageProfile(Users user)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                var existingUser = _db.Users.FirstOrDefault(x => x.UserId == user.UserId);

                if (existingUser != null)
                {
                    existingUser.Name = user.Name;
                    existingUser.Email = user.Email;

                    // ----- Image Upload -----
                    if (user.Image != null)
                    {
                        var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
                        var fileExt = Path.GetExtension(user.Image.FileName).ToLower();

                        if (!supportedTypes.Contains(fileExt))
                        {
                            ModelState.AddModelError("Image", "Only JPG, JPEG, PNG, and JFIF are allowed.");
                            return View(user);
                        }

                        if (user.Image.Length > 1048576)
                        {
                            ModelState.AddModelError("Image", "Image size must be less than 1MB.");
                            return View(user);
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Website", "UserImages");

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string randomPrefix = Path.GetRandomFileName().Replace(".", "").Substring(0, 11) + "abcd";
                        string fileName = randomPrefix + "_" + Path.GetFileName(user.Image.FileName);

                        string filePath = Path.Combine(uploadsFolder, fileName);

                        // Delete old image
                        if (!string.IsNullOrEmpty(existingUser.ImagePath))
                        {
                            string oldPath = Path.Combine(uploadsFolder, existingUser.ImagePath);

                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            user.Image.CopyTo(stream);
                        }

                        existingUser.ImagePath = fileName;
                    }

                    // ----- Password Change -----
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        var passwordHasher = new PasswordHasher<Users>();
                        existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, user.PasswordHash);
                    }

                    _db.SaveChanges();
                    TempData["SuccessMsg"] = "User Profile Updated Successfully !";
                }

                return RedirectToAction("Logout");
            }

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMsg"] = "User Logged out Successfully !";
            return RedirectToAction("Index");
        }


        // Stripe Payment Code
        public IActionResult CreateCheckoutSession(int BookingId, int amount)
        {
            var currency = "usd";
            var successUrl = $"https://localhost:7095/Home/PaymentSuccess?BookingId={BookingId}&session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"https://localhost:7095/Home/PaymentCancel?BookingId={BookingId}";

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },

                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = Convert.ToInt32(amount) * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Booking",
                                Description = "Demo Description"
                            }

                        },

                        Quantity = 1
                    }
                },

                Mode = "payment",
                SuccessUrl = successUrl, 
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        // On Stripe Payment Success
        public IActionResult PaymentSuccess(int BookingId, string session_id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var service = new SessionService();
            var session = service.Get(session_id);

            if (session != null && session.PaymentStatus == "paid")
            {
                var bk = _db.Bookings.FirstOrDefault(b => b.BookingId == BookingId);
                if (bk != null)
                {
                    bk.Status = "Paid";
                    _db.SaveChanges();
                }
                TempData["BookingStatus"] = $"Payment successful for Booking Id: {BookingId}";
            }
            else
            {
                TempData["BookingStatus"] = $"Payment not confirmed for Booking Id: {BookingId}";
            }

            return RedirectToAction("TrackBookings", new { id = userId });
        }

        // On Stripe Payment Cancel
        public IActionResult PaymentCancel(int BookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            TempData["BookingStatus"] = $"Payment failed for Booking Id: {BookingId}. Please try again!";
            return RedirectToAction("TrackBookings", new { id = userId });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
