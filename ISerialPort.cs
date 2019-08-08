namespace LoopMachineOsc
{
  public interface ISerialPort
  {
    SerialMessage Read();
    void Write(SerialMessage msg);
  }
}
