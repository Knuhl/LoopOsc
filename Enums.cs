namespace LoopMachineOsc
{
    public enum ButtonType
    {
      Execute = 0,
      Stop = 1,
      Mode = 2,
      Reset = 3,
      UndoClear = 4,
      Track1 = 5,
      Track2 = 6,
      Track3 = 7,
      Track4 = 8
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

    public enum TrackType
    {
      Track1 = 5,
      Track2 = 6,
      Track3 = 7,
      Track4 = 8
    }

    public enum TrackRecordState
    {
      Empty,
      RecordArmed,
      Record,
      InsertArmed,
      Insert,
      OverdubArmed,
      Overdub
    }

    public enum TrackPlayState
    {
      Empty,
      MutedArmed,
      Muted,
      MutedPaused,
      PlayArmed,
      Play
    }
}
