using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using QLRapPhim.Models;
using QLRapPhim.VNPAY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QLRapPhim.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingController(AppDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Booking/SelectShowtime?movieId={id}
        [HttpGet]
        public async Task<IActionResult> SelectShowtime(int movieId)
        {
            var userId = User?.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var movie = await _context.Phims
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == movieId);

            if (movie == null)
            {
                return NotFound();
            }

            var showtimes = await _context.SuatChieus
                .Include(s => s.Screen)
                .Include(s => s.Movie)
                .Where(s => s.MovieId == movieId && s.ShowDate >= DateTime.Today)
                .OrderBy(s => s.ShowDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            if (!showtimes.Any())
            {
                TempData["Error"] = "Hiện tại không có suất chiếu nào cho phim này.";
                return RedirectToAction("Detail", "Home", new { id = movieId });
            }

            ViewBag.Movie = movie;
            return View(showtimes);
        }

        // GET: Booking/SelectSeat?showtimeId={id}
        [HttpGet]
        public async Task<IActionResult> SelectSeat(int showtimeId)
        {
            var showtime = await _context.SuatChieus
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                .ThenInclude(p => p.Seats)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                return NotFound();
            }

            var bookedSeats = await _context.Bookings
                .Where(b => b.ShowtimeId == showtimeId && b.IsConfirmed == true)
                .Select(b => b.SeatId)
                .ToListAsync();

            var seats = showtime.Screen.Seats.Select(s => new SeatViewModel
            {
                Id = s.Id,
                Position = s.Position,
                IsOccupied = bookedSeats.Contains(s.Id)
            }).ToList();

            ViewBag.Showtime = showtime;
            return View(seats);
        }

        // POST: Booking/SelectTicketAndFood
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectTicketAndFood(int showtimeId, string seatIds)
        {
            var showtime = await _context.SuatChieus
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);

            if (showtime == null)
            {
                return NotFound();
            }

            var seatIdList = seatIds.Split(',').Select(int.Parse).ToList();
            var seats = await _context.GheNgois
                .Where(g => seatIdList.Contains(g.Id) && g.ScreenId == showtime.ScreenId)
                .ToListAsync();

            if (seats == null || !seats.Any())
            {
                return NotFound();
            }

            var isAnyBooked = await _context.Bookings
                .AnyAsync(b => b.ShowtimeId == showtimeId && seatIdList.Contains(b.SeatId) && b.IsConfirmed == true);

            if (isAnyBooked)
            {
                TempData["Error"] = "Một hoặc nhiều ghế đã được đặt. Vui lòng chọn lại.";
                return RedirectToAction(nameof(SelectSeat), new { showtimeId });
            }

            var ticketPrices = await _context.GiaVes.ToListAsync();
            var foods = await _context.DoAns.ToListAsync();

            ViewBag.Showtime = showtime;
            ViewBag.Seats = seats;
            ViewBag.TicketPrices = ticketPrices;
            ViewBag.Foods = foods;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int showtimeId, string seatIds, int ticketPriceId, int? foodId, int? foodQuantity)
        {
            var showtime = await _context.SuatChieus.FindAsync(showtimeId);
            var ticketPrice = await _context.GiaVes.FindAsync(ticketPriceId);
            var food = foodId.HasValue ? await _context.DoAns.FindAsync(foodId.Value) : null;

            if (showtime == null || ticketPrice == null)
            {
                return BadRequest("Suất chiếu hoặc giá vé không hợp lệ.");
            }

            var seatIdList = seatIds.Split(',').Select(int.Parse).ToList();
            var seats = await _context.GheNgois
                .Where(g => seatIdList.Contains(g.Id) && g.ScreenId == showtime.ScreenId)
                .ToListAsync();

            if (seats == null || seats.Count != seatIdList.Count)
            {
                return BadRequest("Một hoặc nhiều ghế không hợp lệ hoặc không thuộc phòng chiếu này.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập để tiếp tục.");
            }

            var isAnyBooked = await _context.Bookings
                .AnyAsync(b => b.ShowtimeId == showtimeId && seatIdList.Contains(b.SeatId) && b.IsConfirmed == true);

            if (isAnyBooked)
            {
                TempData["Error"] = "Một hoặc nhiều ghế đã được đặt. Vui lòng chọn lại.";
                return RedirectToAction(nameof(SelectSeat), new { showtimeId });
            }

            // Tính tổng giá đồ ăn một lần duy nhất
            decimal totalFoodPrice = 0;
            if (food != null && foodQuantity.HasValue && foodQuantity.Value > 0)
            {
                totalFoodPrice = food.Price * foodQuantity.Value;
            }

            List<Booking> bookings = new List<Booking>();
            foreach (var seat in seats)
            {
                var booking = new Booking
                {
                    UserId = userId,
                    ShowtimeId = showtimeId,
                    SeatId = seat.Id,
                    FoodId = foodId,
                    BookingDate = DateTime.Now,
                    IsConfirmed = false,
                    IsFailed = false
                };

                _context.Bookings.Add(booking);

                var bookingDetails = new BookingDetails
                {
                    TicketPriceId = ticketPriceId,
                    Quantity = 1, // Số lượng vé là 1 cho mỗi ghế
                    TotalPrice = ticketPrice.Price // Chỉ tính giá vé cho mỗi ghế
                };
                booking.BookingDetails = new List<BookingDetails> { bookingDetails };

                bookings.Add(booking);
            }

            await _context.SaveChangesAsync();

            if (bookings.Count != seatIdList.Count)
            {
                TempData["Error"] = "Lỗi đồng bộ: Số lượng vé được tạo không khớp với số ghế đã chọn.";
                return RedirectToAction(nameof(SelectSeat), new { showtimeId });
            }

            var bookingIds = bookings.Select(b => b.Id).ToList();
            TempData["BookingIds"] = JsonConvert.SerializeObject(bookingIds);
            TempData["TotalFoodPrice"] = totalFoodPrice.ToString();
            TempData["FoodQuantity"] = foodQuantity?.ToString() ?? "0"; // Lưu số lượng đồ ăn
            _httpContextAccessor.HttpContext.Session.SetString("BookingIds", JsonConvert.SerializeObject(bookingIds));
            _httpContextAccessor.HttpContext.Session.SetString("TotalFoodPrice", totalFoodPrice.ToString());
            _httpContextAccessor.HttpContext.Session.SetString("FoodQuantity", foodQuantity?.ToString() ?? "0");

            return RedirectToAction(nameof(Payment), new { bookingId = bookings.First().Id });
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId)
        {
            var bookingIdsJson = _httpContextAccessor.HttpContext.Session.GetString("BookingIds") ?? TempData["BookingIds"] as string;
            if (string.IsNullOrEmpty(bookingIdsJson))
            {
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            var bookingIds = JsonConvert.DeserializeObject<List<int>>(bookingIdsJson);
            var bookings = await _context.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Screen)
                .Include(b => b.User)
                .Include(b => b.Food)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TicketPrice)
                .Include(b => b.Showtime.Screen.Seats)
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            // Tính tổng giá vé
            decimal totalTicketPrice = bookings.Sum(b => b.BookingDetails.First().TotalPrice);

            // Lấy giá đồ ăn tổng từ Session hoặc TempData
            var totalFoodPrice = decimal.Parse(_httpContextAccessor.HttpContext.Session.GetString("TotalFoodPrice") ?? TempData["TotalFoodPrice"]?.ToString() ?? "0");

            // Tổng giá cuối cùng
            decimal totalPrice = totalTicketPrice + totalFoodPrice;

            var seatPositions = bookings.Select(b => b.Showtime.Screen.Seats
                .FirstOrDefault(s => s.Id == b.SeatId)?.Position ?? "Không xác định").ToList();

            ViewBag.BookingIds = bookingIds;
            ViewBag.Bookings = bookings;
            ViewBag.SeatPositions = string.Join(", ", seatPositions);
            ViewBag.TotalPrice = totalPrice;
            ViewBag.Quantity = bookings.Count;
            ViewBag.TotalTicketPrice = totalTicketPrice; // Hiển thị riêng giá vé
            ViewBag.TotalFoodPrice = totalFoodPrice;    // Hiển thị riêng giá đồ ăn
            ViewBag.IsConfirmed = bookings.All(b => b.IsConfirmed);

            return View(bookings.First());
        }

        [HttpPost]
        public IActionResult CreatePaymentUrl(int bookingId, string bankCode)
        {
            var bookingIdsJson = _httpContextAccessor.HttpContext.Session.GetString("BookingIds")
                                  ?? TempData["BookingIds"] as string;

            if (string.IsNullOrEmpty(bookingIdsJson))
            {
                TempData["Error"] = "Không tìm thấy đơn đặt vé.";
                return RedirectToAction("Index", "Home");
            }

            var bookingIds = JsonConvert.DeserializeObject<List<int>>(bookingIdsJson);
            var bookings = _context.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => bookingIds.Contains(b.Id))
                .ToList();

            if (bookings == null || !bookings.Any())
            {
                TempData["Error"] = "Không tìm thấy đơn đặt vé.";
                return RedirectToAction("Index", "Home");
            }

            var total = bookings.Sum(b => b.BookingDetails.FirstOrDefault()?.TotalPrice ?? 0);
            var amount = ((long)(total * 100)).ToString(); // Nhân 100 theo yêu cầu VNPay

            var config = _configuration.GetSection("Vnpay").Get<VnpayConfig>();

            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", config.TmnCode);
            vnpay.AddRequestData("vnp_Amount", amount);
            vnpay.AddRequestData("vnp_BankCode", bankCode); // có thể rỗng
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán vé xem phim - Mã: {string.Join("-", bookingIds)}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", config.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", string.Join("-", bookingIds));

            var paymentUrl = vnpay.CreateRequestUrl(config.PaymentUrl, config.HashSecret);

            return Redirect(paymentUrl);
        }



        [HttpGet]
        public async Task<IActionResult> VnpayReturn()
        {
            if (Request.Query.Count > 0)
            {
                var vnpay = new VnPayLibrary();
                var config = _configuration.GetSection("Vnpay").Get<VnpayConfig>();

                foreach (var (key, value) in Request.Query)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, value);
                    }
                }

                var vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash");
                var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                var vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");

                var isValid = vnpay.ValidateSignature(vnp_SecureHash, config.HashSecret);

                if (!isValid)
                {
                    TempData["Error"] = "Lỗi xác minh chữ ký (VNPay).";
                    return RedirectToAction("PaymentFailed", new { bookingId = vnp_TxnRef });
                }

                var bookingIds = vnp_TxnRef.Split('-').Select(int.Parse).ToList();

                var bookings = await _context.Bookings
                    .Where(b => bookingIds.Contains(b.Id))
                    .ToListAsync();

                if (vnp_ResponseCode == "00")
                {
                    foreach (var booking in bookings)
                        booking.IsConfirmed = true;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thanh toán thành công.";
                    return RedirectToAction("PaymentComplete", new { bookingId = bookingIds.First() });
                }
                else
                {
                    foreach (var booking in bookings)
                        booking.IsFailed = true;

                    await _context.SaveChangesAsync();

                    TempData["Error"] = $"Thanh toán thất bại. Mã lỗi: {vnp_ResponseCode}";
                    return RedirectToAction("PaymentFailed", new { bookingId = bookingIds.First() });
                }
            }

            return RedirectToAction("Index", "Home");
        }



        // GET: Booking/PaymentFailed?bookingId={id}
        [HttpGet]
        public async Task<IActionResult> PaymentFailed(int bookingId)
        {
            var bookingIdsJson = TempData["BookingIds"] as string ?? _httpContextAccessor.HttpContext.Session.GetString("BookingIds");
            if (string.IsNullOrEmpty(bookingIdsJson))
            {
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            var bookingIds = JsonConvert.DeserializeObject<List<int>>(bookingIdsJson);
            var bookings = await _context.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Screen)
                .Include(b => b.User)
                .Include(b => b.Food)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TicketPrice)
                .Include(b => b.Showtime.Screen.Seats)
                .Where(b => bookingIds.Contains(b.Id)) // Sửa từ b.Id == bookingIds.Contains(b.Id)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            var seatPositions = bookings.Select(b => b.Showtime.Screen.Seats
                .FirstOrDefault(s => s.Id == b.SeatId)?.Position ?? "Không xác định").ToList();
            ViewBag.SeatPositions = string.Join(", ", seatPositions);
            ViewBag.Error = TempData["Error"] as string;

            return View(bookings.First());
        }

        [HttpGet]
        public async Task<IActionResult> PaymentComplete(int bookingId)
        {
            var bookingIdsJson = _httpContextAccessor.HttpContext.Session.GetString("BookingIds") ?? TempData["BookingIds"] as string;
            if (string.IsNullOrEmpty(bookingIdsJson))
            {
                System.Diagnostics.Debug.WriteLine($"PaymentComplete Error at {DateTime.Now}: bookingIdsJson is null or empty.");
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            var bookingIds = JsonConvert.DeserializeObject<List<int>>(bookingIdsJson);
            var bookings = await _context.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Screen)
                .Include(b => b.User)
                .Include(b => b.Food)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TicketPrice)
                .Include(b => b.Showtime.Screen.Seats)
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                System.Diagnostics.Debug.WriteLine($"PaymentComplete Error at {DateTime.Now}: No bookings found for bookingIds: {string.Join(", ", bookingIds)}");
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;
            var seatPositions = bookings.Select(b => b.Showtime.Screen.Seats
                .FirstOrDefault(s => s.Id == b.SeatId)?.Position ?? "Không xác định").ToList();
            ViewBag.SeatPositions = string.Join(", ", seatPositions);
            ViewBag.Bookings = bookings; // Gán rõ ràng
            return View(bookings.First());
        }
        public async Task SendPaymentConfirmationEmailAsync(List<Booking> bookings)
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == bookings.First().UserId);

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("CinemaApp", emailSettings.SenderEmail));
            email.To.Add(new MailboxAddress(user.FullName, user.Email));
            email.Subject = "Xác nhận đặt vé thành công";

            var seatDetails = new StringBuilder();
            decimal totalPrice = 0;
            foreach (var booking in bookings)
            {
                var showTime = booking.Showtime.ShowDate + booking.Showtime.StartTime;
                var seat = await _context.GheNgois.FindAsync(booking.SeatId);
                seatDetails.Append($@"
            <li>
                <strong>Ghế:</strong> {seat?.Position ?? "Không xác định"}<br/>
                <strong>Loại vé:</strong> {booking.BookingDetails.First().TicketPrice.Name}<br/>
                {(booking.Food != null ? $"<strong>Đồ ăn:</strong> {booking.Food.Name}<br/>" : "")}
                <strong>Giá:</strong> {booking.BookingDetails.First().TotalPrice:C}<br/>
            </li>");
                totalPrice += booking.BookingDetails.First().TotalPrice;
            }

            var body = new TextPart("html")
            {
                Text = $@"
            <h2>Xác nhận đặt vé thành công</h2>
            <p>Chào {user.FullName},</p>
            <p>Đơn đặt vé của bạn đã được thanh toán thành công tại {DateTime.Now:dd/MM/yyyy HH:mm:ss}. Dưới đây là thông tin chi tiết:</p>
            <ul>
                <li><strong>Phim:</strong> {bookings.First().Showtime.Movie.Name}</li>
                <li><strong>Ngày chiếu:</strong> {bookings.First().Showtime.ShowDate:dd/MM/yyyy}</li>
                <li><strong>Giờ chiếu:</strong> {(bookings.First().Showtime.ShowDate + bookings.First().Showtime.StartTime):hh\\:mm}</li>
                <li><strong>Phòng:</strong> {bookings.First().Showtime.Screen.Name}</li>
                <li><strong>Chi tiết đặt vé:</strong>
                    <ul>{seatDetails.ToString()}</ul>
                </li>
                <li><strong>Tổng tiền:</strong> {totalPrice:C}</li>
            </ul>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
            <p>Trân trọng,<br>CinemaApp Team</p>"
            };

            email.Body = body;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(emailSettings.SmtpServer, emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings.SenderEmail, emailSettings.SenderPassword);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> BookingHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập để xem lịch sử đặt vé.");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Screen)
                .Include(b => b.Food)
                .Include(b => b.User)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TicketPrice)
                .Include(b => b.Showtime.Screen.Seats)
                .Where(b => b.UserId == userId && b.IsConfirmed == true)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            if (!bookings.Any())
            {
                ViewBag.Message = "Bạn chưa có lịch sử đặt vé nào.";
                return View(new List<BookingHistoryViewModel>());
            }

            var grouped = bookings
                .GroupBy(b => b.Id)
                .Select(g =>
                {
                    var first = g.First();
                    var seat = first.Showtime.Screen.Seats.FirstOrDefault(s => s.Id == first.SeatId);
                    var seatPos = seat?.Position ?? "Không xác định";
                    var foodPrice = first.Food != null ? first.Food.Price : 0;

                    return new BookingHistoryViewModel
                    {
                        BookingId = first.Id,
                        Showtime = first.Showtime,
                        SeatPositions = seatPos,
                        Quantity = 1, // Vì mỗi booking là 1 ghế
                        Food = first.Food,
                        BookingDetails = first.BookingDetails,
                        TotalTicketPrice = first.BookingDetails.FirstOrDefault()?.TotalPrice ?? 0,
                        TotalFoodPrice = foodPrice,
                        IsConfirmed = first.IsConfirmed,
                        IsFailed = first.IsFailed
                    };
                })
                .ToList();

            return View(grouped);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> BookingHistoryDetail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập để xem chi tiết.");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Screen)
                .Include(b => b.Food)
                .Include(b => b.User)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.TicketPrice)
                .Include(b => b.Showtime.Screen.Seats)
                .Where(b => b.UserId == userId && b.IsConfirmed == true && b.Id == id)
                .ToListAsync();

            if (!bookings.Any())
            {
                return NotFound("Đơn đặt vé không tồn tại.");
            }

            var seatPositions = bookings.Select(b =>
                b.Showtime.Screen.Seats.FirstOrDefault(s => s.Id == b.SeatId)?.Position ?? "Không xác định"
            ).ToList();

            ViewBag.SeatPositions = string.Join(", ", seatPositions);
            ViewBag.Bookings = bookings;
            ViewBag.SuccessMessage = "Đơn đặt vé của bạn đã được xác nhận.";
            ViewBag.TotalTicketPrice = bookings.Sum(b => b.BookingDetails.FirstOrDefault()?.TotalPrice ?? 0);
            ViewBag.TotalFoodPrice = bookings.FirstOrDefault()?.Food?.Price ?? 0;
            ViewBag.TotalPrice = (decimal)ViewBag.TotalTicketPrice + (decimal)ViewBag.TotalFoodPrice;
            ViewBag.Quantity = bookings.Count;

            return View(bookings.First());
        }


        // Model cấu hình VNPay
        public class VnpayConfig
        {
            public string TmnCode { get; set; }
            public string HashSecret { get; set; }
            public string PaymentUrl { get; set; }
            public string ReturnUrl { get; set; }
        }

        // Model cấu hình Email
        public class EmailSettings
        {
            public string SmtpServer { get; set; }
            public int Port { get; set; }
            public string SenderEmail { get; set; }
            public string SenderPassword { get; set; }
        }
    }
}