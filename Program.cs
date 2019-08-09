using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using Qml.Net;
using Qml.Net.Runtimes;

namespace LoopMachineOsc
{
  class Program
  {
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

      SerialServer serialServer = null;
      while (serialServer == null)
      {
        string[] ports = SerialPort.GetPortNames();
        if (ports.Length < 1)
        {
          serialServer = new SerialServer(DummyPort.Instance);
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
          Console.WriteLine("Connecting to " + portName);
          int baudRate = configFile.BaudRate < 1 ? 9600 : configFile.BaudRate;
          ISerialPort port = new SerialPortWrapper(portName, baudRate);
          //ReadTimeOut
          //WriteTimeOut
          serialServer = new SerialServer(port);
        }
      }

      CommunicationServer communicationServer = new CommunicationServer(serialServer);
      communicationServer.RunAsync();

      RuntimeManager.DiscoverOrDownloadSuitableQtRuntime();

      using (var application = new QGuiApplication(args))
      {
        MainViewModel.App = application;
        QQmlApplicationEngine.ActivateMVVMBehavior();
        using (var qmlEngine = new QQmlApplicationEngine())
        {
          Qml.Net.Qml.RegisterType<MainViewModel>("LoopMachineOsc");

          qmlEngine.Load("Main.qml");

          var result = application.Exec();
        }
      }

      communicationServer.Stop();
    }
  }

}
