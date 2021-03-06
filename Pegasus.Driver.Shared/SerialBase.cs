
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Versioning;

#if BUIID_FOR_ESP32
using nanoFramework.Hardware.Esp32;
#endif

namespace Pegasus.Driver
{
    public abstract class SerialBase : IDisposable
    {
        protected SerialPort _connection;
        private bool disposedValue;

        public SerialBase()
        {
            BaudRate = 9600;
            StopBits = StopBits.One;
            Parity = Parity.None;
            DataBits = 8;

#if BUIID_FOR_ESP32
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // COM2 in ESP32-WROVER-KIT mapped to free GPIO pins
            // mind to NOT USE pins shared with other devices, like serial flash and PSRAM
            // also it's MANDATORY to set pin function to the appropriate COM before instantiating it

            Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);
#endif
        }

        public bool IsConnected => _connection!=null && _connection.IsOpen;
        public string CommPort { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int DataBits { get; set; }

        public void Connect(string commPort)
        {
            CommPort = commPort;
            Connect();
        }

        public void Connect()
        {
            Disconnect();

            Debug.WriteLine($"CONNECT: {CommPort}");
            Debug.WriteLine($"\tBaudRate: {BaudRate}");
            Debug.WriteLine($"\tParity: {Parity}");
            Debug.WriteLine($"\tDataBits: {DataBits}");
            Debug.WriteLine($"\tStopBits: {StopBits}");

            _connection = new SerialPort(CommPort);
            _connection.BaudRate= BaudRate;
            _connection.Parity= Parity;
            _connection.DataBits= DataBits;
            _connection.StopBits= StopBits;

            _connection.Open();
            OnConnect();
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                OnDisconnect();
                Debug.WriteLine($"DISCONNECT: {CommPort}");
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public string SendCommand(string command)
        {
            if(!IsConnected)
            {
                return null;
            }

            Debug.WriteLine($"SEND: {command}");
            _connection.WriteLine(command);

            var response = _connection.ReadLine().Trim();
            Debug.WriteLine($"RECV: {response}");

            return response;
        }

        public void SendNullCommand(string command)
        {
            if (!IsConnected)
            {
                throw new Exception("Cannot send command: Not connected");
            }

            Debug.WriteLine($"SEND: {command}");

            _connection.WriteLine(command);
        }

        public void EnsureConnected()
        {
            if(!IsConnected)
            {
                throw new Exception("Serial device not connected");
            }
        }

        protected abstract void OnConnect();
        protected abstract void OnDisconnect();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(IsConnected)
                    {
                        _connection.Close();
                    }

                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }

                disposedValue = true;
            }
        }

        public string[] GetPorts() => SerialPort.GetPortNames();

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
