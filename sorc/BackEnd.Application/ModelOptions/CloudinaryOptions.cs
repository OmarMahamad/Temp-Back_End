using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.ModelOptions
{
    public sealed class CloudinaryOptions
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string Folder { get; set; } = "generated_images";
        public string DeliveryFormat { get; set; } = "webp";
    }
}
