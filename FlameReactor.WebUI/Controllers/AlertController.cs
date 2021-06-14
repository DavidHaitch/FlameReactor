using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlameReactor.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : Controller
    {
        private AppState _appState;
        private IConfiguration _configuration;
        public AlertController(AppState appState, IConfiguration configuration)
        {
            _appState = appState;
            _configuration = configuration;
        }

        [HttpGet()]
        public string GetAlert()
        {
            return _appState.AlertMessage;
        }

        [HttpPost()]
        public ActionResult SetAlert([FromBody] string alert, string apiKey)
        {
            if (apiKey != _configuration["APIKey"]) return Unauthorized();
            _appState.AlertMessage = alert;
            return Ok();
        }
    }
}
