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
        while (_port.ReadChar() != 0xF0) {}
      }
      catch (TimeoutException)
      {
        return null;
      }

      type = (MessageType) _port.ReadChar();
      value1 = (char) _port.ReadChar();
      value2 = (char) _port.ReadChar();

      while (_port.ReadChar() != 0xF7) {}

      return new SerialMessage(type, value1, value2);
    }

    public void Write(SerialMessage msg)
    {
      char[] buffer = new []
      {
        (char)0xF0,
        (char)msg.Type,
        msg.Value1,
        msg.Value2,
        (char)0xF7
      };
      _port.Write(buffer, 0, buffer.Length);
    }
  }
}
