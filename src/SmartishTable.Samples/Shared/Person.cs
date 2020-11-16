using System;
using System.Collections.Generic;
using System.Text;

namespace SmartishTable.Samples.Shared
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public byte Dependents { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        public decimal? Income { get; set; }
    }
}
