using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlameReactor.WebUI.Controllers
{
    public class EmberParams
    {
        public int GenomeTries { get; set; }
        public int LoopFrames { get; set; }
        public int Quality { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class EmberController : ControllerBase
    {
        private EmberService _emberService;
        private IConfiguration _configuration;

        public EmberController(EmberService emberService, IConfiguration configuration)
        {
            _emberService = emberService;
            _configuration = configuration;
        }

        [HttpGet("flameConfig")]
        public FlameConfig GetParams()
        {
            return _emberService.FlameConfig;
        }

        [HttpPost("flameConfig")]
        public ActionResult SetParams([FromBody] FlameConfig fc, string apiKey)
        {
            if (apiKey != _configuration["APIKey"]) return Unauthorized();
            _emberService.FlameConfig = fc;
            return Ok();
        }
    }
}
