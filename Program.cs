using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Linq;

namespace LoopMachineOsc
{
  class Program
  {
    private static SerialServer _serialServer;

    static void Main(string[] args)
    {
      const string configFilePath = "config.json";

      ConfigFile configFile = ConfigFile.Read(configFilePath);
      if (configFile == null)
      {
        configFile = new ConfigFile
        {
          SerialPortName = null,
          BaudRate = 9600
        };
        configFile.Save(configFilePath);
      }

      while (_serialServer == null)
      {
        string[] ports = SerialPort.GetPortNames();
        if (ports.Length < 1)
        {
          ISerialPort dummy = new DummyPort();
          _serialServer = new SerialServer(dummy);
          //Console.WriteLine("No Ports found, retrying in 1s");
          //Thread.Sleep(1000);
          continue;
        }

        string portName = null;
        if (!string.IsNullOrEmpty(configFile.SerialPortName))
        {
          if (ports.Contains(configFile.SerialPortName))
            portName = configFile.SerialPortName;
          else
          {
            Console.WriteLine($"Port {configFile.SerialPortName} not available, retrying in 1s");
            Thread.Sleep(1000);
            continue;
          }
        }
        else
        {
          portName = ports[0];
        }

        if (portName != null)
        {
          int baudRate = configFile.BaudRate < 1 ? 9600 : configFile.BaudRate;
          ISerialPort port = new SerialPortWrapper(portName, baudRate);
          //ReadTimeOut
          //WriteTimeOut
          _serialServer = new SerialServer(port);
        }
      }

      while (true)
      {
        try
        {
          Loop();
        }
        catch (Exception e)
        {
          Console.WriteLine($"ERROR: {e}");
        }
      }
    }

    private static void Loop()
    {
      SerialMessage msg;
      while (_serialServer.HasMessage())
      {
        msg = _serialServer.GetNextMessage();
        EvaluateMessage(msg);
        msg = null;
      }
    }

    private static void EvaluateMessage(SerialMessage msg)
    {
      Console.WriteLine($"Message received {msg.Type} {(int)msg.Value1} {(int)msg.Value2}");
      switch (msg.Type)
      {
        case MessageType.Ack:
          if (msg.Value1 == 0 && msg.Value2 == 0)
            _serialServer.Write(MessageType.Ack, (char)1, (char)0);
          break;
        case MessageType.ButtonStateChanged:
          UpdateButtonState((ButtonType)msg.Value1, msg.Value2 > 0);
          break;
        default:
          break;
      }
    }

    private static void UpdateButtonState(ButtonType button, bool newState)
    {

    }
  }

}
