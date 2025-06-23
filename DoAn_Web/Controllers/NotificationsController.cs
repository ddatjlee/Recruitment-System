using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn_Web.Models;
using System.Threading.Tasks;

namespace DoAn_Web.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly RecruitmentSystemContext _context;

        public NotificationsController(RecruitmentSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                TempData["NotificationError"] = "Thông báo không tồn tại.";
                return RedirectToAction("Index");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            TempData["NotificationSuccess"] = "Thông báo đã được đánh dấu là đã đọc.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                TempData["NotificationSuccess"] = "Thông báo đã được xóa.";
            }
            else
            {
                TempData["NotificationError"] = "Không tìm thấy thông báo cần xóa.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("StudentId") ??
                         HttpContext.Session.GetInt32("CompanyId") ??
                         HttpContext.Session.GetInt32("AdminId");

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            string userType = "student";
            if (HttpContext.Session.GetInt32("CompanyId") != null)
            {
                userType = "company";
            }
            else if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                userType = "admin";
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId.Value && n.UserType == userType)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UnreadNotifications = notifications.Count(n => !n.IsRead.HasValue || !n.IsRead.Value);

            return View(notifications);
        }
    }
}
