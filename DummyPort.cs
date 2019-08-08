using System;
using System.Collections.Generic;

namespace LoopMachineOsc
{
  public class DummyPort : ISerialPort
  {
    private byte _ack;

    public Queue<SerialMessage> OutQueue { get; } = new Queue<SerialMessage>();

    public DummyPort()
    {
      _ack = 0;
      Console.WriteLine("Dummy Port created");
    }

    public SerialMessage Read()
    {
      if (_ack == 0)
      {
        _ack++;
        return new SerialMessage(MessageType.Ack, (char)0, (char)0);
      }
      else if (_ack == 2)
      {
        _ack++;
        return new SerialMessage(MessageType.Ack, (char)0, (char)1);
      }

      //Console.WriteLine("Reading");
      return null;
    }

    public void Write(SerialMessage msg)
    {
      Console.WriteLine($"WRITE {msg.Type} {(int)msg.Value1} {(int)msg.Value2:X}");
      if (_ack == 1 && msg.Type == MessageType.Ack && msg.Value1 == 1 && msg.Value2 == 0)
        _ack++;
    }
  }
}
