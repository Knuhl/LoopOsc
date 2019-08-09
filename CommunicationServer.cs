using System;
using System.Threading;

namespace LoopMachineOsc
{
  public class CommunicationServer
  {
    private readonly SerialServer _serialServer;
    private bool _running;
    private int _tempColor = 0;

    public CommunicationServer(SerialServer server)
    {
      _serialServer = server;
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

    private void EvaluateMessage(SerialMessage msg)
    {
      Console.WriteLine($"Received Message {msg.Type} {(int)msg.Value1} {(int)msg.Value2} from BOARD");
      switch (msg.Type)
      {
        case MessageType.ButtonStateChanged:
          ButtonStateChanged((ButtonType)(int)msg.Value1, msg.Value2 > 0);
          break;
      }
    }

    private void ButtonStateChanged(ButtonType button, bool state)
    {
      if (!state) return;
      int c = _tempColor++ % 3;
      _serialServer.Write(MessageType.SetLed, (char)(int)button, (char)c);
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
}
