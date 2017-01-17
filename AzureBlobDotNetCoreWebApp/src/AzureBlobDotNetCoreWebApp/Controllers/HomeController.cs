using AzureBlobDotNetCoreWebApp.Data;
using AzureBlobDotNetCoreWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace AzureBlobDotNetCoreWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private BlobUtility utility;
        //private string accountName = "azurestoragemysample";
        //private string accountKey = "aBYvofuV8TfsshAgZlCxWgJPO36m9Cdt2M/dCJslV6PA3oIQxG8/7PnVjFjn1sZNy3mOSuO081Vaes3MsBrMgg==";
        //private string containerName = "photoalbumcontainer";
        private ApplicationDbContext _db;

        private SignInManager<ApplicationUser> _signInManager;
        private IConfigurationRoot _config;

        public HomeController(ApplicationDbContext db,
            SignInManager<ApplicationUser> signInManager,
            IConfigurationRoot config)
        {
            _db = db;
            _signInManager = signInManager;
            _config = config;
            utility = new BlobUtility(_config["BlobDetails:AccountName"], _config["BlobDetails:Accountkey"]);
        }

        public IActionResult Index()
        {
            string loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //string loggedInUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<UserImage> userImages = (from r in _db.UserImages where r.ApplicationUserId == loggedInUserId select r).ToList();
            ViewBag.PhotoCount = userImages.Count;
            return View(userImages);
        }

        public ActionResult DeleteImage(string id)
        {
            UserImage userImage = _db.UserImages.FirstOrDefault(x => x.Id == id);
            _db.UserImages.Remove(userImage);
            _db.SaveChanges();
            string BlobNameToDelete = userImage.ImageUrl.Split('/').Last();
            utility.DeleteBlob(BlobNameToDelete, _config["BlobDetails:ContainerName"]);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UploadImage(IFormFile file)
        {
            string loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int imageCount = (from r in _db.UserImages where r.ApplicationUserId == loggedInUserId select r).Count();
            if (imageCount >= 10)
            {
                return RedirectToAction("Index");
            }

            if (file != null)
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var fileContent = reader.BaseStream;
                    var parsedContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                    string fileName = parsedContentDisposition.FileName;
                    var result = utility.UploadBlob(fileName, _config["BlobDetails:ContainerName"], fileContent);
                    if (result != null)
                    {
                        UserImage userimage = new UserImage();
                        userimage.Id = new Random().Next().ToString();
                        userimage.ApplicationUserId = loggedInUserId;
                        result.Wait();
                        userimage.ImageUrl = result.Result.Uri.ToString();
                        _db.UserImages.Add(userimage);
                        _db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}