using System;
using nanoFramework.WebServer;
using Pegasus.Driver;
using nanoFramework.Json;

namespace Pegasus.API
{
    public class PocketPowerBoxController
    {
        private readonly static PocketPowerBoxDriver _driver;

        static PocketPowerBoxController()
        {
            _driver = new PocketPowerBoxDriver();
        }

        [Route("state")]
        [Method("GET")]
        public void GetState(WebServerEventArgs e)
        {
            ReturnState(e);
        }

        [Route("ports")]
        [Method("GET")]
        public void GetPorts(WebServerEventArgs e)
        {
            var portList = _driver.GetPorts();
            var jsonResult = JsonConvert.SerializeObject(portList);
            e.Context.Response.ContentType = "application/json";

            WebServer.OutPutStream(e.Context.Response, jsonResult);
        }

        //[Method("POST")]
        [Route("connect")]
        public void Connect(WebServerEventArgs e)
        {
            try
            {
                var port = GetArg(e, 1);

                if (string.IsNullOrEmpty(port))
                {
                    _driver.Connect();
                }
                else
                {
                    _driver.Connect(port);
                }

                ReturnState(e);
            }
            catch(Exception ex)
            {
                ReturnError(e,ex);
            }
        }

        [Method("POST")]
        [Route("disconnect")]
        public void Disconnect(WebServerEventArgs e)
        {
            try
            {
                _driver.Disconnect();
                ReturnState(e);
            }
            catch (Exception ex)
            {
                ReturnError(e, ex);
            }
        }

        [Method("POST")]
        [Route("led")]
        public void Led(WebServerEventArgs e)
        {
            try
            {
                var state = GetBoolArg(e, 1);

                _driver.SetIndicatorLed(state);
                ReturnState(e);
            }
            catch (Exception ex)
            {
                ReturnError(e, ex);
            }
        }

        [Method("POST")]
        [Route("dslr")]
        public void Dslr(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 1);

            _driver.SetDslrState(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("power")]
        public void PowerOutput(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 1);

            _driver.SetPowerState(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("autodew")]
        public void AutoDew(WebServerEventArgs e)
        {
            var state = GetBoolArg(e, 1);

            _driver.SetAutoDew(state);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("dewa")]
        public void DewA(WebServerEventArgs e)
        {
            var pct = GetDoubleArg(e, 1);

            _driver.SetDewA(pct);

            ReturnState(e);
        }

        [Method("POST")]
        [Route("dewb")]
        public void DewB(WebServerEventArgs e)
        {
            var pct = GetDoubleArg(e, 1);

            _driver.SetDewB(pct);

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

        private void ClearError()
        {
            SetError(string.Empty);
        }

        private void SetError(string message)
        {
            _driver.State.Error = message;
        }

        private void ReturnError(WebServerEventArgs e, Exception ex)
        {
            SetError(ex.Message);
            var state = _driver.State;

            var jsonResult = JsonConvert.SerializeObject(state);
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = 500;

            WebServer.OutPutStream(e.Context.Response, jsonResult);
        }

        private void ReturnState(WebServerEventArgs e)
        {
            var state = _driver.State;

            var jsonResult = JsonConvert.SerializeObject(state);
            e.Context.Response.ContentType = "application/json";

            WebServer.OutPutStream(e.Context.Response, jsonResult);
        }
    }
}
