using System;
using System.IO;
using Newtonsoft.Json;

namespace LoopMachineOsc
{
  public class ConfigFile
  {
    public string SerialPortName { get; set; }
    public int BaudRate { get; set; }

    public static ConfigFile Read(string path)
    {
      if (!File.Exists(path)) return null;
      try
      {
        return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path));
      }
      catch (Exception)
      {
        throw;
        //return null;
      }
    }

    public void Save(string path)
    {
      string json = JsonConvert.SerializeObject(this, Formatting.Indented);
      if (File.Exists(path))
        File.Delete(path);
      File.WriteAllText(path, json);
    }
  }
}
