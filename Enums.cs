namespace LoopMachineOsc
{
    public enum ButtonType
    {
      ArmExecute = 0,
      Stop = 1,
      Mode = 2,
      UndoClear = 3,
      Track1 = 4,
      Track2 = 5,
      Track3 = 6,
      Track4 = 7
    }

    public enum MessageType
    {
      Reset = 0,
      SetLed = 1,
      ButtonStateChanged = 2,
      Debug = 99
    }

    public enum LoopMachineMode
    {
      Reset,
      Record,
      Play
    }

    public enum ExecuteMode
    {
      Arm,
      Execute
    }

    public enum TrackType
    {
      Track1 = 4,
      Track2 = 5,
      Track3 = 6,
      Track4 = 7
    }

    public enum TrackRecordState
    {
      Empty,
      Record,
      Insert,
      Overdub
    }

    public enum TrackPlayState
    {
      Empty,
      Muted,
      MutedPaused,
      Play
    }

    public enum LedColorType
    {
      Off = 0,
      Green = 1,
      Red = 2,
      BlinkGreen = 3,
      BlinkRed = 4,
      GreenBlinkRed = 5,
      RedBlinkGreen = 6
    }
}
