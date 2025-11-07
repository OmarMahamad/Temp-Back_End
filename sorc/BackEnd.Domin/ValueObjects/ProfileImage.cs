using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.ValueObjects
{
    public class ProfileImage
    {
        public string? Url { get; }
        public string? PublicId { get; }

        private ProfileImage() { }

        public ProfileImage(string? url, string? publicId)
        {
            Url = url;
            PublicId = publicId;
        }
    }

}
