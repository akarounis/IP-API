using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NovibetProject.Services;
using System.Net;

namespace NovibetProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IpController : ControllerBase
    {
        private readonly IpDetailsService ipDetailsService_;
        //private readonly MemoryCache cache_;

        public IpController(IpDetailsService ipService)
        {
            ipDetailsService_ = ipService;
        }

        [HttpGet("{ipAddress}")]
        public async Task<IActionResult> GetIPDetails(string ipAddress)
        {
            try
            {
                var details = await ipDetailsService_.GetIpDetailsAsync(ipAddress);
                if (details == null) { return Ok("No address with this IP could be found"); }

                var json = JsonConvert.SerializeObject(details);

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetIpReport()
        {
            try
            {
                var details = await ipDetailsService_.GetReportAsync();
                if (details == null) { return Ok("Could not produce report due to inefficient data"); }

                var json = JsonConvert.SerializeObject(details);

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }    

}
