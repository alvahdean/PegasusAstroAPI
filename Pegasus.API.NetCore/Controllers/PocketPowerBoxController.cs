using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pegasus.Driver;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace PegasusApi.Controllers
{
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    [ApiController]
    [Route("[controller]")]
    public class PocketPowerBoxController : ControllerBase
    {
        private readonly ILogger<PocketPowerBoxController> _logger;
        private static PocketPowerBoxDriver _pocketPowerBoxDriver;

        public PocketPowerBoxController(ILogger<PocketPowerBoxController> logger, PocketPowerBoxDriver pocketPowerBoxDriver)
        {
            _logger = logger;
            _pocketPowerBoxDriver = pocketPowerBoxDriver;
        }

        [HttpGet]
        public PocketPowerBoxState Get()
        {
            _logger.LogDebug($"GET /");
            return _pocketPowerBoxDriver.State;
        }

        [HttpPost]
        [Route("connect")]
        public PocketPowerBoxState Connect()
        {
            _logger.LogDebug($"POST /connect");
            _pocketPowerBoxDriver.Connect();
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("connect/{commPort}")]
        public PocketPowerBoxState Connect(string commPort)
        {
            _logger.LogDebug($"POST /connect/{commPort}");
            if(commPort.StartsWith("tty"))
            {
                commPort = $"/dev/{commPort}";
            }

            _pocketPowerBoxDriver.Connect(commPort);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("disconnect")]
        public PocketPowerBoxState Disconnect()
        {
            _logger.LogDebug($"POST /disconnect");
            _pocketPowerBoxDriver.Disconnect();
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("led/{state}")]
        public PocketPowerBoxState Led(bool state)
        {
            _logger.LogDebug($"POST /led/{state}");
            _pocketPowerBoxDriver.SetIndicatorLed(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("dslr/{state}")]
        public PocketPowerBoxState Dslr(bool state)
        {
            _logger.LogDebug($"POST /dslr/{state}"); 
            _pocketPowerBoxDriver.SetDslrState(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("power/{state}")]
        public PocketPowerBoxState PowerOutput(bool state)
        {
            _logger.LogDebug($"POST /power/{state}");
            _pocketPowerBoxDriver.SetPowerState(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("autodew/{state}")]
        public PocketPowerBoxState AutoDew(bool state)
        {
            _logger.LogDebug($"POST /autodew/{state}");
            _pocketPowerBoxDriver.SetAutoDew(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("dewa/{pct}")]
        public PocketPowerBoxState DewA(double pct)
        {
            _logger.LogDebug($"POST /dewa/{pct}");
            _pocketPowerBoxDriver.SetDewA(pct);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("dewb/{pct}")]
        public PocketPowerBoxState DewB(double pct)
        {
            _logger.LogDebug($"POST /dewb/{pct}");
            _pocketPowerBoxDriver.SetDewB(pct);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }


        [HttpGet]
        [Route("ports")]
        public IEnumerable<string> Ports()
        {
            _logger.LogDebug($"GET /ports");
            var result =_pocketPowerBoxDriver.GetPorts();

            return result;
        }

    }
}
