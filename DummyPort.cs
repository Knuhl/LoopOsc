using System;
using System.Collections.Generic;

namespace LoopMachineOsc
{
  public class DummyPort : ISerialPort
  {
    private static DummyPort _instance;
    public static DummyPort Instance => _instance ?? (_instance = new DummyPort());

    public Queue<SerialMessage> InQueue { get; } = new Queue<SerialMessage>();
    public Queue<SerialMessage> OutQueue { get; } = new Queue<SerialMessage>();

    private DummyPort()
    {
      Console.WriteLine("Dummy Port created");
    }

    public SerialMessage Read()
    {
      if (InQueue.Count > 0)
        return InQueue.Dequeue();
      return null;
    }

    public void Write(SerialMessage msg)
    {
      Console.WriteLine($"WRITE {msg.Type} {(int)msg.Value1} {(int)msg.Value2:X}");
      OutQueue.Enqueue(msg);
    }
  }
}
