using System;
using System.Linq;

namespace SmartishTable.Samples.Shared
{
    public class Person
    {
        public Person()
        {
            var rand = new Random();

            var value = rand.Next(1000);

            if (value % 2 == 0 || true)
            {
                this.NestedObj = new Nested()
                {
                    IntProp = value,
                    StringProp = GetRandomString(value % 30),
                    DateTimeProp = DateTime.Now.AddHours(-300).AddHours(value)
                };
            }
        }

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

        public Nested NestedObj { get; set; }

        public string GetRandomString(int length)
        {
            var r = new Random();
            return new String(Enumerable.Range(0, length).Select(n => (Char)(r.Next(32, 127))).ToArray());
        }
    }

    public class Nested
    {
        public int IntProp { get; set; }

        public string StringProp { get; set; }

        public DateTime? DateTimeProp { get; set; }

    }

    
}
