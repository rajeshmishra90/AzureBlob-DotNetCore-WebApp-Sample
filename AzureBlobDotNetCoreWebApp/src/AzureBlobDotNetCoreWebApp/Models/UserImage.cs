using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobDotNetCoreWebApp.Models
{
    public class UserImage
    {
        public string Id { get; set; }

        public string ImageUrl { get; set; }

        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
