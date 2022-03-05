using System;

namespace Pegasus.Driver
{
    public class PocketPowerBoxState
    {
        public bool IsConnected { get; set; }
        public string CommPort { get; set; }
        public double Voltage { get; set; }
        public double Current { get;  set; }
        public double Temperature { get;  set; }
        public double Humidity { get;  set; }
        public double DewPoint { get;  set; }
        public bool QuadPortEnabled { get;  set; }
        public bool DslrEnabled { get;  set; }
        public double DewHeaterPowerA { get;  set; }
        public double DewHeaterPowerB { get;  set; }
        public bool AutoDewEnabled { get;  set; }
        public bool IndicatorLedOn { get;  set; }
        public string FirmwareVersion { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Error { get; set; }
    }
}
