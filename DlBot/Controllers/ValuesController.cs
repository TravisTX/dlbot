using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DlBot.Services;

namespace DlBot.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly SlackService _slackService;

        public ValuesController(SlackService slackService)
        {
            _slackService = slackService;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _slackService.GetListOfUsers();
            var validUser = users.Any(x => x.name.ToLower() == "abc");
            var validUser2 = users.Any(x => x.name.ToLower() == "travis.collins");
            return Ok(new {validUser, validUser2});
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
