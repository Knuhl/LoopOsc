using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Qml.Net;

namespace LoopMachineOsc
{
  public class MainViewModel
  {
    public static QCoreApplication App;

    public class ButtonViewModel
    {
      public ButtonType ButtonType { get; set; }

      public string Title => ButtonType.ToString();

      private string _ledColor = "black";
      [NotifySignal]
      public string LedColor
      {
        get => _ledColor;
        set
        {
          if (_ledColor == value) return;
          _ledColor = value;
          this.ActivateSignal("ledColorChanged");
        }
      }
    }

    public List<ButtonViewModel> Buttons { get; }

    public MainViewModel()
    {
      Buttons = new List<ButtonViewModel>(Enum.GetValues(typeof(ButtonType)).Cast<ButtonType>().Select(e => new ButtonViewModel { ButtonType = e }));

      Thread updateLoopThread = new Thread(UpdateLoop);
      updateLoopThread.Start();
    }

    public void Press(ButtonType pressedButton)
    {
      Console.WriteLine($"{pressedButton} pressed");
      DummyPort.Instance.InQueue.Enqueue(new SerialMessage(MessageType.ButtonStateChanged, (char)pressedButton, (char)1));
      DummyPort.Instance.InQueue.Enqueue(new SerialMessage(MessageType.ButtonStateChanged, (char)pressedButton, (char)0));
    }

    public void UpdateLoop()
    {
      SerialMessage msg;
      while(true)
      {
        msg = DummyPort.Instance.OutQueue.Count > 0 ? DummyPort.Instance.OutQueue.Dequeue() : null;
        if (msg != null)
        {
          Console.WriteLine($"Board got Message {msg.Type} {(int)msg.Value1} {(int)msg.Value2}");
          SerialMessage cur = msg;
          App.Dispatch(new Action(() => EvaluateMessage(cur)));
          msg = null;
        }
        Thread.Sleep(100);
      }
    }

    private void EvaluateMessage(SerialMessage msg)
    {
      switch (msg.Type)
      {
        case MessageType.SetLed:
          int led = (int)msg.Value1;
          int color = (int)msg.Value2;
          var btn = Buttons.First(x => x.ButtonType == (ButtonType)led);
          string colorString;
          switch (color)
          {
            case 1:
              colorString = "red";
              break;
            case 2:
              colorString = "green";
              break;
            default:
              colorString = "black";
              break;
          }
          Console.WriteLine($"Setting LED of {(ButtonType)led} to {colorString}");
          btn.LedColor = colorString;
          break;
      }
    }
  }
}
