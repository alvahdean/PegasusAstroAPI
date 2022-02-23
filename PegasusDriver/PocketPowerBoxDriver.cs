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
            UpdateInterval = TimeSpan.FromSeconds(5);
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


        protected override void OnConnect()
        {
            bool wasSuccessful;
            Console.WriteLine("Initializing connection");
            State.IsConnected = IsConnected;
            State.CommPort = CommPort;

            EnsureConnected();
            Console.WriteLine("Setting boot power state to on for all 12V outputs");
            var response = SendCommand($"P1000");
            wasSuccessful = response == "PPB_OK";

            if (!wasSuccessful)
            {
                Console.WriteLine($"Failed to set boot power state. Response={response}");
                Console.WriteLine($"Disconnecting");
                Disconnect();
                return;
            }

            State.FirmwareVersion = GetFirmwareVersion();
            UpdateStatus();
        }

        protected override void OnDisconnect()
        {
            State.IsConnected = false;
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
                State.Error = ex;
            }

            return false;
        }
 
        public void UpdateStatus()
        { 
                //Receive: PPB:12.2:0.5.22.2:45:17.2:1:1:120:130:1
                //             Volt  :Amp:Temp:Hum:DPt :Pwr :DSLR :Dew1:Dew2:AutoDew
                //Receive: PPB:12.2:0.5  :22.2:45 :17.2:1   :1    :120 :130 :1
                var response = SendCommand($"PA");
                Console.WriteLine($"Status: {response}");

                var values = response.Split(':');
                State.Voltage = double.Parse(values[0]);
                State.Current = double.Parse(values[1]);
                State.Temperature = double.Parse(values[2]);
                State.Humidity = double.Parse(values[3]);
                State.DewPoint = double.Parse(values[4]);
                State.QuadPortEnabled = bool.Parse(values[5]);
                State.DslrEnabled = bool.Parse(values[6]);
                State.DewHeaterPowerA = Math.Round(int.Parse(values[7]) / 255d, 1);
                State.DewHeaterPowerB = Math.Round(int.Parse(values[8]) / 255d, 1);
                State.AutoDewEnabled = bool.Parse(values[9]);
                State.LastUpdate = DateTime.Now;
        }

        public void Reboot()
        {
            State.Error = null;
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
