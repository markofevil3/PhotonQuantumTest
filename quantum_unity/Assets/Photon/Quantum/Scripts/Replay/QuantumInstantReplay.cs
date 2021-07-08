using Photon.Deterministic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum{
  public enum QuantumInstantReplaySeekMode {
    Disabled,
    FromStartSnapshot,
    FromIntermediateSnapshots,
  }

  public class QuantumInstantReplay {

    public bool IsRunning { get; private set; }
    public float ReplayLength { get; set; }
    public float PlaybackSpeed { get; set; }
    public QuantumGame LiveGame => _liveGame;
    public QuantumGame ReplayGame => _replayRunner?.Game;

    public int StartFrame { get; private set; }
    public int EndFrame { get; private set; }

    public bool CanSeek => _rewindSnapshots?.Count > 0;

    public float NormalizedTime {
      get {
        if (!IsRunning) {
          throw new InvalidOperationException("Not running");
        }
        var currentFrame = _replayRunner.Game.Frames.Verified.Number;
        float result = (currentFrame - StartFrame) / (float)(EndFrame - StartFrame);
        return result;
      }
    }

    public event Action<QuantumGame> OnReplayStarted;
    public event Action<QuantumGame> OnReplayStopped;

    // We need this to fast forward the simulation and wait until is fully initialized.
    public const int InitalFramesToSimulation = 4;

    private QuantumGame _liveGame;
    private QuantumRunner _replayRunner;
    private DeterministicFrameRingBuffer _rewindSnapshots;
    private bool _loop;

    public QuantumInstantReplay(QuantumGame game) {
      _liveGame = game;
    }

    public void Shutdown() {
      if (IsRunning)
        StopInstantReplay();

      OnReplayStarted = null;
      OnReplayStopped = null;

      _liveGame = null;
    }

    public void Update() {
      if (IsRunning) {
        _replayRunner.Session.Update(Time.unscaledDeltaTime * PlaybackSpeed);

        // Stop the running instant replay.
        if (_replayRunner.Game.Frames.Verified != null &&
            _replayRunner.Game.Frames.Verified.Number >= EndFrame) {

          if (_loop) {
            SeekFrame(StartFrame);
          } else {
            StopInstantReplay();
          }
        }
      }
    }

    public void StartInstantReplay(QuantumInstantReplaySeekMode seekMode = QuantumInstantReplaySeekMode.Disabled, bool loop = false) {
      if (IsRunning) {
        Debug.LogError("Instant replay is already running.");
        return;
      }

      var inputProvider = _liveGame.Session.IsReplay ? _liveGame.Session.ReplayProvider : _liveGame.RecordedInputs;
      if (inputProvider == null) {
        Debug.LogError("Can't run instant replays without an input provider. Start the game with StartParams including RecordingFlags.Input.");
        return;
      }

      IsRunning = true;
      EndFrame = _liveGame.Frames.Verified.Number;

      var deterministicConfig = _liveGame.Session.SessionConfig;
      var desiredReplayFrame = EndFrame - Mathf.FloorToInt(ReplayLength * deterministicConfig.UpdateFPS);

      // clamp against actual start frame
      desiredReplayFrame = Mathf.Max(deterministicConfig.UpdateFPS, desiredReplayFrame);

      var snapshot = _liveGame.GetInstantReplaySnapshot(desiredReplayFrame);
      if (snapshot == null) {
        throw new InvalidOperationException("Unable to find a snapshot for frame " + desiredReplayFrame);
      }

      StartFrame = Mathf.Max(snapshot.Number, desiredReplayFrame);

      List<Frame> snapshotsForRewind = null;
      if (seekMode == QuantumInstantReplaySeekMode.FromIntermediateSnapshots) {
        snapshotsForRewind = new List<Frame>();
        _liveGame.GetInstantReplaySnapshots(desiredReplayFrame, EndFrame, snapshotsForRewind);
        Debug.Assert(snapshotsForRewind.Count >= 1);
      } else if (seekMode == QuantumInstantReplaySeekMode.FromStartSnapshot) {
        snapshotsForRewind = new List<Frame>();
        snapshotsForRewind.Add(snapshot);
      } else if (loop) { 
        throw new ArgumentException(nameof(loop), $"Seek mode not compatible with looping: {seekMode}");
      }

      _loop = loop;


      // Create all required start parameters and serialize the snapshot as start data.
      var param = new QuantumRunner.StartParameters {
        RuntimeConfig = _liveGame.Configurations.Runtime,
        DeterministicConfig = deterministicConfig,
        ReplayProvider = inputProvider,
        GameMode = DeterministicGameMode.Replay,
        FrameData = snapshot.Serialize(DeterministicFrameSerializeMode.Blit),
        InitialFrame = snapshot.Number,
        RunnerId = "InstantReplay",
        PlayerCount = deterministicConfig.PlayerCount,
        LocalPlayerCount = deterministicConfig.PlayerCount,
        HeapExtraCount = snapshotsForRewind?.Count ?? 0,
      };

      _replayRunner = QuantumRunner.StartGame("INSTANTREPLAY", param);
      _replayRunner.OverrideUpdateSession = true;

      // Run a couple of frames until fully initialized (replayRunner.Session.FrameVerified is set and session state isRunning).
      for (int i = 0; i < InitalFramesToSimulation; i++) { 
        _replayRunner.Session.Update(1.0f / deterministicConfig.UpdateFPS);
      }

      // clone the original snapshots
      Debug.Assert(_rewindSnapshots == null);
      if (snapshotsForRewind != null) {
        _rewindSnapshots = new DeterministicFrameRingBuffer(snapshotsForRewind.Count);
        foreach (var frame in snapshotsForRewind) {
          _rewindSnapshots.PushBack(frame, _replayRunner.Game.CreateFrame);
        }
      }

      FastForwardSimulation(desiredReplayFrame);

      if (OnReplayStarted != null)
        OnReplayStarted(_replayRunner.Game);
    }

    public void SeekNormalizedTime(float seek) {
      var frame = Mathf.FloorToInt(Mathf.Lerp(StartFrame, EndFrame, seek));
      SeekFrame(frame);
    }

    public void SeekFrame(int frameNumber) {
      if (!CanSeek) {
        throw new InvalidOperationException("Not seekable");
      }
      if (!IsRunning) {
        throw new InvalidOperationException("Not running");
      }

      Debug.Assert(_rewindSnapshots != null);
      var frame = _rewindSnapshots.Find(frameNumber, DeterministicFrameSnapshotBufferFindMode.ClosestLessThanOrEqual);
      if (frame == null) {
        throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Unable to find a frame with number less or equal to {frameNumber}.");
      }

      _replayRunner.Session.ResetReplay(frame);
      FastForwardSimulation(frameNumber);
    }

    public void StopInstantReplay() {

      if (!IsRunning) {
        Debug.LogError("Instant replay is not running.");
        return;
      }

      IsRunning = false;

      if (OnReplayStopped != null)
        OnReplayStopped(_replayRunner.Game);

      _rewindSnapshots?.Clear();
      _rewindSnapshots = null;

      if (_replayRunner != null)
        _replayRunner.Shutdown();

      _replayRunner = null;
    }

    private void FastForwardSimulation(int frameNumber) {
      var simulationRate = _replayRunner.Session.SessionConfig.UpdateFPS;
      while (_replayRunner.Session.FrameVerified.Number < frameNumber) {
        _replayRunner.Session.Update(1.0f / simulationRate);
      }
    }
  }
}
