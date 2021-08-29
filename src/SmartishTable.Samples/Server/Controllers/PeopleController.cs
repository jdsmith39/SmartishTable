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
        private List<Dictionary<string,object>> peopleDictionary;

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

        [HttpGet("AFewMore")]
        public async Task<IEnumerable<Person>> GetAFewMore()
        {
            await RetrievePeople();
            return people.Skip(10).Take(30);
        }

        [HttpGet("Some/{count}")]
        public async Task<IEnumerable<Person>> GetSome(int count)
        {
            await RetrievePeople();
            return people.Take(count);
        }

        [HttpGet("Dictionary/{count}/{name}")]
        public async Task<IEnumerable<Dictionary<string, object>>> GetDictionaryOfPeople(int count, string name)
        {
            await RetrievePeopleDictionary();
            var asdf = peopleDictionary.Take(count).OrderBy(o => o[name] as string);
            return asdf;
        }

        private async Task RetrievePeople()
        {
            if (people != null)
                return;
            var json = await System.IO.File.ReadAllTextAsync("data.json");
            people = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(json);
        }

        private async Task RetrievePeopleDictionary()
        {
            if (peopleDictionary != null)
                return;
            var json = await System.IO.File.ReadAllTextAsync("data.json");
            peopleDictionary = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string,object>>>(json);
        }
    }
}
