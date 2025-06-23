using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn_Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BCrypt.Net;
namespace DoAn_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly RecruitmentSystemContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(RecruitmentSystemContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Interviews()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (!studentId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch phỏng vấn.";
                return RedirectToAction("LoginStudent");
            }

            var interviews = await _context.Interviews
                .Include(i => i.Application)
                .ThenInclude(a => a.Job)
                .ThenInclude(j => j.Company)
                .Where(i => i.Application.StudentId == studentId.Value)
                .OrderBy(i => i.StartTime)
                .ToListAsync();

            return View(interviews);
        }
        public async Task<IActionResult> Index(int page = 1, int? companyId = null)
        {
            var expiredJobs = await _context.JobPostings
        .Where(j => j.IsActive == true && j.IsApproved == true && j.ApplicationDeadline <= DateTime.Today)
        .ToListAsync();

            if (expiredJobs.Any())
            {
                foreach (var job in expiredJobs)
                {
                    job.IsActive = false;
                }
                await _context.SaveChangesAsync();
            }
            int pageSize = 3; // Số lượng công việc hiển thị mỗi trang
            var totalJobs = await _context.JobPostings
                .Where(j => j.IsActive == true && j.IsApproved == true)
                .CountAsync(); // Đếm tổng số công việc

            var jobs = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.Location)
                .Where(j => j.IsActive == true && j.IsApproved == true)
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)  // Bỏ qua các công việc đã hiển thị
                .Take(pageSize)  // Lấy 3 công việc
                .ToListAsync();

            var companies = await _context.Companies
                .Where(c => c.Verified == true)
                .OrderByDescending(c => c.CreatedAt)
                .Take(4)
                .ToListAsync();

            // Nếu không có companyId, chọn công ty đầu tiên trong danh sách
            Company selectedCompany = null;
            if (companyId.HasValue)
            {
                selectedCompany = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId.Value);
            }
            else if (companies.Any())
            {
                selectedCompany = companies.First(); // Chọn công ty đầu tiên nếu không có companyId
            }

            var model = new HomeViewModel
            {
                Jobs = jobs,
                Companies = companies,
                SelectedCompany = selectedCompany // Thêm công ty được chọn vào model
            };

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                var unreadNotifications = await _context.Notifications
                    .CountAsync(n => n.UserId == studentId.Value && n.UserType == "student" && (n.IsRead.HasValue && !n.IsRead.Value));
                ViewBag.UnreadNotifications = unreadNotifications;
            }
            else
            {
                ViewBag.UnreadNotifications = 0;
            }

            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("StudentId") != null || HttpContext.Session.GetInt32("CompanyId") != null;

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(model);
        }

        public async Task<IActionResult> CompanyDetails(int companyId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null)
            {
                return Content("Không tìm thấy công ty."); // Trả về thông báo nếu không tìm thấy
            }
            return PartialView("_CompanyDetails", company);
        }

        public async Task<IActionResult> GetJobs(int page = 1)
        {
            int pageSize = 3; // Số lượng công việc mỗi trang
            var jobs = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.Location)
                .Where(j => j.IsActive == true && j.IsApproved == true)
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PartialView("_JobList", jobs);
        }

        [HttpGet]
        public async Task<IActionResult> SearchJobs(string keyword, int page = 1)
        {
            int pageSize = 3; 

            var query = _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.Location)
                .Include(j => j.JobType)
                .Include(j => j.Skills)
                .Where(j => j.IsActive == true && j.IsApproved == true && j.ApplicationDeadline >= DateTime.Today);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(j =>
                    j.Company.Name.ToLower().Contains(keyword) ||
                    (j.Location != null && j.Location.City != null && j.Location.City.ToLower().Contains(keyword)) ||
                    (j.JobType != null && j.JobType.Name != null && j.JobType.Name.ToLower().Contains(keyword)) ||
                    j.Skills.Any(s => s.Name != null && s.Name.ToLower().Contains(keyword))
                );
            }

            var totalJobs = await query.CountAsync();

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var companies = await _context.Companies
                .Where(c => c.Verified == true)
                .OrderByDescending(c => c.CreatedAt)
                .Take(4)
                .ToListAsync();

            var model = new HomeViewModel
            {
                Jobs = jobs,
                Companies = companies
            };

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                var unreadNotifications = await _context.Notifications
                    .CountAsync(n => n.UserId == studentId.Value && n.UserType == "student" && (n.IsRead.HasValue && !n.IsRead.Value));
                ViewBag.UnreadNotifications = unreadNotifications;
            }
            else
            {
                ViewBag.UnreadNotifications = 0;
            }

            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("StudentId") != null || HttpContext.Session.GetInt32("CompanyId") != null;

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);

            return View("Index", model);
        }


        [HttpGet]
        public IActionResult LoginStudent()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginStudent(string studentCode, string password)
        {
            if (string.IsNullOrEmpty(studentCode) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Mật khẩu hoặc tài khoản không đúng.";
                return View();
            }

            if (student.IsLocked == true)
            {
                TempData["ErrorMessage"] = "Tài khoản đã bị khóa!";
                return View();
            }

            // Sử dụng BCrypt để so sánh mật khẩu nhập vào với mật khẩu đã mã hóa
            if (!BCrypt.Net.BCrypt.Verify(password, student.PasswordHash))
            {
                TempData["ErrorMessage"] = "Mật khẩu hoặc tài khoản không đúng.";
                return View();
            }

            HttpContext.Session.SetInt32("StudentId", student.StudentId);
            HttpContext.Session.SetString("StudentName", student.FullName);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult LoginCompany()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginCompany(string companyEmail, string password)
        {
            if (string.IsNullOrEmpty(companyEmail) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == companyEmail);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Mật khẩu hoặc Email không đúng.";
                return View();
            }
            if (company.IsLocked == true)
            {
                TempData["ErrorMessage"] = "Tài khoản đã bị khóa!";
                return View();
            }

            // So sánh mật khẩu đã mã hóa
            if (!BCrypt.Net.BCrypt.Verify(password, company.PasswordHash))
            {
                TempData["ErrorMessage"] = "Mật khẩu hoặc Email không đúng.";
                return View();
            }

            HttpContext.Session.SetInt32("CompanyId", company.CompanyId);
            HttpContext.Session.SetString("CompanyName", company.Name);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult LoginAdmin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginAdmin(string adminEmail, string password)
        {
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                return View();
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null)
            {
                TempData["ErrorMessage"] = "Mật khẩu hoặc Email không đúng.";
                return View();
            }

            // So sánh mật khẩu đã mã hóa
            if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                TempData["ErrorMessage"] = "Mật khẩu hoặc Email không đúng.";
                return View();
            }

            HttpContext.Session.SetInt32("AdminId", admin.AdminId);
            HttpContext.Session.SetString("AdminName", admin.FullName);

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public IActionResult RegisterStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterStudent(string fullName, string studentCode, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(studentCode) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            if (!Regex.IsMatch(studentCode, @"^\d{10}$"))
            {
                TempData["ErrorMessage"] = "Mã số sinh viên phải chứa đúng 10 chữ số.";
                return View();
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu nhập lại không khớp.";
                return View();
            }

            var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
            if (existingStudent != null)
            {
                TempData["ErrorMessage"] = "Mã sinh viên đã tồn tại. Vui lòng chọn mã khác.";
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var student = new Student
            {
                FullName = fullName,
                StudentCode = studentCode,
                PasswordHash = hashedPassword, 
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("LoginStudent", "Home");
        }

        [HttpGet]
        public IActionResult RegisterCompany()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCompany(string companyName, string companyTaxCode, string companyEmail, string companyPhone, string companyWebsite, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(companyTaxCode) ||
                string.IsNullOrEmpty(companyEmail) || string.IsNullOrEmpty(companyPhone) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            if (!Regex.IsMatch(companyEmail, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
            {
                TempData["ErrorMessage"] = "Email công ty phải có đuôi @gmail.com.";
                return View();
            }

            if (!Regex.IsMatch(companyPhone, @"^\d{10}$"))
            {
                TempData["ErrorMessage"] = "Số điện thoại phải chứa đúng 10 chữ số.";
                return View();
            }

            if (!Regex.IsMatch(companyTaxCode, @"^\d{10,13}$"))
            {
                TempData["ErrorMessage"] = "Mã số thuế phải chứa từ 10 đến 13 chữ số.";
                return View();
            }

            if (!string.IsNullOrEmpty(companyWebsite) && !Regex.IsMatch(companyWebsite, @"^https?://"))
            {
                TempData["ErrorMessage"] = "Website phải bắt đầu bằng http:// hoặc https://.";
                return View();
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu nhập lại không khớp.";
                return View();
            }

            var existingCompany = await _context.Companies.FirstOrDefaultAsync(c => c.Email == companyEmail);
            if (existingCompany != null)
            {
                TempData["ErrorMessage"] = "Email công ty đã tồn tại.";
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var company = new Company
            {
                Name = companyName,
                TaxCode = companyTaxCode,
                Email = companyEmail,
                Phone = companyPhone,
                Website = companyWebsite,
                PasswordHash = hashedPassword, 
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký công ty thành công!";
            return RedirectToAction("LoginCompany", "Home");
        }



        public async Task<IActionResult> Details(int id)
        {
            var jobPosting = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.Location)
                .Include(j => j.JobType)
                .Include(j => j.Level)
                .Include(j => j.Skills)
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (jobPosting == null)
            {
                return NotFound();
            }

            return View(jobPosting); 
        }

        public async Task<IActionResult> Profile()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login");
            }

            var student = await _context.Students
                .Include(s => s.Skills)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Skills = await _context.Skills.ToListAsync();
            return View(student);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(int StudentId, IFormFile avatarFile, DateTime? DateOfBirth, string Phone, decimal? GPA, int? GraduationYear, string GitHubProfile, string[] SelectedSkillIds)
        {
            var student = await _context.Students
                .Include(s => s.Skills)
                .FirstOrDefaultAsync(s => s.StudentId == StudentId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Sinh viên không tồn tại.";
                ViewBag.Skills = await _context.Skills.ToListAsync();
                return View("Profile", student);
            }

            if (DateOfBirth.HasValue)
            {
                if (DateOfBirth.Value > DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Ngày sinh không được là ngày trong tương lai.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }
            }

            if (!string.IsNullOrEmpty(Phone))
            {
                if (!Regex.IsMatch(Phone, @"^\d{10}$"))
                {
                    TempData["ErrorMessage"] = "Số điện thoại phải chứa đúng 10 chữ số.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }
            }

            if (GPA.HasValue)
            {
                if (GPA < 0.0m || GPA > 4.0m) 
                {
                    TempData["ErrorMessage"] = "GPA phải nằm trong khoảng từ 0.0 đến 4.0.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }
            }

            if (GraduationYear.HasValue)
            {
                int currentYear = DateTime.Now.Year;
                if (GraduationYear < 2000 || GraduationYear > currentYear)
                {
                    TempData["ErrorMessage"] = $"Năm tốt nghiệp phải nằm trong khoảng từ 2000 đến {currentYear}.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }
            }

            if (!string.IsNullOrEmpty(GitHubProfile))
            {
                if (!Regex.IsMatch(GitHubProfile, @"^https?://"))
                {
                    TempData["ErrorMessage"] = "GitHub profile phải bắt đầu bằng http:// hoặc https://.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }
            }

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Ảnh đại diện chỉ hỗ trợ định dạng .jpg, .jpeg, .png, .gif.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }

                if (avatarFile.Length > 5 * 1024 * 1024) 
                {
                    TempData["ErrorMessage"] = "Ảnh đại diện không được lớn hơn 5MB.";
                    ViewBag.Skills = await _context.Skills.ToListAsync();
                    return View("Profile", student);
                }

                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/avatars");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                student.AvatarUrl = $"/images/avatars/{fileName}";
            }

            student.DateOfBirth = DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : null;
            student.Phone = Phone;
            student.Gpa = GPA;
            student.GraduationYear = GraduationYear;
            student.GitHubProfile = GitHubProfile;
            student.UpdatedAt = DateTime.Now;

            if (SelectedSkillIds != null && SelectedSkillIds.Any())
            {
                student.Skills.Clear();

                var selectedSkills = await _context.Skills
                    .Where(s => SelectedSkillIds.Contains(s.SkillId.ToString()))
                    .ToListAsync();
                foreach (var skill in selectedSkills)
                {
                    student.Skills.Add(skill);
                }
            }

            _context.Update(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thành công!";
            ViewBag.Skills = await _context.Skills.ToListAsync();
            return View("Profile", student);
        }
        public async Task<IActionResult> ProfileCompany()
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");
            if (companyId == null)
            {
                TempData["ErrorMessage"] = "Công ty không tồn tại.";
                return RedirectToAction("LoginCompany");
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null)
            {
                return RedirectToAction("LoginCompany");
            }

            return View(company);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileCompany(int companyId, IFormFile logoFile, string phone, string website, string description, string address)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Công ty không tồn tại.";
                return View("ProfileCompany", company);
            }

            if (!string.IsNullOrEmpty(phone) && !Regex.IsMatch(phone, @"^\d{10}$"))
            {
                TempData["ErrorMessage"] = "Số điện thoại phải chứa đúng 10 chữ số.";
                return View("ProfileCompany", company);
            }

            if (!string.IsNullOrEmpty(website) && !Regex.IsMatch(website, @"^https?://"))
            {
                TempData["ErrorMessage"] = "Website phải bắt đầu bằng http:// hoặc https://.";
                return View("ProfileCompany", company);
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(logoFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Logo chỉ hỗ trợ định dạng .jpg, .jpeg, .png, .gif.";
                    return View("ProfileCompany", company);
                }

                if (logoFile.Length > 5 * 1024 * 1024) 
                {
                    TempData["ErrorMessage"] = "Logo không được lớn hơn 5MB.";
                    return View("ProfileCompany", company);
                }

                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/logos");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }
                company.LogoUrl = $"/images/logos/{fileName}";
            }

            company.Phone = phone;
            company.Website = website;
            company.Description = description;
            company.Address = address;
            company.UpdatedAt = DateTime.Now;

            _context.Update(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thông tin công ty thành công!";
            return View("ProfileCompany", company);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}