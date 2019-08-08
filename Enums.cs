namespace LoopMachineOsc
{
    public enum ButtonType
    {
      RecPlay = 0,
      Track1 = 1,
      Track2 = 2,
      Track3 = 3,
      Track4 = 4
    }

    public enum MessageType
    {
      Ack = 0,
      Reset = 1,
      SetLed = 2,
      ButtonStateChanged = 3,
      Debug = 99
    }
}
