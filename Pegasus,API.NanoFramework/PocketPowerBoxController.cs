using System;
using nanoFramework.WebServer;
using Pegasus.Driver;
using nanoFramework.Json;

namespace Pegasus.API
{
    public class PocketPowerBoxController
    {
        private readonly static PocketPowerBoxDriver _pocketPowerBoxDriver;

        static PocketPowerBoxController()
        {
            _pocketPowerBoxDriver = new PocketPowerBoxDriver();
        }

        [Route("state")]
        [Method("GET")]
        public void GetState(WebServerEventArgs e)
        {
            ReturnState(e);
        }

        [Method("POST")]
        [Route("connect")]
        public void Connect(WebServerEventArgs e)
        {
            var port = GetArg(e, 0);

            if (string.IsNullOrEmpty(port))
            {
                _pocketPowerBoxDriver.Connect();
            }
            else
            {
                _pocketPowerBoxDriver.Connect(port);
            }

            ReturnState(e);
        }

        [Method("POST")]
        [Route("disconnect")]
        public void Disconnect(WebServerEventArgs e)
        {
            _pocketPowerBoxDriver.Disconnect();

            ReturnState(e);
        }

        [Method("POST")]
        [Route("led")]
        public void Led(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 0);

            _pocketPowerBoxDriver.SetIndicatorLed(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("dslr")]
        public void Dslr(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 0);

            _pocketPowerBoxDriver.SetDslrState(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("power")]
        public void PowerOutput(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 0);

            _pocketPowerBoxDriver.SetPowerState(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("autodew")]
        public void AutoDew(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 0);

            _pocketPowerBoxDriver.SetAutoDew(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("dewa")]
        public void DewA(WebServerEventArgs e)
        {
            var pct = GetDoubleArg(e, 0);

            _pocketPowerBoxDriver.SetDewA(pct);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("dewb")]
        public void DewB(WebServerEventArgs e)
        {
            var pct = GetDoubleArg(e, 0);

            _pocketPowerBoxDriver.SetDewB(pct);

            ReturnState(e);
        }

        private string[] GetArgs(WebServerEventArgs e)
        {
            var rawUrl = e.Context.Request.RawUrl.TrimStart('/');
            var args = rawUrl.Split('/');

            return args;
        }

        private string GetArg(WebServerEventArgs e,int index)
        {
            var result = string.Empty;

            var args = GetArgs(e);

            if(args.Length > index)
            {
                return args[index];
            }

            return result;
        }

        private int GetIntArg(WebServerEventArgs e, int index,int defaultValue=int.MinValue)
        {
            int result = defaultValue;

            var argString = GetArg(e,index);
            
            if(!string.IsNullOrEmpty(argString))
            {
                result = int.Parse(argString);
            }

            return result;
        }

        private double GetDoubleArg(WebServerEventArgs e, int index, double defaultValue = double.NaN)
        {
            double result = defaultValue;

            var argString = GetArg(e, index);

            if (!string.IsNullOrEmpty(argString))
            {
                result = double.Parse(argString);
            }

            return result;
        }

        private bool GetBoolArg(WebServerEventArgs e, int index)
        {
            bool result = false;

            var argString = GetArg(e, index).ToLower();

            switch (argString)
            {
                case "false":
                case "no":
                case "n":
                case "off":
                case "low":
                case "0":
                    result = false;
                    break;

                case "true":
                case "yes":
                case "y":
                case "on":
                case "high":
                case "1":
                    result = true;
                    break;

                default:
                    if (int.TryParse(argString, out int intValue))
                    {
                        result = intValue != 0;
                    }
                    else
                    {
                        throw new Exception($"Cannot convert '{argString}' to bool");
                    }
                    break;
            }

            return result;
        }

        private void ReturnState(WebServerEventArgs e)
        {
            var state = _pocketPowerBoxDriver.State;

            var jsonResult = JsonConvert.SerializeObject(state);
            e.Context.Response.ContentType = "application/json";

            WebServer.OutPutStream(e.Context.Response, jsonResult);
        }
    }
}
