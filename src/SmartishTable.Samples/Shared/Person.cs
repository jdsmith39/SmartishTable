using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartishTable.Samples.Shared
{
    public class Person
    {
        public Person()
        {
            var rand = new Random();

            var value = rand.Next(1000);

            if (value % 4 < 3)
            {
                this.NestedObj = new Nested()
                {
                    IntProp = value < 50 ? null : value,
                    StringProp = value % 40 == 0 ? null : GetRandomString(value % 40),
                    DateTimeProp = value % 20 == 0 ? null : DateTime.Now.AddHours(-300).AddHours(value)
                };
                if (value % 2 == 0)
                {
                    NestedObj.NestedObj = new Nested()
                    {
                        IntProp = value > 850 ? null : value,
                        StringProp = value % 30 == 0 ? null : GetRandomString(value % 30),
                        DateTimeProp = value % 15 == 0 ? null : DateTime.UtcNow.AddHours(-300).AddHours(value)
                    };
                }
            }

            StringFields = new List<string>();
            StringFields.Add(Guid.NewGuid().ToString());
            StringFields.Add(Guid.NewGuid().ToString());
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

        public List<string> StringFields { get; set; }

        public string GetRandomString(int length)
        {
            var r = new Random();
            return new string(Enumerable.Range(0, length).Select(n => (Char)(r.Next(32, 127))).ToArray());
        }
    }

    public class Nested
    {
        public int? IntProp { get; set; }

        public string StringProp { get; set; }

        public DateTime? DateTimeProp { get; set; }

        public Nested NestedObj { get; set; }
    }
}
