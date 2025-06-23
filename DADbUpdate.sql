	-- Tạo database
	CREATE DATABASE RecruitmentSystem;
	GO
	USE RecruitmentSystem;
	GO
	-- Bảng loại hình công việc
	CREATE TABLE JobTypes (
		JobTypeID INT PRIMARY KEY IDENTITY(1,1),
		Name NVARCHAR(50) NOT NULL UNIQUE
	);

	-- Bảng mức độ kinh nghiệm
	CREATE TABLE ExperienceLevels (
		LevelID INT PRIMARY KEY IDENTITY(1,1),
		Name NVARCHAR(50) NOT NULL UNIQUE
	);

	-- Bảng địa điểm
	CREATE TABLE Locations (
		LocationID INT PRIMARY KEY IDENTITY(1,1),
		City NVARCHAR(100) NOT NULL,
		Country NVARCHAR(100) NOT NULL,
		CONSTRAINT UC_CityCountry UNIQUE (City, Country)
	);

	-- Bảng công ty
	CREATE TABLE Companies (
		CompanyID INT PRIMARY KEY IDENTITY(1,1),
		Name NVARCHAR(255) NOT NULL,
		TaxCode NVARCHAR(20) UNIQUE NOT NULL,
		Email NVARCHAR(255) UNIQUE NOT NULL,
		PasswordHash NVARCHAR(255) NOT NULL,
		Phone NVARCHAR(20) NOT NULL,
		Website NVARCHAR(255),
		Description NVARCHAR(MAX),
		LogoUrl NVARCHAR(512),
		IsLocked BIT NOT NULL DEFAULT 0,
		Address NVARCHAR(MAX),
		Verified BIT DEFAULT 0,
		CreatedAt DATETIME2 DEFAULT GETDATE(),
		UpdatedAt DATETIME2 DEFAULT GETDATE()
	);
	-- Bảng sinh viên (chỉ dành cho khoa CNTT của một trường cụ thể)
	CREATE TABLE Students (
		StudentID INT PRIMARY KEY IDENTITY(1,1),
		StudentCode NVARCHAR(20) UNIQUE NOT NULL, -- Mã sinh viên thay cho email cá nhân
		PasswordHash NVARCHAR(255) NOT NULL,
		FullName NVARCHAR(255) NOT NULL,
		AvatarUrl NVARCHAR(512),
		DateOfBirth DATE,
		Phone NVARCHAR(20),
		GPA DECIMAL(3,2),
		IsLocked BIT NOT NULL DEFAULT 0,
		GraduationYear INT,
		GitHubProfile NVARCHAR(255),
		CreatedAt DATETIME2 DEFAULT GETDATE(),
		UpdatedAt DATETIME2 DEFAULT GETDATE()
	);
	-- Bảng kỹ năng (chủ yếu liên quan đến CNTT)
	CREATE TABLE Skills (
		SkillID INT PRIMARY KEY IDENTITY(1,1),
		Name NVARCHAR(100) NOT NULL UNIQUE,
		Description NVARCHAR(MAX)
	);
	-- Bảng kỹ năng của sinh viên
	CREATE TABLE StudentSkills (
		StudentID INT NOT NULL,
		SkillID INT NOT NULL,
		PRIMARY KEY (StudentID, SkillID),
		FOREIGN KEY (StudentID) REFERENCES Students(StudentID) ON DELETE CASCADE,
		FOREIGN KEY (SkillID) REFERENCES Skills(SkillID)
	);
	-- Bảng tin tuyển dụng
	CREATE TABLE JobPostings (
		JobID INT PRIMARY KEY IDENTITY(1,1),
		CompanyID INT NOT NULL,
		JobTypeID INT NOT NULL,
		LevelID INT NOT NULL,
		LocationID INT,
		Title NVARCHAR(255) NOT NULL,
		Description NVARCHAR(MAX) NOT NULL,
		Requirements NVARCHAR(MAX) NOT NULL,
		Benefits NVARCHAR(MAX),
		SalaryRange NVARCHAR(100),
		ApplicationDeadline DATETIME2 NOT NULL,
		Vacancies INT DEFAULT 1,
		IsActive BIT DEFAULT 1,
		IsApproved BIT DEFAULT 0,
		CreatedAt DATETIME2 DEFAULT GETDATE(),
		UpdatedAt DATETIME2 DEFAULT GETDATE(),
		FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID) ON DELETE CASCADE,
		FOREIGN KEY (JobTypeID) REFERENCES JobTypes(JobTypeID),
		FOREIGN KEY (LevelID) REFERENCES ExperienceLevels(LevelID),
		FOREIGN KEY (LocationID) REFERENCES Locations(LocationID)
	);
	-- Bảng kỹ năng yêu cầu cho công việc
	CREATE TABLE JobSkills (
		JobID INT NOT NULL,
		SkillID INT NOT NULL,
		PRIMARY KEY (JobID, SkillID),
		FOREIGN KEY (JobID) REFERENCES JobPostings(JobID) ON DELETE CASCADE,
		FOREIGN KEY (SkillID) REFERENCES Skills(SkillID)
	);
	-- Bảng ứng tuyển
	CREATE TABLE Applications (
		ApplicationID INT PRIMARY KEY IDENTITY(1,1),
		JobID INT NOT NULL,
		StudentID INT NOT NULL,
		CoverLetter NVARCHAR(MAX),
		ResumeUrl NVARCHAR(512) NOT NULL,
		Status NVARCHAR(20) NOT NULL CHECK (Status IN ('pending', 'reviewing', 'approved', 'rejected')) DEFAULT 'pending',
		AppliedAt DATETIME2 DEFAULT GETDATE(),
		ReviewedAt DATETIME2,
		FOREIGN KEY (JobID) REFERENCES JobPostings(JobID) ON DELETE CASCADE,
		FOREIGN KEY (StudentID) REFERENCES Students(StudentID) ON DELETE CASCADE,
		CONSTRAINT UC_JobStudent UNIQUE (JobID, StudentID)
	);
	-- Bảng phỏng vấn
	CREATE TABLE Interviews (
		InterviewID INT PRIMARY KEY IDENTITY(1,1),
		ApplicationID INT NOT NULL,
		InterviewType NVARCHAR(20) CHECK (InterviewType IN ('online', 'in-person')) NOT NULL,
		StartTime DATETIME2 NOT NULL,
		EndTime DATETIME2 NOT NULL,
		Location NVARCHAR(MAX),
		OnlineLink NVARCHAR(512),
		Notes NVARCHAR(MAX),
		Result NVARCHAR(20) CHECK (Result IN ('passed', 'failed', 'pending')) DEFAULT 'pending',
		FOREIGN KEY (ApplicationID) REFERENCES Applications(ApplicationID) ON DELETE CASCADE
	);

	-- Bảng quản trị viên
	CREATE TABLE Admins (
		AdminID INT PRIMARY KEY IDENTITY(1,1),
		Email NVARCHAR(255) UNIQUE NOT NULL,
		PasswordHash NVARCHAR(255) NOT NULL,
		FullName NVARCHAR(255) NOT NULL,
		CreatedAt DATETIME2 DEFAULT GETDATE()
	);

	-- Bảng thông báo
	CREATE TABLE Notifications (
		NotificationID INT PRIMARY KEY IDENTITY(1,1),
		UserID INT NOT NULL,
		UserType NVARCHAR(20) CHECK (UserType IN ('student', 'company', 'admin')) NOT NULL,
		Message NVARCHAR(MAX) NOT NULL,
		IsRead BIT DEFAULT 0,
		CreatedAt DATETIME2 DEFAULT GETDATE()
	);
	CREATE TABLE ApprovalHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    JobId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- "Approved" hoặc "Rejected"
    ActionDate DATETIME NOT NULL,
    AdminId INT NOT NULL,
    CONSTRAINT FK_ApprovalHistory_JobPostings FOREIGN KEY (JobId) REFERENCES JobPostings(JobId)
	);

	-- Indexes để tối ưu truy vấn
	CREATE INDEX idx_CompanyName ON Companies(Name);
	CREATE INDEX idx_JobTitle ON JobPostings(Title);
	CREATE INDEX idx_ApplicationStatus ON Applications(Status);
	CREATE INDEX idx_StudentCode ON Students(StudentCode);
	GO
	INSERT INTO JobTypes (Name) VALUES
	(N'Toàn thời gian'),
	(N'Bán thời gian'),
	(N'Thực tập');

	INSERT INTO ExperienceLevels (Name) VALUES
	(N'Không yêu cầu'),
	(N'Dưới 1 năm'),
	(N'1-3 năm');

	INSERT INTO Locations (City, Country) VALUES
	(N'Hà Nội', N'Việt Nam'),
	(N'TP.HCM', N'Việt Nam'),
	(N'Đà Nẵng', N'Việt Nam');

	INSERT INTO Companies (Name, TaxCode, Email,PasswordHash, Phone, Website, Description, LogoUrl, Address, Verified) VALUES
	(N'Công ty FPT Software', N'0101234567', N'recruit@fpt.com','$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'0241234567', N'https://fptsoftware.com', N'FPT Software là một trong những tập đoàn công nghệ hàng đầu tại Việt Nam, chuyên cung cấp các giải pháp công nghệ thông tin toàn diện cho khách hàng trên toàn cầu. Với đội ngũ kỹ sư giàu kinh nghiệm và công nghệ tiên tiến, FPT Software cam kết mang lại sản phẩm chất lượng cao, giúp chuyển đổi số và tối ưu hóa quy trình kinh doanh của các doanh nghiệp.', N'/images/logos/2c098358-fdca-4a0d-b8ed-f1f67439bcd9.png', N'123 Đường Láng, Hà Nội', 1),
	(N'Công ty TMA Solutions', N'0209876543', N'hr@tma.com','$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'0289876543', N'https://tmasolutions.com',  N'TMA Solutions là công ty outsourcing CNTT hàng đầu, chuyên cung cấp dịch vụ phát triển phần mềm, kiểm thử và bảo trì ứng dụng cho khách hàng trong và ngoài nước. Với tầm nhìn đổi mới sáng tạo và cam kết chất lượng, công ty đã xây dựng mối quan hệ bền vững với nhiều tập đoàn lớn, mang đến các giải pháp công nghệ đột phá đáp ứng mọi nhu cầu kinh doanh.', N'/images/logos/add6c18e-c904-488f-861f-19d299ebca9e.png', N'456 Nguyễn Thị Minh Khai, TP.HCM', 1),
	(N'Công ty Axon Active', N'0304567891', N'jobs@axonactive.com','$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'0236456789', N'https://axonactive.com',N'Axon Active là công ty phát triển phần mềm quốc tế, chuyên cung cấp các giải pháp công nghệ thông tin chất lượng cao cho khách hàng toàn cầu. Với văn hóa sáng tạo và môi trường làm việc linh hoạt, Axon Active tập trung vào phát triển các sản phẩm tiên tiến, góp phần thúc đẩy sự chuyển đổi số và tăng trưởng bền vững cho doanh nghiệp.',N'/images/logos/2a5aa8a0-cbb9-414c-b924-3791de5c7995.png', N'789 Trần Hưng Đạo, Đà Nẵng', 1),
	(N'Công ty Techcombank', N'0401122334', N'hr@techcombank.com','$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'0245678901', N'https://techcombank.com',  N'Techcombank là một trong những ngân hàng thương mại hàng đầu Việt Nam, cung cấp đa dạng các dịch vụ tài chính hiện đại và an toàn. Với triết lý "Sáng tạo - Chuyên nghiệp - Tận tâm", Techcombank không ngừng đổi mới công nghệ, mở rộng dịch vụ nhằm mang đến trải nghiệm tốt nhất cho khách hàng và góp phần vào sự phát triển kinh tế của đất nước.', N'/images/logos/17b0c322-8f0f-4f57-88c8-dd04abf15cde.png', N'191 Bà Triệu, Hà Nội', 1),
	(N'Công ty VNG Corporation', N'0501234567', N'hr@vng.com',N'$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'0281234567', N'https://vng.com', N'VNG Corporation là một trong những tập đoàn công nghệ và giải trí số hàng đầu tại Việt Nam, cung cấp đa dạng các dịch vụ trong lĩnh vực giải trí kỹ thuật số, thương mại điện tử và công nghệ tài chính. Với tinh thần đổi mới sáng tạo và năng động, VNG cam kết mang lại các giải pháp số tiên tiến nhằm nâng cao trải nghiệm người dùng và thúc đẩy quá trình chuyển đổi số trên toàn quốc.',N'/images/logos/e881c0df-d121-4d30-a221-5c8838a42835.png',N'45 Lê Duẩn, Hà Nội', 1),
	(N'Công ty Viettel Group', N'0601234567', N'hr@viettel.com',N'$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'02411223344', N'https://viettel.com', N'Viettel Group là nhà cung cấp dịch vụ viễn thông hàng đầu Việt Nam, nổi bật với công nghệ tiên tiến và dịch vụ toàn diện. Công ty đóng góp quan trọng trong việc xây dựng hạ tầng số, nâng cao kết nối quốc gia và quốc tế, từ đó mang lại trải nghiệm liên lạc chất lượng cho hàng triệu người dùng.',N'/images/logos/cdb3b487-8eaf-4599-9dba-609f6968bcf3.png', N'10 Phan Chu Trinh, Hà Nội', 1),
	(N'Công ty CMC Corporation', N'0701234567', N'hr@cmc.com',N'$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'0289988776', N'https://cmc.com', N'CMC Corporation là nhà cung cấp giải pháp công nghệ thông tin hàng đầu tại Việt Nam, chuyên về chuyển đổi số, an ninh mạng và phát triển phần mềm. Với đội ngũ chuyên gia giàu kinh nghiệm, CMC không ngừng đổi mới để thúc đẩy tiến bộ công nghệ và tối ưu hóa hiệu quả kinh doanh cho các doanh nghiệp trong và ngoài nước.',N'/images/logos/002364c1-31d2-4eed-a8fc-0aa8d5988f22.png', N'123 Nguyễn Văn Cừ, TP.HCM',1),
	(N'Công ty Zalo Group', N'0801234567', N'hr@zalogroup.com',N'$2a$11$mn.Rv6CtXMtlsQr0H0YlceRTE2X6HG6p/6.14ae5ndeNzePCfsMYy', N'02833445566', N'https://zalogroup.com', N'Zalo Group là công ty công nghệ nổi bật với ứng dụng nhắn tin phổ biến và các dịch vụ số đa dạng. Tập trung vào trải nghiệm người dùng và sự đổi mới liên tục, Zalo Group đóng góp tích cực vào việc xây dựng hệ sinh thái số tại Việt Nam, tạo nền tảng cho giao tiếp và kết nối hiện đại.',N'/images/logos/01950917-68db-4134-b264-60f89f96866a.png', N'50 Lê Lợi, TP.HCM',1);
	
	INSERT INTO Students (StudentCode, PasswordHash, FullName, AvatarUrl, DateOfBirth, Phone, GPA, GraduationYear, GitHubProfile) VALUES
	(N'20201234', N'hash123', N'Nguyễn Văn A', N'avatar_a.png', '2002-05-15', N'0912345678', 3.5, 2024, N'github.com/nguyenvana'),
	(N'20205678', N'hash456', N'Trần Thị B', N'avatar_b.png', '2001-08-20', N'0987654321', 3.8, 2023, N'github.com/tranthib'),
	(N'20209012', N'hash789', N'Lê Văn C', N'avatar_c.png', '2003-01-10', N'0934567890', 3.2, 2025, N'github.com/levanc');

	INSERT INTO Skills (Name, Description) VALUES
	(N'C#', N'Ngôn ngữ lập trình hướng đối tượng'),
	(N'Java', N'Ngôn ngữ lập trình đa nền tảng'),
	(N'SQL', N'Quản lý cơ sở dữ liệu'),
	(N'HTML/CSS', N'Thiết kế giao diện web'),
	(N'Python', N'Lập trình AI và dữ liệu');

	INSERT INTO StudentSkills (StudentID, SkillID) VALUES
	(1, 1), -- Nguyễn Văn A biết C#
	(1, 3), -- Nguyễn Văn A biết SQL
	(2, 2), -- Trần Thị B biết Java
	(2, 4), -- Trần Thị B biết HTML/CSS
	(3, 5); -- Lê Văn C biết Python
	SELECT * FROM JobPostings WHERE CompanyId = 1;
		SELECT * FROM Students WHERE StudentId = 1;
	INSERT INTO JobPostings (CompanyID, JobTypeID, LevelID, LocationID, Title, Description, Requirements, Benefits, SalaryRange, ApplicationDeadline, Vacancies) VALUES
	(1, 1, 1, 1, N'Lập trình viên C#', N'Phát triển ứng dụng doanh nghiệp', N'Biết C#, SQL', N'Lương cao, bảo hiểm', N'10-15 triệu', '2025-04-01', 2),
	(2, 3, 1, 2, N'Thực tập sinh Java', N'Hỗ trợ dự án web', N'Cơ bản Java, HTML', N'Hỗ trợ chi phí, đào tạo', N'3-5 triệu', '2025-03-20', 3),
	(3, 2, 2, 3, N'Kỹ sư phần mềm Python', N'Phát triển AI', N'Python, 1 năm kinh nghiệm', N'Môi trường quốc tế', N'15-20 triệu', '2025-03-30', 1),
	(4, 1, 1, 1, N'Chuyên viên tài chính', N'Hỗ trợ tài chính ngân hàng', N'Tốt nghiệp tài chính, biết Excel', N'Lương cạnh tranh, bảo hiểm', N'12-18 triệu', '2025-04-15', 2);

	INSERT INTO JobSkills (JobID, SkillID) VALUES
	(1, 1), -- Lập trình viên C# yêu cầu C#
	(1, 3), -- Lập trình viên C# yêu cầu SQL
	(2, 2), -- Thực tập sinh Java yêu cầu Java
	(2, 4), -- Thực tập sinh Java yêu cầu HTML/CSS
	(3, 5); -- Kỹ sư phần mềm Python yêu cầu Python

	INSERT INTO Applications (JobID, StudentID, CoverLetter, ResumeUrl, Status, AppliedAt) VALUES
	(1, 1, N'Tôi rất quan tâm đến vị trí này', N'resume_a.pdf', N'pending', '2025-03-10 10:00:00'),
	(2, 2, N'Mong được học hỏi tại TMA', N'resume_b.pdf', N'reviewing', '2025-03-11 14:30:00'),
	(3, 3, N'Tôi có kinh nghiệm Python', N'resume_c.pdf', N'approved', '2025-03-12 09:15:00');

	INSERT INTO Interviews (ApplicationID, InterviewType, StartTime, EndTime, Location, OnlineLink, Notes, Result) VALUES
	(3, N'online', '2025-03-15 10:00:00', '2025-03-15 11:00:00', NULL, N'zoom.us/123456', N'Phỏng vấn kỹ thuật', N'pending');

	INSERT INTO Admins (Email, PasswordHash, FullName) VALUES
	(N'admin@xyz.edu.vn', N'hashadmin', N'Nguyễn Thị Admin');

	INSERT INTO Notifications (UserID, UserType, Message, IsRead) VALUES
	(1, N'student', N'Đơn ứng tuyển của bạn đang được xem xét', 0),
	(3, N'company', N'Có ứng viên mới cho vị trí Kỹ sư Python', 0),
	(1, N'admin', N'Công ty Axon Active cần xác minh', 0);
