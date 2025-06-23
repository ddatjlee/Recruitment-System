using System.Collections.Generic;

namespace DoAn_Web.Models
{
    public class HomeViewModel
    {
            public List<JobPosting> Jobs { get; set; }
            public List<Company> Companies { get; set; }
            public Company SelectedCompany { get; set; } // Thuộc tính này lưu công ty được chọn
        }
    }

