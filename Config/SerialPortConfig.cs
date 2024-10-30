using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToAsterixConverter.Config;

[JsonFileBasedConfiguration("serialport.json")]
public class SerialPortConfig : JsonFileBasedConfiguration
{
    public override void InitialDefaults()
    {
    }
    public string? Port { get; set; } = "COM1";
    public int BaudRate { get; set; } = 115200;
    public StopBits StopBits { get; set; } = StopBits.One;
    public Parity Parity { get; set; } = Parity.None;
    public int DataBits { get; set; } = 8;
}