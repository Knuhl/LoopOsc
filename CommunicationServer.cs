using System;
using System.Threading;

namespace LoopMachineOsc
{
  public class CommunicationServer : ICommunicationServer
  {
    private readonly SerialServer _serialServer;
    private bool _running;

    public CommunicationServer(SerialServer server)
    {
      _serialServer = server;
      // TODO OSC-Server
    }

    private void EvaluateMessage(SerialMessage msg)
    {
      Console.WriteLine($"Received Message {msg.Type} {(int)msg.Value1} {(int)msg.Value2} from BOARD");
      switch (msg.Type)
      {
        case MessageType.ButtonStateChanged:
          OnButtonStateChanged((ButtonType)(int)msg.Value1, msg.Value2 > 0);
          break;
      }
    }

    public event ButtonStateChangedEventHandler ButtonStateChanged;

    private void OnButtonStateChanged(ButtonType button, bool state)
    {
      ButtonStateChanged?.Invoke(button, state);
    }

    public void WriteSerial(SerialMessage msg)
      => _serialServer.WriteSerial(msg);

    public void WriteSerial(MessageType type, char value1, char value2)
      => _serialServer.WriteSerial(type, value1, value2);

    public void SendOsc(string osc)
    {
      System.Console.WriteLine("OSC: " + osc);
    }

    private void Loop()
    {
      SerialMessage msg;
      while (_serialServer.HasMessage())
      {
        msg = _serialServer.GetNextMessage();
        EvaluateMessage(msg);
        msg = null;
      }
    }

    public void Run()
    {
      _running = true;
      while (_running)
      {
        Loop();
      }
    }

    public void RunAsync()
    {
      Thread t = new Thread(Run);
      t.Start();
    }

    public void Stop() => _running = false;
  }

  public interface ICommunicationServer : ISerialWriter
  {
    void SendOsc(string osc);

    event ButtonStateChangedEventHandler ButtonStateChanged;
  }

  public delegate void ButtonStateChangedEventHandler(ButtonType button, bool newState);
}
