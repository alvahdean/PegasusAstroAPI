using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace PegasusDriver
{
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
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
        }

        public bool IsConnected => _connection?.IsOpen ?? false;
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

            if(!GetPorts().Contains(CommPort))
            {
                return;
            }

            _connection = new SerialPort(CommPort, BaudRate, Parity, DataBits, StopBits);
            _connection.Open();
            OnConnect();
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                OnDisconnect();
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

            _connection.WriteLine(command);

            var response = _connection.ReadLine().Trim();

            return response;
        }
        public bool? SendBoolCommand(string command)
        {
            var text = SendCommand(command);

            if(!bool.TryParse(text,out bool result))
            {
                return null;
            }

            return result;
        }

        public int? SendIntegerCommand(string command)
        {
            var text = SendCommand(command);

            if (!int.TryParse(text, out int result))
            {
                return null;
            }

            return result;
        }

        public double? SendDoubleCommand(string command)
        {
            var text = SendCommand(command);

            if (!double.TryParse(text, out double result))
            {
                return null;
            }

            return result;
        }

        public void SendNullCommand(string command)
        {
            if (!IsConnected)
            {
                throw new Exception("Cannot send command: Not connected");
            }

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
                    if(_connection?.IsOpen??false)
                    {
                        _connection?.Close();
                    }

                    _connection?.Dispose();
                }

                disposedValue = true;
            }
        }

        public IList<string> GetPorts() => SerialPort.GetPortNames().ToList();

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
