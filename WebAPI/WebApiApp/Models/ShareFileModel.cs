using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiApp.Models
{
    public class ShareFileModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int")]
        public int Id { get; set; }
        [Column(TypeName = "int")]
        public int imgId { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string userId { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string sharedTo { get; set; }
    }

    public class ShareFileResponseModel
    {
        public string sharedWith { get; set; }
        public string sharedFrom { get; set; }
        public string  ImageName { get; set; }
    }

    public class ShareFileViewModel
    {
        public ShareFileViewModel()
        {
            shareFiles = new List<ShareFileResponseModel>();
            UserList = new List<ApplicationUser>();
            uploadFormDatas = new List<UploadFormData>();
        }
        public List<ShareFileResponseModel> shareFiles { get; set; }
        public List<ApplicationUser> UserList { get; set; }
        public List<UploadFormData> uploadFormDatas { get; set; }
    }
}
