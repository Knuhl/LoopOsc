using System;
using System.Collections.Generic;
using System.Linq;

namespace LoopMachineOsc
{
  public class MachineState
  {
    private class TrackState
    {
      public TrackType Track { get; }

      public bool IsEmpty { get; set; } = true;

      public TrackPlayState PlayState { get; set; } = TrackPlayState.Empty;
      public TrackPlayState ArmedPlayState { get; set; } = TrackPlayState.Empty;

      public TrackRecordState RecordState { get; set; } = TrackRecordState.Empty;
      public TrackRecordState ArmedRecordState { get; set; } = TrackRecordState.Empty;

      public TrackState(TrackType track) => Track = track;

      public LedColorType GetLedStatus(LoopMachineMode recPlay, ExecuteMode executeMode)
      {
        switch (recPlay)
        {
          case LoopMachineMode.Reset:
          case LoopMachineMode.Record:
            switch (executeMode)
            {
              case ExecuteMode.Arm:
                //REC ARM

                break;
              case ExecuteMode.Execute:
                //REC EXECUTE
                break;
            }
            break;
          case LoopMachineMode.Play:

            break;
        }
        return LedColorType.Off;
      }
    }

    private static Queue<MachineState> _stateHistory = new Queue<MachineState>(11);

    private readonly ICommunicationServer _communicationServer;

    private LoopMachineMode _mode;
    private ExecuteMode _execMode;

    private readonly Dictionary<TrackType, TrackState> _trackStates;
    private readonly Dictionary<ButtonType, DateTime> _buttonPressTime;

    public MachineState(ICommunicationServer communicationServer)
    {
      _communicationServer = communicationServer;
      _communicationServer.ButtonStateChanged += ButtonStateChanged;

      _mode = LoopMachineMode.Reset;
      _execMode = ExecuteMode.Arm;

      _buttonPressTime = new Dictionary<ButtonType, DateTime>();
      _trackStates = Enum.GetValues(typeof(TrackType)).Cast<TrackType>().ToDictionary(x => x, x => new TrackState(x));
    }

    public void Reset()
    {
      _communicationServer.WriteSerial(MessageType.Reset, (char)0, (char)0);

      _mode = LoopMachineMode.Reset;
      _execMode = ExecuteMode.Arm;

      foreach (KeyValuePair<TrackType, TrackState> track in _trackStates)
      {
        _trackStates[track.Key].IsEmpty = true;
        _trackStates[track.Key].PlayState = TrackPlayState.Empty;
        _trackStates[track.Key].RecordState = TrackRecordState.Empty;
        SetLed((ButtonType)track.Key, LedColorType.Off);
      }
      SetLed(ButtonType.ArmExecute, LedColorType.Green);
      SetLed(ButtonType.Mode, LedColorType.Red);

      _trackStates[TrackType.Track1].ArmedRecordState = TrackRecordState.Record;
      SetLed(ButtonType.Track1, LedColorType.Green);
    }

    private void SetLed(ButtonType ledButton, LedColorType color)
    {
      _communicationServer.WriteSerial(MessageType.SetLed, (char)(int)ledButton, (char)(int)color);
    }

    public void ButtonStateChanged(ButtonType button, bool state)
    {
      switch (button)
      {
          case ButtonType.ArmExecute:
            //only care for buttonDown
            if(!state) return;

            switch (_mode) //current Mode
            {
              case LoopMachineMode.Reset:
                Console.WriteLine("Executing First Record (now Record Mode)");
                _execMode = ExecuteMode.Execute;
                _mode = LoopMachineMode.Record;
                _trackStates[TrackType.Track1].RecordState = TrackRecordState.Record;
                _trackStates[TrackType.Track1].ArmedRecordState = TrackRecordState.Empty;
                _trackStates[TrackType.Track1].PlayState = TrackPlayState.Play;
                _trackStates[TrackType.Track1].IsEmpty = false;
                SetLed(ButtonType.ArmExecute, LedColorType.Red);
                SetLed(ButtonType.Track1, LedColorType.Red);
                break;
              case LoopMachineMode.Record:
                switch (_execMode)
                {
                  case ExecuteMode.Arm:
                    //REC from ARM to EXECUTE
                    Console.WriteLine("Switching from ARM to EXECUTE (Rec Mode)");
                    _execMode = ExecuteMode.Execute;

                    foreach (TrackType track in _trackStates.Keys)
                    {
                      if (_trackStates[track].ArmedRecordState != TrackRecordState.Empty)
                      {
                        _trackStates[track].RecordState = _trackStates[track].ArmedRecordState;
                        _trackStates[track].ArmedRecordState = TrackRecordState.Empty;
                        _trackStates[track].PlayState = TrackPlayState.Play;
                        _trackStates[track].ArmedPlayState = TrackPlayState.Empty;
                        _trackStates[track].IsEmpty = false;
                        Console.WriteLine($"Recording on {track} ({_trackStates[track].RecordState})");
                        SetLed((ButtonType)track, LedColorType.Red);
                      }
                      else
                        SetLed((ButtonType)track, LedColorType.Off);
                    }

                    SetLed(ButtonType.ArmExecute, LedColorType.Red);
                    break;
                  case ExecuteMode.Execute:
                    //REC from EXECUTE to ARM
                    _execMode = ExecuteMode.Arm;
                    Console.WriteLine("Switching from EXECUTE to ARM (Rec Mode)");

                    foreach (TrackType track in _trackStates.Keys)
                    {
                      switch (_trackStates[track].RecordState)
                      {
                        case TrackRecordState.Record:
                        case TrackRecordState.Insert:
                        case TrackRecordState.Overdub:
                          _trackStates[track].RecordState = TrackRecordState.Empty;
                          _trackStates[track].ArmedRecordState = TrackRecordState.Empty;
                          break;
                      }
                      SetLed((ButtonType)track, LedColorType.Off);
                    }

                    SetLed(ButtonType.ArmExecute, LedColorType.Green);
                    break;
                }
                break;
              case LoopMachineMode.Play:
                switch (_execMode)
                {
                  case ExecuteMode.Arm:
                    //PLAY from ARM to EXECUTE
                    _execMode = ExecuteMode.Execute;
                    Console.WriteLine("Switching from ARM to EXECUTE (Play Mode)");

                    foreach (TrackType track in _trackStates.Keys)
                    {
                      if (_trackStates[track].ArmedPlayState != TrackPlayState.Empty)
                      {
                        _trackStates[track].PlayState = _trackStates[track].ArmedPlayState;
                        _trackStates[track].ArmedPlayState = TrackPlayState.Empty;

                        switch (_trackStates[track].PlayState)
                        {
                          case TrackPlayState.Play:
                            SetLed((ButtonType)track, LedColorType.Green);
                            break;
                          case TrackPlayState.Muted:
                          case TrackPlayState.MutedPaused:
                            SetLed((ButtonType)track, LedColorType.Red);
                            break;
                        }
                        Console.WriteLine($"{track} PlayState {_trackStates[track].PlayState}");
                      }
                    }

                    SetLed(ButtonType.ArmExecute, LedColorType.Red);
                    break;
                  case ExecuteMode.Execute:
                    //PLAY from EXECUTE to ARM
                    _execMode = ExecuteMode.Arm;
                    Console.WriteLine("Switching from EXECUTE to ARM (Play Mode)");

                    SetLed(ButtonType.ArmExecute, LedColorType.Green);
                    break;
                }

                break;
            }
            break;
          case ButtonType.Stop:
            //only care for buttonDown
            if(!state) return;

            //only stop during EXECUTE PLAY mode
            if (_mode != LoopMachineMode.Play || _execMode != ExecuteMode.Execute)
              return;

            //TODO: Execute/Arm?

            foreach (KeyValuePair<TrackType, TrackState> track in _trackStates)
            {
              if(!track.Value.IsEmpty)
              {
                _trackStates[track.Key].PlayState = TrackPlayState.MutedPaused;
                _trackStates[track.Key].ArmedPlayState = TrackPlayState.Empty;
                SetLed((ButtonType)track.Key, LedColorType.Red);
              }
            }
            break;
          case ButtonType.Mode:
            //only care for buttonDown
            if(!state) return;

            // stay in reset-mode until EXECUTE is pressed
            if (_mode == LoopMachineMode.Reset)
              return;

            switch (_mode)
            {
              case LoopMachineMode.Record:
                //from REC to PLAY
                _mode = LoopMachineMode.Play;

                Console.WriteLine("Switching from REC to PLAY");

                foreach (TrackType track in _trackStates.Keys)
                {
                  //stop recording
                  if (_trackStates[track].RecordState != TrackRecordState.Empty)
                  {
                    _trackStates[track].RecordState = TrackRecordState.Empty;
                  }

                  //update LED to play-status
                  switch (_trackStates[track].PlayState)
                  {
                    case TrackPlayState.Empty:
                      SetLed((ButtonType)track, LedColorType.Off);
                      break;
                    case TrackPlayState.Play:
                      SetLed((ButtonType)track, LedColorType.Green);
                      break;
                    case TrackPlayState.Muted:
                    case TrackPlayState.MutedPaused:
                      SetLed((ButtonType)track, LedColorType.Red);
                      break;
                  }
                }

                SetLed(ButtonType.Mode, LedColorType.Green);
                break;
              case LoopMachineMode.Play:
                //from PLAY to RECORD
                _mode = LoopMachineMode.Record;

                Console.WriteLine("Switching from PLAY to REC");

                //all LED off - nothing armed / executing
                foreach (TrackType track in _trackStates.Keys)
                {
                  _trackStates[track].ArmedRecordState = TrackRecordState.Empty;
                  SetLed((ButtonType)track, LedColorType.Off);
                }

                SetLed(ButtonType.Mode, LedColorType.Red);
                break;
            }
            break;
          case ButtonType.UndoClear:
            if (state)
              _buttonPressTime[button] = DateTime.Now;
            else
            {
              if (!_buttonPressTime.ContainsKey(button)) return;
              TimeSpan diff = DateTime.Now - _buttonPressTime[button];
              if (diff.TotalMilliseconds > 1000)
              {
                // CLEAR
                Reset();
              }
              else
              {
                // UNDO
              }
            }
            break;
          case ButtonType.Track1:
            ToggleTrackState(_trackStates[TrackType.Track1], state);
            break;
          case ButtonType.Track2:
            ToggleTrackState(_trackStates[TrackType.Track2], state);
            break;
          case ButtonType.Track3:
            ToggleTrackState(_trackStates[TrackType.Track3], state);
            break;
          case ButtonType.Track4:
            ToggleTrackState(_trackStates[TrackType.Track4], state);
            break;
      }
    }

    private void ToggleTrackState(TrackState track, bool state)
    {
      if (!state) return;

      Console.WriteLine($"Toggling {track.Track} State in {_mode} {_execMode} Mode");
      Console.WriteLine($"Play {track.PlayState} (Armed: {track.ArmedPlayState}) Rec {track.RecordState} (Armed: {track.ArmedRecordState})");
      switch (_mode)
      {
        case LoopMachineMode.Reset: return;
        case LoopMachineMode.Record:
          LedColorType recColor = LedColorType.Off;
          switch (_execMode)
          {
            case ExecuteMode.Arm:
              //in ARM RECORD mode
              switch (track.ArmedRecordState)
              {
                case TrackRecordState.Empty:
                  //TODO: can Insert be skipped by directly going into OVERDUB on empty track?
                  track.ArmedRecordState = track.IsEmpty ? TrackRecordState.Insert : TrackRecordState.Overdub;
                  recColor = LedColorType.Green;
                  break;
                case TrackRecordState.Overdub:
                  if (!track.IsEmpty)
                  {
                    if (track.Track == TrackType.Track1)
                      track.ArmedRecordState = TrackRecordState.Record;
                    else
                      track.ArmedRecordState = TrackRecordState.Insert;
                    recColor = LedColorType.Green;
                  }
                  else
                  {
                    track.ArmedRecordState = TrackRecordState.Empty;
                    recColor = LedColorType.Off;
                  }
                  break;
                case TrackRecordState.Insert:
                case TrackRecordState.Record:
                  track.ArmedRecordState = TrackRecordState.Empty;
                  recColor = LedColorType.Off;
                  break;
              }
              break;
            case ExecuteMode.Execute:
              //in EXECUTE RECORD mode
              switch (track.RecordState)
              {
                case TrackRecordState.Empty:
                  //TODO: can Insert be skipped by directly going into OVERDUB on empty track?
                  track.RecordState = track.IsEmpty ? TrackRecordState.Insert : TrackRecordState.Overdub;
                  track.IsEmpty = false;
                  track.PlayState = TrackPlayState.Play;
                  track.ArmedPlayState = TrackPlayState.Empty;
                  recColor = LedColorType.Red;
                  break;
                case TrackRecordState.Insert:
                  //track.RecordState = TrackRecordState.Overdub;
                  //recColor = LedColorType.Red;
                  //break;
                case TrackRecordState.Overdub:
                case TrackRecordState.Record:
                  track.RecordState = TrackRecordState.Empty;
                  recColor = LedColorType.Off;
                  break;
              }
              break;
          }
          SetLed((ButtonType)track.Track, recColor);
          break;
        case LoopMachineMode.Play:

          //ignore if track is empty
          if (track.IsEmpty || track.PlayState == TrackPlayState.Empty)
            return;

          LedColorType playColor = LedColorType.Off;
          switch (_execMode)
          {
            case ExecuteMode.Arm:
              //in ARM PLAY mode
              switch (track.ArmedPlayState)
              {
                case TrackPlayState.Empty:
                  switch (track.PlayState)
                  {
                    case TrackPlayState.Play:
                      track.ArmedPlayState = TrackPlayState.Muted;
                      playColor = LedColorType.Red;
                      break;
                    case TrackPlayState.MutedPaused:
                    case TrackPlayState.Muted:
                      track.ArmedPlayState = TrackPlayState.Play;
                      playColor = LedColorType.Green;
                      break;
                  }
                  break;
                case TrackPlayState.Muted:
                case TrackPlayState.MutedPaused:
                  track.ArmedPlayState = TrackPlayState.Empty;
                  //was armed for mute, now armed for nothing again -> play = green
                  playColor = LedColorType.Green;
                  break;
                case TrackPlayState.Play:
                  track.ArmedPlayState = TrackPlayState.Empty;
                  //was armed for play, now armed for nothing again -> muted = red
                  playColor = LedColorType.Red;
                  break;
              }
              break;
            case ExecuteMode.Execute:
              //in EXECUTE PLAY mode
              switch (track.PlayState)
              {
                case TrackPlayState.Empty:
                  break;
                case TrackPlayState.Muted:
                case TrackPlayState.MutedPaused:
                  track.PlayState = TrackPlayState.Play;
                  playColor = LedColorType.Green;
                  break;
                case TrackPlayState.Play:
                  track.PlayState = TrackPlayState.Muted;
                  playColor = LedColorType.Red;
                  break;
              }
              break;
          }

          SetLed((ButtonType)track.Track, playColor);
          break;
      }

      Console.WriteLine($"-> Play {track.PlayState} (Armed: {track.ArmedPlayState}) Rec {track.RecordState} (Armed: {track.ArmedRecordState})");
    }
  }
}
