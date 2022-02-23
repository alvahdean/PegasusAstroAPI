﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PegasusDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace PegasusAPI.Controllers
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
            return _pocketPowerBoxDriver.State;
        }

        [HttpPost]
        [Route("connect")]
        public PocketPowerBoxState Connect()
        {
            _pocketPowerBoxDriver.Connect();
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("connect/{commPort}")]
        public PocketPowerBoxState Connect(string commPort)
        {
            _pocketPowerBoxDriver.Connect(commPort);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("disconnect")]
        public PocketPowerBoxState Disconnect()
        {
            _pocketPowerBoxDriver.Disconnect();
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("led/{state}")]
        public PocketPowerBoxState Led(bool state)
        {
            _pocketPowerBoxDriver.SetIndicatorLed(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("dslr/{state}")]
        public PocketPowerBoxState Dslr(bool state)
        {
            _pocketPowerBoxDriver.SetDslrState(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("power/{state}")]
        public PocketPowerBoxState PowerOutput(bool state)
        {
            _pocketPowerBoxDriver.SetPowerState(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("autodew/{state}")]
        public PocketPowerBoxState AutoDew(bool state)
        {
            _pocketPowerBoxDriver.SetAutoDew(state);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("dewa/{pct}")]
        public PocketPowerBoxState DewA(double pct)
        {
            _pocketPowerBoxDriver.SetDewA(pct);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

        [HttpPost]
        [Route("dewb/{pct}")]
        public PocketPowerBoxState DewB(double pct)
        {
            _pocketPowerBoxDriver.SetDewB(pct);
            var result = _pocketPowerBoxDriver.State;

            return result;
        }

    }
}
