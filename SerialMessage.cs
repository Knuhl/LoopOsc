namespace LoopMachineOsc
{
  public class SerialMessage
  {
    public readonly MessageType Type;
    public readonly char Value1;
    public readonly char Value2;

    public SerialMessage(MessageType type, char v1, char v2)
    {
      Type = type;
      Value1 = v1;
      Value2 = v2;
    }
  }
}
