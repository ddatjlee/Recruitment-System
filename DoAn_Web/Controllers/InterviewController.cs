using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn_Web.Models;
using System;
using System.Threading.Tasks;

namespace DoAn_Web.Controllers
{
    public class InterviewController : Controller
    {
        private readonly RecruitmentSystemContext _context;

        public InterviewController(RecruitmentSystemContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateInterview(int applicationId, DateTime startTime, DateTime endTime, string interviewType, string location, string onlineLink, string notes)
        {
            var application = await _context.Applications
                .Include(a => a.JobPostings)
                .Include(a => a.Student)  
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (application == null)
            {
                TempData["ErrorMessage"] = "Ứng tuyển không tồn tại!";
                return RedirectToAction("Index", "Home");
            }

            var interview = new Interview
            {
                ApplicationId = applicationId,
                InterviewType = interviewType,
                StartTime = startTime,
                EndTime = endTime,
                Location = location,
                OnlineLink = onlineLink,
                Notes = notes,
                Result = "pending"
            };

            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();

            // Tạo thông báo cho sinh viên
            var notification = new Notification
            {
                UserId = application.StudentId,   
                UserType = "student",
                Message = $"Bạn đã được mời tham gia phỏng vấn cho công việc '{application.JobPostings.Title}' vào lúc {startTime:dd/MM/yyyy HH:mm}.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đơn phỏng vấn đã được gửi thành công!";
            return RedirectToAction("Index", "Jobs");
        }
        //// Công ty gửi đơn phỏng vấn cho sinh viên
        //[HttpPost]
        //public async Task<IActionResult> ScheduleInterview(int applicationId, string interviewType, DateTime startTime, DateTime endTime, string location, string onlineLink, string notes)
        //{
        //    var application = await _context.Applications
        //        .Include(a => a.Student) // Bao gồm thông tin sinh viên để gửi thông báo
        //        .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

        //    if (application == null)
        //    {
        //        TempData["ErrorMessage"] = "Ứng tuyển không tồn tại.";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var interview = new Interview
        //    {
        //        ApplicationId = applicationId,
        //        InterviewType = interviewType,
        //        StartTime = startTime,
        //        EndTime = endTime,
        //        Location = location,
        //        OnlineLink = onlineLink,
        //        Notes = notes,
        //        Result = "pending" // Đặt kết quả là "pending" khi phỏng vấn chưa diễn ra
        //    };

        //    // Thêm đơn phỏng vấn vào cơ sở dữ liệu
        //    _context.Interviews.Add(interview);
        //    await _context.SaveChangesAsync();

        //    // Tạo thông báo cho sinh viên
        //    var notification = new Notification
        //    {

        //        UserId = application.StudentId,  // Gửi thông báo cho sinh viên này
        //        UserType = "student",
        //        Message = $"Bạn đã được mời tham gia phỏng vấn cho công việc '{application.JobPostings.Title}' vào lúc {startTime}.",
        //        IsRead = false,
        //        CreatedAt = DateTime.Now
        //    };

        //    _context.Notifications.Add(notification);
        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = "Đơn phỏng vấn đã được gửi cho sinh viên.";
        //    return RedirectToAction("Index", "Home");
        //}

        //// Sinh viên xem danh sách các đơn phỏng vấn
        //public async Task<IActionResult> ViewInterviews()
        //{
        //    var studentId = HttpContext.Session.GetInt32("StudentId");

        //    if (!studentId.HasValue)
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    var interviews = await _context.Interviews
        //        .Where(i => i.Application.StudentId == studentId.Value)
        //        .Include(i => i.Application)
        //        .OrderByDescending(i => i.StartTime)
        //        .ToListAsync();

        //    return View(interviews);
        //}

        //// Sinh viên xem chi tiết đơn phỏng vấn
        //public async Task<IActionResult> InterviewDetails(int interviewId)
        //{
        //    var interview = await _context.Interviews
        //        .Include(i => i.Application)
        //        .ThenInclude(a => a.Student)
        //        .FirstOrDefaultAsync(i => i.InterviewId == interviewId);

        //    if (interview == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(interview);
        //}
    }
}
