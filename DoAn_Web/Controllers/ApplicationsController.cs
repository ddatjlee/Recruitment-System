using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn_Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace DoAn_Web.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly RecruitmentSystemContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ApplicationsController(RecruitmentSystemContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> ApplyJob(int jobId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để ứng tuyển.";
                return RedirectToAction("Login", "Home");
            }

            var job = await _context.JobPostings
                .FirstOrDefaultAsync(j => j.JobId == jobId);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Công việc không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.JobId == jobId && a.StudentId == studentId);
            if (existingApplication != null)
            {
                TempData["ErrorMessage"] = "Bạn đã ứng tuyển công việc này rồi.";
                return RedirectToAction("Index", "Home");
            }

            var application = new Application
            {
                JobId = jobId,
                StudentId = studentId.Value
            };

            ViewBag.JobTitle = job.Title;
            return View(application);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyJob(Application application, IFormFile resumeFile)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để ứng tuyển.";
                return RedirectToAction("Login", "Home");
            }

            var job = await _context.JobPostings
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.JobId == application.JobId);
            if (job == null)
            {
                ViewBag.ErrorMessage = "Công việc không tồn tại.";
                ViewBag.JobTitle = "Công việc không xác định";
                return View(application);
            }

            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.JobId == application.JobId && a.StudentId == studentId);
            if (existingApplication != null)
            {
                ViewBag.ErrorMessage = "Bạn đã ứng tuyển công việc này rồi.";
                ViewBag.JobTitle = job.Title;
                return View(application);
            }


            if (resumeFile == null || resumeFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Vui lòng tải lên file CV.";
                ViewBag.JobTitle = job.Title;
                return View(application);
            }

            if (!resumeFile.FileName.EndsWith(".pdf"))
            {
                ViewBag.ErrorMessage = "File CV phải có định dạng PDF.";
                ViewBag.JobTitle = job.Title;
                return View(application);
            }

            try
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "resumes");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + ".pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await resumeFile.CopyToAsync(stream);
                }
                application.ResumeUrl = $"/resumes/{fileName}";
                application.StudentId = studentId.Value;
                application.AppliedAt = DateTime.Now;
                application.Status = "pending";

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                // Thêm thông báo cho sinh viên
                var studentNotification = new Notification
                {
                    UserId = studentId.Value,
                    UserType = "student",
                    Message = $"Bạn đã nộp đơn ứng tuyển cho vị trí {job.Title} tại {job.Company.Name}.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(studentNotification);

                // Thêm thông báo cho công ty
                var companyNotification = new Notification
                {
                    UserId = job.CompanyId,
                    UserType = "company",
                    Message = $"Có ứng viên mới nộp đơn cho vị trí {job.Title}.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(companyNotification);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Nộp đơn ứng tuyển thành công!";
                return RedirectToAction("", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Có lỗi xảy ra khi nộp đơn: {ex.Message}";
                ViewBag.JobTitle = job.Title;
                return View(application);
            }
        }
    }
}