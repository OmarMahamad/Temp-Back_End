using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.DTOs
{
    public class AddressRequestDTo
    {
        public string? Street { get; set; }=null;
        public string? City { get; set; }=null;
    }
}
