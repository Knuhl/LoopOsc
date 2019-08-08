using System;
using System.IO.Ports;

namespace LoopMachineOsc
{
  public class SerialPortWrapper : ISerialPort
  {
    private readonly SerialPort _port;

    public SerialPortWrapper(string portName, int baudRate)
    {
      _port = new SerialPort(portName, baudRate);
      _port.Open();
    }

    public SerialMessage Read()
    {
      MessageType type;
      char value1;
      char value2;

      try
      {
        while (_port.ReadByte() != 0xF0) {}
      }
      catch (TimeoutException)
      {
        return null;
      }

      type = (MessageType) _port.ReadByte();
      value1 = (char) _port.ReadByte();
      value2 = (char) _port.ReadByte();

      while (_port.ReadByte() != 0xF7) {}

      return new SerialMessage(type, value1, value2);
    }

    public void Write(SerialMessage msg)
    {
      byte[] buffer = new []
      {
        (byte)0xF0,
        (byte)msg.Type,
        (byte)msg.Value1,
        (byte)msg.Value2,
        (byte)0xF7
      };
      _port.Write(buffer, 0, buffer.Length);
    }
  }
}
