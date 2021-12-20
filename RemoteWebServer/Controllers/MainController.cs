using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RemoteWebServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        private readonly ILogger _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            _logger.LogInformation("GET from: {0} | Querystring: {1}", this.Request.Host, this.Request.QueryString);
            return "This is a GET request, use POST instead.";
        }

        [HttpPost]
        public string Post()
        {
            var traceID = Request.HttpContext.TraceIdentifier;
                   
            _logger.LogInformation("{0}: GET | Session:Request: {1}", DateTime.Now.ToLongTimeString(), traceID);
            bool isComplete = true;
            int loopCount = 0;

            while (isComplete)
            {
                System.Threading.Thread.Sleep(1000);
                loopCount++;
                if (loopCount >= 10)    // seconds to wait
                    isComplete = false;

                System.Diagnostics.Debug.Print("Worker iteration {0}", loopCount);
            }

            _logger.LogInformation("{0}: Session:Request: {1} Completed", DateTime.Now.ToLongTimeString(), traceID);
            return "Server work completed in " + loopCount + " seconds.";
        }
    }
}
