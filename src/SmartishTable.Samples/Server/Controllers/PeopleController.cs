using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartishTable.Samples.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartishTable.Samples.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger;

        private List<Person> people;

        public PeopleController(ILogger<PeopleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Person>> Get()
        {
            await RetrievePeople();
            return people;
        }

        [HttpGet("AFew")]
        public async Task<IEnumerable<Person>> GetAFew()
        {
            await RetrievePeople();
            return people.Take(20);
        }

        private async Task RetrievePeople()
        {
            if (people != null)
                return;
            var json = await System.IO.File.ReadAllTextAsync("data.json");
            people = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(json);
        }
    }
}
