using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiApp.Models
{
    public class UploadFormData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string filename { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string filepath { get; set; }
        [Column(TypeName = "float")]
        public float latitude { get; set; }
        [Column(TypeName = "float")]
        public float longitude { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string userName { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string useremail { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string userid { get; set; }

    }
    public class UploadFormDataRequest : UploadFormData
    {
        public IFormFile file { get; set; }
    }
}
