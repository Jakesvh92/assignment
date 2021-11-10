using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApiApp.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _singInManager;
        private readonly ApplicationSettings _appSettings;
        private readonly AuthenticationContext _context;
        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings, AuthenticationContext context)
        {
            _userManager = userManager;
            _singInManager = signInManager;
            _appSettings = appSettings.Value;
            _context = context;
        }

        [HttpPost]
        [Route("Register")]
        //POST : /api/ApplicationUser/Register
        public async Task<Object> PostApplicationUser(ApplicationUserModel model)
        {
            var applicationUser = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            try
            {
                var result = await _userManager.CreateAsync(applicationUser, model.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        [Route("Login")]
        //POST : /api/ApplicationUser/Login
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID",user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                var listData = getAllListAtOnce(user.Id);
                return Ok(new { token = token, user = user, listData = listData });
            }
            else
                return BadRequest(new { message = "Username or password is incorrect." });
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("Upload")]
        public IActionResult Upload([FromForm] UploadFormDataRequest formData)
        {
            ShareFileViewModel shareFile = new ShareFileViewModel();
            try
            {
                var file = formData.file;
                var folderName = Path.Combine(@"wwwroot\Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                List<UploadFormData> result = new List<UploadFormData>();
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    using (_context)
                    {
                        UploadFormData ufd = new UploadFormData
                        {
                            filename = fileName,
                            filepath = fullPath,
                            longitude = formData.longitude,
                            latitude = formData.latitude,
                            useremail = formData.useremail,
                            userid = formData.userid,
                            userName = formData.userName,
                            imgtype = formData.imgtype,
                            tags = formData.tags,
                            captureBy = formData.userid,
                            captureDate = DateTime.Now
                        };
                        _context.UploadFormDatas.Add(ufd);
                        _context.SaveChanges();

                        result = _context.UploadFormDatas.ToList();

                        shareFile.shareFilestoOthers = (from item in _context.ShareFiles
                                                join img in _context.UploadFormDatas on item.imgId equals img.Id
                                                join user in _context.ApplicationUsers on item.userId equals user.Id
                                                join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                                where item.userId == formData.userid
                                                select new ShareFileResponseModel
                                                {
                                                    ImageName = img.filename,
                                                    sharedFrom = user.FullName + "_" + user.UserName,
                                                    sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                    ImageAlbum = img.imgtype
                                                }).Distinct().ToList();
                        shareFile.shareFilestoMe = (from item in _context.ShareFiles
                                                    join img in _context.UploadFormDatas on item.imgId equals img.Id
                                                    join user in _context.ApplicationUsers on item.userId equals user.Id
                                                    join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                                    where item.sharedTo == formData.userid
                                                    select new ShareFileResponseModel
                                                    {
                                                        ImageName = img.filename,
                                                        sharedFrom = user.FullName + "_" + user.UserName,
                                                        sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                        ImageAlbum = img.imgtype
                                                    }).Distinct().ToList();
                        shareFile.UserList = _context.ApplicationUsers.Where(u => u.Id != formData.userid).ToList();
                        shareFile.uploadFormDatas = _context.UploadFormDatas.Where(x => x.userid == formData.userid).ToList();
                    }
                    return Ok(new { dbPath = dbPath, result = result, listData = shareFile });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpGet]
        [Route("DeleteImg/{id}/{userId}")]
        public async Task<IActionResult> DeleteImg(string id, string userId)
        {
            ShareFileViewModel shareFile = new ShareFileViewModel();
            var img1 = new UploadFormData()
            {
                Id = string.IsNullOrWhiteSpace(id) ? 0 : Convert.ToInt32(id)
            };

            using (_context)
            {
                _context.Remove<UploadFormData>(img1);
                _context.SaveChanges();

                shareFile.shareFilestoOthers = (from item in _context.ShareFiles
                                                join img in _context.UploadFormDatas on item.imgId equals img.Id
                                                join user in _context.ApplicationUsers on item.userId equals user.Id
                                                join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                                where item.userId == userId
                                                select new ShareFileResponseModel
                                                {
                                                    ImageName = img.filename,
                                                    sharedFrom = user.FullName + "_" + user.UserName,
                                                    sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                    ImageAlbum = img.imgtype
                                                }).Distinct().ToList();
                shareFile.shareFilestoMe = (from item in _context.ShareFiles
                                            join img in _context.UploadFormDatas on item.imgId equals img.Id
                                            join user in _context.ApplicationUsers on item.userId equals user.Id
                                            join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                            where item.sharedTo == userId
                                            select new ShareFileResponseModel
                                            {
                                                ImageName = img.filename,
                                                sharedFrom = user.FullName + "_" + user.UserName,
                                                sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                ImageAlbum = img.imgtype
                                            }).Distinct().ToList();
                shareFile.UserList = _context.ApplicationUsers.Where(u => u.Id != userId).ToList();
                shareFile.uploadFormDatas = _context.UploadFormDatas.Where(u => u.captureBy == userId).ToList();
            }
            return Ok(new { listData = shareFile });
        }

        [HttpPost]
        [Route("SaveSharedImage")]
        public async Task<IActionResult> SaveSharedImage([FromForm] ShareFileModel formData)
        {
            ShareFileViewModel shareFile = new ShareFileViewModel();
            using (_context)
            {
                _context.ShareFiles.Add(formData);
                _context.SaveChanges();

                shareFile.shareFilestoOthers = (from item in _context.ShareFiles
                                                join img in _context.UploadFormDatas on item.imgId equals img.Id
                                                join user in _context.ApplicationUsers on item.userId equals user.Id
                                                join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                                where item.userId == formData.userId
                                                select new ShareFileResponseModel
                                                {
                                                    ImageName = img.filename,
                                                    sharedFrom = user.FullName + "_" + user.UserName,
                                                    sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                    ImageAlbum = img.imgtype
                                                }).Distinct().ToList();
                shareFile.shareFilestoMe = (from item in _context.ShareFiles
                                            join img in _context.UploadFormDatas on item.imgId equals img.Id
                                            join user in _context.ApplicationUsers on item.userId equals user.Id
                                            join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                            where item.sharedTo == formData.userId
                                            select new ShareFileResponseModel
                                            {
                                                ImageName = img.filename,
                                                sharedFrom = user.FullName + "_" + user.UserName,
                                                sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                ImageAlbum = img.imgtype
                                            }).Distinct().ToList();
                shareFile.UserList = _context.ApplicationUsers.Where(u => u.Id != formData.userId).ToList();
                shareFile.uploadFormDatas = _context.UploadFormDatas.Where(u => u.captureBy == formData.userId).ToList();
            }
            return Ok(new { listData = shareFile });
        }

        private ShareFileViewModel getAllListAtOnce(string userId)
        {
            ShareFileViewModel shareFile = new ShareFileViewModel();
            using (_context)
            {
                shareFile.shareFilestoOthers = (from item in _context.ShareFiles
                                                join img in _context.UploadFormDatas on item.imgId equals img.Id
                                                join user in _context.ApplicationUsers on item.userId equals user.Id
                                                join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                                where item.userId == userId
                                                select new ShareFileResponseModel
                                                {
                                                    ImageName = img.filename,
                                                    sharedFrom = user.FullName + "_" + user.UserName,
                                                    sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                    ImageAlbum = img.imgtype
                                                }).Distinct().ToList();
                shareFile.shareFilestoMe = (from item in _context.ShareFiles
                                            join img in _context.UploadFormDatas on item.imgId equals img.Id
                                            join user in _context.ApplicationUsers on item.userId equals user.Id
                                            join shareTo in _context.ApplicationUsers on item.sharedTo equals shareTo.Id
                                            where item.sharedTo == userId
                                            select new ShareFileResponseModel
                                            {
                                                ImageName = img.filename,
                                                sharedFrom = user.FullName + "_" + user.UserName,
                                                sharedWith = shareTo.FullName + "_" + shareTo.UserName,
                                                ImageAlbum = img.imgtype
                                            }).Distinct().ToList();
                shareFile.UserList = _context.ApplicationUsers.Where(u => u.Id != userId).ToList();
                shareFile.uploadFormDatas = _context.UploadFormDatas.Where(u => u.captureBy == userId).ToList();
            }
            return shareFile;
        }

        [HttpGet]
        [Route("getall/{id}")]
        public async Task<IActionResult> getall(string id)
        {
            var list = getAllListAtOnce(id);
            return Ok(new { listData = list });
        }
        [HttpGet, DisableRequestSizeLimit]
        [Route("download")]
        public async Task<IActionResult> Download([FromQuery] string fileUrl)
        {
            var folderName = Path.Combine(@"wwwroot\Resources", "Images");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileUrl);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var memory = new MemoryStream();
            await using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(filePath), filePath);
        }
        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }
    }
}