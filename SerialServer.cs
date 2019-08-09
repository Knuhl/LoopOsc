using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace LoopMachineOsc
{
  public class SerialServer
  {
    public static SerialServer Instance { get; private set; }

    private readonly ISerialPort _port;
    private bool _running;

    private readonly object _inLock = new object();
    private readonly Queue<SerialMessage> _inQueue = new Queue<SerialMessage>(10);

    private readonly object _outLock = new object();
    private readonly Queue<SerialMessage> _outQueue = new Queue<SerialMessage>(10);

    public SerialServer(ISerialPort port)
    {
      Instance = this;
      _port = port;

      Thread readThread = new Thread(Read);
      Thread writeThread = new Thread(Write);

      _running = true;
      readThread.Start();
      writeThread.Start();
    }

    public bool HasMessage() => _inQueue.Count > 0;

    public SerialMessage GetNextMessage()
    {
      lock (_inQueue)
        return HasMessage() ? _inQueue.Dequeue() : null;
    }

    public void Write(MessageType type, char value1, char value2)
      => Write(new SerialMessage(type, value1, value2));

    public void Write(SerialMessage msg)
    {
      lock (_outLock)
        _outQueue.Enqueue(msg);
    }

    private void Read()
    {
      while (_running)
      {
        try
        {
          SerialMessage msg = _port.Read();
          if (msg == null) continue;
          lock (_inLock)
            _inQueue.Enqueue(msg);
        }
        catch (Exception)
        {
          // ..
        }
      }
    }

    private void Write()
    {
      SerialMessage currentMessage = null;
      while (_running)
      {
        if (currentMessage == null)
        {
          if (_outQueue.Count < 1) continue;
          lock (_outLock)
            currentMessage = _outQueue.Dequeue();
        }

        bool success = false;
        while (!success)
        {
          try
          {
            _port.Write(currentMessage);
            currentMessage = null;
            success = true;
          }
          catch
          {
            // ..
          }
        }
      }
    }
  }
}
