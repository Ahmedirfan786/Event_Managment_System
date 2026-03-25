using Event_Managment_System.Data;
using Microsoft.AspNetCore.Mvc;
using Event_Managment_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Managment_System.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BookingsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helper: Populate ViewBag with admin info from session
        private void SetAdminViewBag()
        {
            ViewBag.AdminId = HttpContext.Session.GetInt32("AdminId");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");
            ViewBag.AdminImage = HttpContext.Session.GetString("AdminImage");
        }


        // Pending Bookings View
        public IActionResult PendingBookings()
        {
            // Display Admin Sessions
            SetAdminViewBag();

            // want to fetch Name from Users Table with UserId in Bookings Model;

            var pendingBookings = _db.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == "Pending")
                .ToList();

            return View(pendingBookings);
        }

        // Rejected Bookings View
        public IActionResult RejectedBookings()
        {
            // Display Admin Sessions
            SetAdminViewBag();

            var rejectedBookings = _db.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == "Rejected")
                .ToList();

            return View(rejectedBookings);
        }

        // Approved Bookings View
        public IActionResult ApprovedBookings()
        {
            // Display Admin Sessions
            SetAdminViewBag();

            var approvedBookings = _db.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == "Approved")
                .ToList();

            return View(approvedBookings);
        }

        // Paid Bookings View
        public IActionResult PaidBookings()
        {
            // Display Admin Sessions
            SetAdminViewBag();

            var paidBookings = _db.Bookings
                .Include(b => b.User)
                .Where(b =>b.Status == "Paid")
                .ToList();

            return View(paidBookings);
        }

        // Completed Bookings View
        public IActionResult CompletedBookings()
        {
            // Display Admin Sessions
            SetAdminViewBag();

            var completedBookings = _db.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == "Completed")
                .ToList();

            return View(completedBookings);
        }


        // Booking Reject Code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectBK(int bookingId, string? reason)
        {
            var bk = _db.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (bk == null)
                return NotFound();

            bk.Status = "Rejected";
            bk.Remarks = reason;
            _db.SaveChanges();

            TempData["SuccessMsg"] = "Booking Rejected Successfully";
            return RedirectToAction("PendingBookings");
        }

        // Booking Accept Code
        [HttpGet]
        public IActionResult ApproveBK(int? id)
        {
            if (id == null)
                return NotFound();

            // Fetch the booking by ID
            var bk = _db.Bookings.FirstOrDefault(b => b.BookingId == id);

            if (bk == null)
                return NotFound();

            // Update the status
            bk.Status = "Approved";

            // Save changes
            _db.SaveChanges();

            TempData["SuccessMsg"] = "Booking Approved Successfully";
            return RedirectToAction("PendingBookings");
        }

        // Booking Accept Code
        [HttpGet]
        public IActionResult CompleteBK(int? id)
        {
            if (id == null)
                return NotFound();

            // Fetch the booking by ID
            var bk = _db.Bookings.FirstOrDefault(b => b.BookingId == id);

            if (bk == null)
                return NotFound();

            // Update the status
            bk.Status = "Completed";

            // Save changes
            _db.SaveChanges();

            TempData["SuccessMsg"] = "Booking Event Completed Successfully";
            return RedirectToAction("PaidBookings");
        } 
    }
}
