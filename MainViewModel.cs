using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Qml.Net;

namespace LoopMachineOsc
{
  public class MainViewModel : ViewModelBase
  {
    public static QCoreApplication App;

    public class ButtonViewModel : ViewModelBase
    {
      public ButtonType ButtonType { get; set; }
      public string Title => ButtonType.ToString();

      public bool RedLedVisible { get; set; } = false;
      public bool GreenLedVisible { get; set; } = false;
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
        case MessageType.Reset:
          foreach (var b in Buttons)
          {
            b.RedLedVisible = false;
            b.GreenLedVisible = false;
          }
          break;
        case MessageType.SetLed:
          int led = (int)msg.Value1;
          LedColorType color = (LedColorType)(int)msg.Value2;
          var btn = Buttons.First(x => x.ButtonType == (ButtonType)led);
          switch (color)
          {
            case LedColorType.Off:
              btn.RedLedVisible = false;
              btn.GreenLedVisible = false;
              break;
            case LedColorType.Green:
              btn.RedLedVisible = false;
              btn.GreenLedVisible = true;
              break;
            case LedColorType.Red:
              btn.RedLedVisible = true;
              btn.GreenLedVisible = false;
              break;
          }
          RaisePropertyChanged(nameof(Buttons));
          break;
      }
    }
  }

  public class ViewModelBase : INotifyPropertyChanged
  {
    protected bool SetProperty<T>(ref T storage, T value, Action onPropertyChanged = null, [CallerMemberName]string propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(storage, value))
        return false;
      storage = value;
      RaisePropertyChanged(propertyName);
      onPropertyChanged?.Invoke();
      return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

}
