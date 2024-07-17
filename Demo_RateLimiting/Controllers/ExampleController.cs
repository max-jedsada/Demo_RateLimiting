using AspNetCoreRateLimit;
using Demo_RateLimiting.Middleware.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Demo_RateLimiting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExampleController : Controller
    {
        private readonly ILogger<ExampleController> _logger;

        public ExampleController(ILogger<ExampleController> logger)
        {
            _logger = logger;
        }

        [HttpGet("get")]
        public IActionResult Get()
        {
            return Ok("");
        }


        [HttpGet("")]
        [LimitRequests(Limit = 5, Period = 10)]         // 5 request in 10 sec.
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllProducts() => Ok(new
        {
            status = true,
            message = ""
        });


        [HttpGet("getById")]
        [LimitRequests(Limit = 5, Period = 15)]         // 5 request in 15 sec.
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int? id) => Ok(new
        {
            status = true,
            message = "id : " + id.ToString() ?? "-"
        });

        [HttpGet("GetProductByGeneralRules")]           // use config GeneralRules in appsetting
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductByGeneralRules(int? id) => Ok(new
        {
            status = true,
            message = "id : " + id.ToString() ?? "-"
        });

    }
}