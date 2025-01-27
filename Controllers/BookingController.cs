using Microsoft.AspNetCore.Mvc;
using Padel_Court_Booking.Models.Data;
using Padel_Court_Booking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


namespace Padel_Court_Booking.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courts = await _context.Courts.ToListAsync();
            return View(courts);
        }

        [HttpPost]
        public async Task<IActionResult> Book(int courtId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
            {
                ViewBag.Error = "Start time must be before end time.";
                return RedirectToAction("Book", new { courtId });
            }

            var court = await _context.Courts.FirstOrDefaultAsync(c => c.Id == courtId);
            if (court == null)
            {
                ViewBag.Error = "Invalid court selected.";
                return RedirectToAction("Index");
            }

            // Check availability
            var bookedCourts = await _context.Bookings.CountAsync(b => b.CourtId == courtId && b.BookingDate == date);
            if (bookedCourts >= 5)  // Assuming a total of 5 courts
            {
                ViewBag.Error = "No courts available for the selected date.";
                return RedirectToAction("Book", new { courtId });
            }

            var booking = new Booking
            {
                UserId = int.Parse(HttpContext.Session.GetString("UserId")),
                CourtId = courtId,
                BookingDate = date,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Confirmed"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyBookings");
        }
        [HttpGet]
        public async Task<IActionResult> Book(int courtId)
        {
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.Id == courtId);
            if (court == null)
            {
                return NotFound();
            }

            // Simulated available courts logic (can be updated with real business logic)
            var totalCourts = 5;  // Example: Assume 5 courts available
            var bookedCourts = await _context.Bookings.CountAsync(b => b.CourtId == courtId);
            int availableCourts = totalCourts - bookedCourts;

            var model = new Booking
            {
                CourtId = court.Id,
                Court = court,
                BookingDate = DateTime.Today
            };

            ViewBag.AvailableCourts = availableCourts;
            return View(model);
        }
        public async Task<IActionResult> MyBookings()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var bookings = await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
            return View(bookings);
        }
    }

}
