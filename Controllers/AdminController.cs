using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Padel_Court_Booking.Models;
using Padel_Court_Booking.Models.Data;

namespace Padel_Court_Booking.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public IActionResult Dashboard()
        {
            ViewBag.TotalCourts = _context.Courts.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalBookings = _context.Bookings.Count();

            var recentBookings = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToList();

            ViewBag.RecentBookings = recentBookings;

            return View();
        }

        // Manage Courts
        public IActionResult ManageCourts()
        {
            var courts = _context.Courts.ToList();
            return View(courts);
        }

        [HttpPost]
        public IActionResult AddCourt(Court court)
        {
            if (ModelState.IsValid)
            {
                _context.Courts.Add(court);
                _context.SaveChanges();
                return RedirectToAction("ManageCourts");
            }
            return View("ManageCourts", _context.Courts.ToList());
        }

        [HttpPost]
        public IActionResult DeleteCourt(int courtId)
        {
            var court = _context.Courts.Find(courtId);
            if (court != null)
            {
                _context.Courts.Remove(court);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageCourts");
        }

        // Manage Users
        public IActionResult ManageUsers()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }

        // Manage Bookings
        public IActionResult ManageBookings()
        {
            var bookings = _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .ToList();
            return View(bookings);
        }

        [HttpPost]
        public IActionResult DeleteBooking(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageBookings");
        }
    }
}
