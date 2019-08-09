using System;
using System.Collections.Generic;
using System.Linq;

namespace LoopMachineOsc
{
  public class MachineState
  {
    private class TrackState
    {
      public bool IsEmpty { get; set; } = true;

      public TrackPlayState PlayStateBeforeArm { get; private set; }

      private TrackPlayState _playState = TrackPlayState.Empty;
      public TrackPlayState PlayState
      {
        get => _playState;
        set
        {
          switch (value)
          {
            case TrackPlayState.MutedArmed:
            case TrackPlayState.PlayArmed:
              PlayStateBeforeArm = _playState;
              break;
          }
          _playState = value;
        }
      }

      public TrackRecordState RecordStateBeforeArm { get; private set; }

      private TrackRecordState _recordState = TrackRecordState.Empty;
      public TrackRecordState RecordState
      {
        get => _recordState;
        set
        {
          switch (value)
          {
            case TrackRecordState.RecordArmed:
            case TrackRecordState.InsertArmed:
            case TrackRecordState.OverdubArmed:
              RecordStateBeforeArm = _recordState;
              break;
          }
          _recordState = value;
        }
      }
    }

    private static Queue<MachineState> _stateHistory = new Queue<MachineState>(11);

    private LoopMachineMode _mode;
    private bool _executing;
    private bool _firstRecord;
    private readonly Dictionary<TrackType, TrackState> _trackStates;
    private readonly HashSet<TrackType> _selectedTracks;
    private readonly Dictionary<ButtonType, DateTime> _buttonPressTime;

    public MachineState()
    {
      _mode = LoopMachineMode.Reset;

      _buttonPressTime = new Dictionary<ButtonType, DateTime>();
      _trackStates = Enum.GetValues(typeof(TrackType)).Cast<TrackType>().ToDictionary(x => x, x => new TrackState());
      _selectedTracks = new HashSet<TrackType>();
    }

    public void Reset()
    {
      _mode = LoopMachineMode.Reset;
      _executing = false;
      _selectedTracks.Clear();
      _selectedTracks.Add(TrackType.Track1);

      foreach (KeyValuePair<TrackType, TrackState> track in _trackStates)
      {
        _trackStates[track.Key].IsEmpty = true;
        _trackStates[track.Key].PlayState = TrackPlayState.Empty;
        _trackStates[track.Key].RecordState = TrackRecordState.Empty;
      }
      _trackStates[TrackType.Track1].RecordState = TrackRecordState.RecordArmed;
    }

    //TODO: react to OSC-messages?

    public void ButtonStateChanged(ButtonType button, bool state)
    {
      switch (button)
      {
          case ButtonType.Execute:
            //only care for buttonDown
            if(!state) return;

            switch (_mode) //current Mode
            {
              case LoopMachineMode.Reset:
                _executing = true;
                _firstRecord = true;
                _selectedTracks.Add(TrackType.Track1);
                _trackStates[TrackType.Track1].RecordState = TrackRecordState.Record;
                _trackStates[TrackType.Track1].IsEmpty = false;
                break;
              case LoopMachineMode.Record:
                if (_firstRecord)
                {
                  _firstRecord = false;
                  _executing = true; //stays true
                  _trackStates[TrackType.Track1].RecordState = TrackRecordState.Overdub;
                }
                else
                {
                  if (!_executing)
                  {
                    _executing = true;
                    foreach (TrackType track in _trackStates.Keys)
                    {
                      switch (_trackStates[track].RecordState)
                      {
                        case TrackRecordState.RecordArmed:
                          _trackStates[track].RecordState = TrackRecordState.Record;
                          _trackStates[track].IsEmpty = false;
                          break;
                        case TrackRecordState.InsertArmed:
                          _trackStates[track].RecordState = TrackRecordState.Insert;
                          _trackStates[track].IsEmpty = false;
                          break;
                        case TrackRecordState.OverdubArmed:
                          _trackStates[track].RecordState = TrackRecordState.Overdub;
                          break;
                      }
                    }
                  }
                  else
                  {
                    _executing = false;
                    foreach (TrackType track in _trackStates.Keys)
                    {
                      switch (_trackStates[track].RecordState)
                      {
                        case TrackRecordState.Record:
                        case TrackRecordState.Insert:
                        case TrackRecordState.Overdub:
                          _trackStates[track].PlayState = TrackPlayState.Play;
                          break;
                      }
                    }
                  }
                }
                break;
              case LoopMachineMode.Play:
                _executing = false;

                foreach (TrackType track in _trackStates.Keys)
                {
                  switch (_trackStates[track].PlayState)
                  {
                    case TrackPlayState.MutedArmed:
                      _trackStates[track].PlayState = TrackPlayState.Muted;
                      break;
                    case TrackPlayState.PlayArmed:
                      _trackStates[track].PlayState = TrackPlayState.Play;
                      break;
                  }
                }
                break;
            }
            break;
          case ButtonType.Stop:
            //only care for buttonDown
            if(!state) return;

            if (_mode == LoopMachineMode.Record)
              return;

            foreach (KeyValuePair<TrackType, TrackState> track in _trackStates)
            {
              if(!track.Value.IsEmpty)
              {
                _trackStates[track.Key].PlayState = TrackPlayState.MutedPaused;
              }
            }
            break;
          case ButtonType.Mode:
            //only care for buttonDown
            if(!state) return;

            // stay in reset-mode until REC is pressed
            if (_mode == LoopMachineMode.Reset)
              return;

            // REC has to end before MODE-switch
            if (AnyTrackRecording())
              return;

            switch (_mode)
            {
              case LoopMachineMode.Record:
                //from record to play
                _mode = LoopMachineMode.Play;

                _selectedTracks.Clear();

                foreach (TrackType track in _trackStates.Keys)
                {
                  switch (_trackStates[track].PlayState)
                  {
                    case TrackPlayState.MutedArmed:
                    case TrackPlayState.Muted:
                    case TrackPlayState.MutedPaused:
                      _trackStates[track].PlayState = TrackPlayState.Play;
                      break;
                  }
                }
                break;
              case LoopMachineMode.Play:
                //from play to record
                _mode = LoopMachineMode.Record;

                _selectedTracks.Clear();
                break;
            }
            break;
          case ButtonType.Reset:
            //TODO: no use for reset buton, CLEAR
            //only care for buttonDown
            if(!state) return;

            Reset();
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
            ToggleTrackState(_trackStates[TrackType.Track1]);
            break;
          case ButtonType.Track2:
            ToggleTrackState(_trackStates[TrackType.Track2]);
            break;
          case ButtonType.Track3:
            ToggleTrackState(_trackStates[TrackType.Track3]);
            break;
          case ButtonType.Track4:
            ToggleTrackState(_trackStates[TrackType.Track4]);
            break;
      }
    }

    private void ToggleTrackState(TrackState track)
    {
      switch (_mode)
      {
        case LoopMachineMode.Reset: return;
        case LoopMachineMode.Record:
          switch (track.RecordState)
          {
              case TrackRecordState.Empty:
                track.RecordState = _executing
                                ? (track.IsEmpty ? TrackRecordState.Insert : TrackRecordState.Overdub)
                                : (track.IsEmpty ? TrackRecordState.InsertArmed : TrackRecordState.OverdubArmed);
                break;
              case TrackRecordState.RecordArmed:
                track.RecordState = track.RecordStateBeforeArm;
                ToggleTrackState(track);
                break;
              case TrackRecordState.Record:
                track.RecordState = TrackRecordState.Overdub;
                break;
              case TrackRecordState.InsertArmed:
                if (!track.IsEmpty)
                  track.RecordState = TrackRecordState.OverdubArmed;
                else
                  track.RecordState = track.RecordStateBeforeArm;
                break;
              case TrackRecordState.Insert:
                if (!track.IsEmpty)
                  track.RecordState = TrackRecordState.Overdub;
                break;
              case TrackRecordState.OverdubArmed:
                track.RecordState = TrackRecordState.InsertArmed;
                break;
              case TrackRecordState.Overdub:
                track.RecordState = TrackRecordState.Insert;
                break;
          }
          break;
        case LoopMachineMode.Play:

          switch (track.PlayState)
          {
            case TrackPlayState.Empty:
              break;
            case TrackPlayState.MutedArmed:
            case TrackPlayState.PlayArmed:
              track.PlayState = track.PlayStateBeforeArm;
              break;
            case TrackPlayState.Muted:
            case TrackPlayState.MutedPaused:
              track.PlayState = TrackPlayState.PlayArmed;
              break;
            case TrackPlayState.Play:
              track.PlayState = TrackPlayState.MutedArmed;
              break;
          }
          break;
      }
    }

    private bool AnyTrackRecording()
    {
      foreach (KeyValuePair<TrackType, TrackState> track in _trackStates)
      {
        switch (track.Value.RecordState)
        {
          case TrackRecordState.Record:
          case TrackRecordState.Insert:
          case TrackRecordState.Overdub:
            return true;
        }
      }
      return false;
    }
  }
}
