using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.Entity
{
    public class Address
    {
        [Key]
        public Guid Id { get; private set; } = new Guid();
        public string City { get; private set; }
        public string Street { get; private set; }


        private Address(string city, string Street)
        {
            SetCity(city);
            SetStreet(Street);
        }

        public static Address Create(string city, string street)
        {
            var address = new Address(city, street);
            address.Id = Guid.NewGuid(); // ✅ توليد Id حقيقي
            return address;
        }

        public void SetCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty.");
            City = city;
        }
        public void SetStreet(string street)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be empty.");
            Street = street;
        }
    }
}
