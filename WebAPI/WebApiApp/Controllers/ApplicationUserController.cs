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
        private readonly AuthenticationContext _dbcontext;
        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings, AuthenticationContext context, AuthenticationContext dbcontext)
        {
            _userManager = userManager;
            _singInManager = signInManager;
            _appSettings = appSettings.Value;
            _context = context;
            _dbcontext = dbcontext;
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
                    }
                    var listData = getAllListAtOnce(formData.userid);
                    return Ok(new { dbPath = dbPath, result = result, listData = listData });
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
        [Route("DeleteImg")]
        public async Task<IActionResult> DeleteImg(int id)
        {
            List<UploadFormData> result = new List<UploadFormData>();
            var img = new UploadFormData()
            {
                Id = id
            };

            using (_context)
            {
                _context.Remove<UploadFormData>(img);
                _context.SaveChanges();

                result = _context.UploadFormDatas.ToList();
            }
            return Ok(result);
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

                shareFile.shareFiles = (from item in _context.ShareFiles
                                        join img in _context.UploadFormDatas on item.imgId equals img.Id
                                        join user in _context.ApplicationUsers on item.userId equals user.Id
                                        join shareTo in _context.ApplicationUsers on item.userId equals shareTo.Id
                                        select new ShareFileResponseModel
                                        {
                                            ImageName = img.filename,
                                            sharedFrom = user.FullName,
                                            sharedWith = shareTo.FullName
                                        }).ToList();
                shareFile.UserList = _context.ApplicationUsers.ToList();
                shareFile.uploadFormDatas = _context.UploadFormDatas.ToList();
            }
            return Ok(shareFile);
        }

        private ShareFileViewModel getAllListAtOnce(string userId)
        {
            ShareFileViewModel shareFile = new ShareFileViewModel();
            using (_dbcontext)
            {
                shareFile.shareFiles = (from item in _dbcontext.ShareFiles
                                        join img in _dbcontext.UploadFormDatas on item.imgId equals img.Id
                                        join user in _dbcontext.ApplicationUsers on item.userId equals user.Id
                                        join shareTo in _dbcontext.ApplicationUsers on item.userId equals shareTo.Id
                                        where img.userid == userId && item.userId == userId
                                        select new ShareFileResponseModel
                                        {
                                            ImageName = img.filename,
                                            sharedFrom = user.FullName,
                                            sharedWith = shareTo.FullName
                                        }).ToList();
                shareFile.UserList = _dbcontext.ApplicationUsers.ToList();
                shareFile.uploadFormDatas = _dbcontext.UploadFormDatas.Where(x=>x.userid == userId).ToList();
            }
            return shareFile;
        }

        [HttpGet]
        [Route("getall")]
        public async Task<IActionResult> getall(string id)
        {
            var list = getAllListAtOnce(id);
            return Ok(list);
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