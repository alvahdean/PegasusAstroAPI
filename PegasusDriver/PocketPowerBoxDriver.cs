using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PegasusDriver
{
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    public class PocketPowerBoxDriver : SerialBase
    {
        private TimeSpan _updateInterval;
        private Timer _statusTimer;

        public PocketPowerBoxDriver()
        {
            State = new PocketPowerBoxState();
            DataBits = 8;
            StopBits = System.IO.Ports.StopBits.One;
            Parity = System.IO.Ports.Parity.None;
            BaudRate = 9600;
            CommPort = null;
            UpdateInterval = TimeSpan.Zero;
        }

        public PocketPowerBoxState State { get; }
  
        public TimeSpan UpdateInterval 
        {
            get => _updateInterval;
            set
            {
                if(_statusTimer!=null)
                {
                    _statusTimer.Dispose();
                    _statusTimer = null;
                }

                if (value > TimeSpan.Zero)
                {
                    _updateInterval = value;
                    _statusTimer = new Timer(OnStatusTimer, null, UpdateInterval, UpdateInterval);
                }
                else
                {
                    _updateInterval = TimeSpan.Zero;
                }
            }
        }

        private void OnStatusTimer(object state)
        {
            if (IsConnected)
            {
                UpdateStatus();
            }
        }

        protected void ClearState()
        {
            State.Voltage = double.NaN;
            State.Current = double.NaN;
            State.Temperature = double.NaN;
            State.Humidity = double.NaN;
            State.DewPoint = double.NaN;
            State.QuadPortEnabled = false;
            State.DslrEnabled = false;
            State.DewHeaterPowerA = -1;
            State.DewHeaterPowerB = -1;
            State.AutoDewEnabled = false;
            State.Error = null;
        }

        protected override void OnConnect()
        {
            Console.WriteLine("Initializing connection");
            State.IsConnected = IsConnected;
            State.CommPort = CommPort;

            EnsureConnected();
            if(!Ping())
            {
                State.Error = $"Device does not seem to be a Pocket Power Box";
                Disconnect();

                return;
            }

            State.FirmwareVersion = GetFirmwareVersion();
            UpdateStatus();
        }

        protected override void OnDisconnect()
        {
            State.IsConnected = false;
            ClearState();
            Console.WriteLine($"Disconnecting");
        }
        
        public bool Ping()
        {
            State.Error = null;
            try
            {
                var response = SendCommand($"P#");
                var result = response == "PPB_OK";
                return result;
            }
            catch(Exception ex)
            {
                State.Error = ex.Message;
            }

            return false;
        }

        public void UpdateStatus()
        {
            if(!IsConnected)
            {
                return;
            }

            var response = SendCommand($"PA");
            Console.WriteLine($"Status: {response}");

            State.LastUpdate = DateTime.Now;
            ClearState();
            
            var values = response.Split(':').ToList();

            if (values.Count !=12 || values[0] !="PPB")
            {
                State.Error = $"Status query failed: {response}";
            }

            int i = 1;

            State.Voltage = double.Parse(values[i++]);
            State.Current = Math.Round(double.Parse(values[i++])/65d,1);
            State.Temperature = double.Parse(values[i++]);
            State.Humidity = double.Parse(values[i++]);
            State.DewPoint = double.Parse(values[i++]);
            State.QuadPortEnabled = values[i++]=="1";
            State.DslrEnabled = values[i++] == "1";
            State.DewHeaterPowerA = Math.Round(int.Parse(values[i++]) / 255d, 1);
            State.DewHeaterPowerB = Math.Round(int.Parse(values[i++]) / 255d, 1);
            State.AutoDewEnabled = values[i++] == "1";
        }

        public void Reboot()
        {
            ClearState();
            Console.WriteLine("Rebooting...");
            SendNullCommand($"PF");
        }

        public string GetFirmwareVersion()
        {
            Console.WriteLine("Reading Firmware...");
            var response = SendCommand($"PV");
            return response;
        }

        public bool SetIndicatorLed(bool on)
        {
            Console.WriteLine($"Setting LED state: {on}");
            int state = on ? 1 : 0;
            var response = SendCommand($"PL:{state}");

            var result = response == $"PL:{state}";

            return result;
        }

        public bool SetAutoDew(bool on)
        {
            Console.WriteLine($"Setting AutoDew Function state: {on}");
            int state = on ? 1 : 0;
            var response = SendCommand($"PD:{state}");

            var result = response == $"PD:{state}";

            UpdateStatus();
            return result;
        }

        public bool SetPowerState(bool on)
        {
            Console.WriteLine($"Setting Power Out state to {on}");
            var state = on ? 1 : 0;
            var response = SendCommand($"P1:{state}");

            var result = response == $"P1:{state}";

            UpdateStatus();

            return result;
        }

        public bool SetDslrState(bool on)
        {

            Console.WriteLine($"Setting DSLR state to {on}");
            var state = on ? 1 : 0;
            var response = SendCommand($"P2:{state}");

            var result = response == $"P2:{state}";

            UpdateStatus();

            return result;
        }

        public bool SetDewA(double pct)
        {
            if (pct < 0d) { pct = 0; }
            if (pct > 1d) { pct = 1d; }

            pct = Math.Round(pct, 1);

            var pwm = (byte)(pct * 255);

            Console.WriteLine($"Setting Dew Heater A to {pct * 100}%");

            var response = SendCommand($"P3:{pwm}");

            var result = response == $"P3:{pwm}";

            UpdateStatus();

            return result;
        }

        public bool SetDewB(double pct)
        {
            if(pct<0d) { pct = 0; }
            if (pct >1d) { pct = 1d; }

            pct = Math.Round(pct, 1);

            var pwm = (byte)(pct * 255);

            Console.WriteLine($"Setting Dew Heater B to {pct*100}%");
            
            var response = SendCommand($"P4:{pwm}");

            var result = response == $"P4:{pwm}";

            UpdateStatus();

            return result;
        }
    }
}
