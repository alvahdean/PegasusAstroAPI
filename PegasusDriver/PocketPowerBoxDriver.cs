using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PegasusDriver
{
    public class PocketPowerBoxDriver : SerialBase
    {
        private TimeSpan _updateInterval;
        private Timer _statusTimer;

        public PocketPowerBoxDriver()
        {
            DataBits = 8;
            StopBits = System.IO.Ports.StopBits.None;
            Parity = System.IO.Ports.Parity.None;
            BaudRate = 9600;
            CommPort = null;
            UpdateInterval = TimeSpan.FromSeconds(5);
        }

        public double Voltage { get; protected set; }
        public double Current { get; protected set; }
        public double Temperature { get; protected set; }
        public double Humidity { get; protected set; }
        public double DewPoint { get; protected set; }
        public bool QuadPortEnabled { get; protected set; }
        public bool DlsrEnabled { get; protected set; }
        public double DewHeaterPowerA { get; protected set; }
        public double DewHeaterPowerB { get; protected set; }
        public bool AutoDewEnabled { get; protected set; }
        public DateTime LastUpdate { get; protected set; }
        public string FirmwareVersion { get; protected set; }
        public bool IndicatorLedOn { get; protected set; }
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

            FirmwareVersion = GetFirmwareVersion();
            UpdateStatus();
        }

        protected override void OnDisconnect()
        {
            Console.WriteLine($"Disconnecting");
        }
        
        public bool Ping()
        {
            var response = SendCommand($"P#");
            var result = response == "PPB_OK";

            return result;
        }
 
        public void UpdateStatus()
        {
            //Receive: PPB:12.2:0.5.22.2:45:17.2:1:1:120:130:1
            //             Volt  :Amp:Temp:Hum:DPt :Pwr :DSLR :Dew1:Dew2:AutoDew
            //Receive: PPB:12.2:0.5  :22.2:45 :17.2:1   :1    :120 :130 :1
            var response = SendCommand($"PA");
            Console.WriteLine($"Status: {response}");

            var values = response.Split(':');
            Voltage = double.Parse(values[0]);
            Current = double.Parse(values[1]);
            Temperature = double.Parse(values[2]);
            Humidity = double.Parse(values[3]);
            DewPoint = double.Parse(values[4]);
            QuadPortEnabled = bool.Parse(values[5]);
            DlsrEnabled = bool.Parse(values[6]);
            DewHeaterPowerA = Math.Round(int.Parse(values[7])/255d,1);
            DewHeaterPowerB = Math.Round(int.Parse(values[8]) / 255d, 1);
            AutoDewEnabled = bool.Parse(values[9]);
            LastUpdate = DateTime.Now;
        }

        public void Reboot()
        {
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

        public bool? SetAutoDew(bool on)
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

        public bool SetDlsrState(bool on)
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
