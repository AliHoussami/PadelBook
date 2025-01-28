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
            var bookedCourts = await _context.Bookings.CountAsync(b =>
                b.CourtId == courtId &&
                b.BookingDate.Date == date.Date &&
                b.StartTime < endTime &&
                b.EndTime > startTime);

            if (bookedCourts >= 5)  // Assuming a total of 5 courts
            {
                ViewBag.Error = "No courts available for the selected date and time.";
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
            var bookedCourts = await _context.Bookings
                .Where(b => b.CourtId == courtId && b.BookingDate.Date == DateTime.Today)
                .CountAsync();
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
            var bookings = await _context.Bookings
                .Include(b => b.Court)  // Include the Court navigation property
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            return View(bookings);
        }
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            // Only allow cancellation if booking is in the future
            if (booking.BookingDate.Date < DateTime.Today ||
                (booking.BookingDate.Date == DateTime.Today && booking.StartTime <= DateTime.Now.TimeOfDay))
            {
                return BadRequest("Cannot cancel past bookings");
            }

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            // Only allow editing future bookings
            if (booking.BookingDate.Date < DateTime.Today ||
                (booking.BookingDate.Date == DateTime.Today && booking.StartTime <= DateTime.Now.TimeOfDay))
            {
                return BadRequest("Cannot edit past bookings");
            }

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DateTime newDate, TimeSpan newStartTime, TimeSpan newEndTime)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (newStartTime >= newEndTime)
            {
                ModelState.AddModelError("", "Start time must be before end time");
                return View(booking);
            }

            // Check availability for new time slot
            var conflictingBookings = await _context.Bookings
                .Where(b => b.Id != id &&
                            b.CourtId == booking.CourtId &&
                            b.BookingDate.Date == newDate.Date &&
                            b.StartTime < newEndTime &&
                            b.EndTime > newStartTime)
                .CountAsync();

            if (conflictingBookings >= 5)
            {
                ModelState.AddModelError("", "Selected time slot is not available");
                return View(booking);
            }

            booking.BookingDate = newDate;
            booking.StartTime = newStartTime;
            booking.EndTime = newEndTime;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }
    }
}