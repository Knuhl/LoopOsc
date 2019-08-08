using System;
using System.Collections.Generic;

namespace LoopMachineOsc
{
  public class DummyPort : ISerialPort
  {
    public Queue<SerialMessage> OutQueue { get; } = new Queue<SerialMessage>();

    public DummyPort()
    {
      Console.WriteLine("Dummy Port created");
    }

    public SerialMessage Read()
    {
      return null;
    }

    public void Write(SerialMessage msg)
    {
      Console.WriteLine($"WRITE {msg.Type} {(int)msg.Value1} {(int)msg.Value2:X}");
    }
  }
}
