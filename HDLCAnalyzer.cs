using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using ToAsterixConverter.Config;


namespace ToAsterixConverter;

public class HDLCAnalyzer
{
    private SerialPort? _serialPort;

    private void EnsureSerialPort()
    {
        if (_serialPort is null || !_serialPort.IsOpen)
        {
            OpenPort();
        }
    }

    public event EventHandler<byte[]> DataFrameExtracted;
    private bool _isPortOpened;
    private bool _connecting = false;
    private SerialPortConfig _config = JsonFileBasedConfiguration.Read<SerialPortConfig>();
    private Crc16Ccitt _crc16Ccitt = new Crc16Ccitt(Crc16Ccitt.InitialCrcValue.Zeros);
    private byte[] _crc = new byte[2];
    private byte[] _dataBytesToSend;
    private List<byte> _dataBytes = new();
    private ReadingState _readingState = ReadingState.WaitToStart;

    public (bool isSuccess, string? errorMessage) OpenPort()
    {
        if (_connecting)
            return (false, "Already connecting");
        if (_isPortOpened)
            return (true, "Already opened");
        _connecting = true;
        try
        {
            if (_serialPort is not null)
            {
                _serialPort.Close();
                _serialPort = null;
            }

            _serialPort = new SerialPort(_config.Port, _config.BaudRate, _config.Parity,
                _config.DataBits, _config.StopBits);
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.Open();
            _isPortOpened = true;
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
        finally
        {
            _connecting = false;
        }
    }

    private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var buffer = new byte[_serialPort.BytesToRead];
        var readLength = _serialPort.Read(buffer, 0, buffer.Length);

        if (readLength > 0)
        {
            for (var i = 0; i < readLength; i++)
            {
                var data = (byte) (255 - buffer[i]);
                if (_readingState is ReadingState.StopReading)
                {
                    if (data is 0x7E)
                    {
                        _readingState = ReadingState.WaitToStart;
                        continue;
                    }
                }

                if (_readingState is ReadingState.WaitToStart && data is not 0x7E)
                {
                    _readingState = ReadingState.Reading;
                }

                if (_readingState is ReadingState.Reading)
                {

                    if (data != 0x7E)
                    {
                        _dataBytes.Add(data);
                    }
                    else
                    {
                        if (_dataBytes.Count > 10)
                        {
                            _crc[1] = _dataBytes[^1];
                            _crc[0] = _dataBytes[^2];
                            _readingState = ReadingState.CrcCheck;
                        }
                        else
                        {
                            _readingState = ReadingState.WaitToStart;
                        }
                    }
                }
                if (_readingState is ReadingState.CrcCheck)
                {
                    var dataArray = _dataBytes.ToArray();
                    var calculatedCrc = _crc16Ccitt.ComputeChecksumBytes(dataArray, false);
                    if (calculatedCrc.Length == _crc.Length && calculatedCrc[0] == _crc[0] && calculatedCrc[1] == _crc[1])
                    {
                        ////Remove 2 first bytes (Address & Control)
                        //_dataBytes.Remove(_dataBytes[1]);
                        //_dataBytes.Remove(_dataBytes[0]);
                        ////Send data on socket
                        //OnDataFrameExtracted(_dataBytes.ToArray());
                    }
                    //Remove 2 first bytes (Address & Control)
                    _dataBytes.RemoveAt(0);
                    _dataBytes.RemoveAt(0);
                    //Send data on socket
                    OnDataFrameExtracted(_dataBytes.ToArray());
                    _dataBytes.Clear();
                    _readingState = ReadingState.StopReading;
                }

                //if (_readingState is ReadingState.Reading)
                //{
                //    if (i + 2 <= readLength)
                //    {
                //        if (buffer[i + 2] != 0x7E)
                //        {
                //            dataBytes.Add(buffer[i]);
                //        }
                //        else if (buffer[i + 2] == 0x7E)
                //        {
                //            crc[0] = buffer[i + 1];
                //            crc[1] = buffer[i + 2];
                //            i += 2;
                //        }
                //    }
                //    else
                //    {
                //        _readingState = ReadingState.StopReading;
                //    }
                //}
            }
        }
    }

    public async Task AnalyzeHDLCPacketAsync()
    {

    }

    protected virtual void OnDataFrameExtracted(byte[] e)
    {
        DataFrameExtracted?.Invoke(this, e);
    }
}

public enum ReadingState
{
    WaitToStart,
    Reading,
    StopReading,
    CrcCheck,
}