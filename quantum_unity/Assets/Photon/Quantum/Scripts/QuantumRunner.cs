using Photon.Deterministic;
using Quantum;
using Quantum.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using UnityEngine;


public sealed class QuantumRunner : MonoBehaviour, IDisposable {
  public static QuantumRunner Default => _activeRunners.Count == 0 ? null : _activeRunners[0];
  public static IEnumerable<QuantumRunner> ActiveRunners => _activeRunners;
  private static List<QuantumRunner> _activeRunners = new List<QuantumRunner>();

  /// <summary>
  ///  Use this prevent the session from being update automatically in order to call Session.Update(DeltaTime) in your own code.
  ///  For example to inject custom delta time values.
  /// </summary>
  [HideInInspector]
  public bool OverrideUpdateSession = false;

  public struct StartParameters {
    public RuntimeConfig                            RuntimeConfig;
    public DeterministicSessionConfig               DeterministicConfig;
    public IDeterministicReplayProvider             ReplayProvider;
    public DeterministicGameMode                    GameMode;
    public Int32                                    InitialFrame;
    public Byte[]                                   FrameData;
    public string                                   RunnerId;
    public QuantumNetworkCommunicator.QuitBehaviour QuitBehaviour;
    public Int32                                    PlayerCount;
    public Int32                                    LocalPlayerCount;
    public RecordingFlags                           RecordingFlags;
    public LoadBalancingClient                      NetworkClient;
    public IResourceManager                         ResourceManagerOverride;
    public InstantReplaySettings                    InstantReplayConfig;
    public Int32                                    HeapExtraCount;
  }

  public QuantumGame          Game            { get; private set; }
  public DeterministicSession Session         { get; private set; }
  public string               Id              { get; private set; }
  public SimulationUpdateTime DeltaTimeType   { get; set; }
  public LoadBalancingClient  NetworkClient   { get; private set; }
  public RecordingFlags       RecordingFlags  { get; private set; }

  public bool IsRunning => Game?.Frames.Predicted != null;

  public float? DeltaTime {
    get {
      switch (DeltaTimeType) {
        case SimulationUpdateTime.EngineDeltaTime:         return Time.deltaTime;
        case SimulationUpdateTime.EngineUnscaledDeltaTime: return Time.unscaledDeltaTime;
      }

      return null;
    }
  }

  private bool _shutdownRequested;

  void Update() {
    if (Session != null && OverrideUpdateSession == false) {
      Session.Update(DeltaTime);
      UnityDB.Update();
    }

    if (_shutdownRequested) {
      _shutdownRequested = false;
      Shutdown();
    }
  }

  void OnDisable() {
    if (Session != null) {
      Session.Destroy();
      Session = null;
      Game    = null;
    }
  }

  void OnDrawGizmos() {
#if UNITY_EDITOR
    if (Session != null) {
      var game = Session.Game as QuantumGame;
      if (game != null) {
        Navigation.Constants.DebugGizmoSize = QuantumEditorSettings.Instance.NavigationGizmoSize;
        QuantumGameGizmos.OnDrawGizmos(game);
      }
    }
#endif
  }

  public void Shutdown() {
    // Runner is shut down, destroys its gameobject, will trigger OnDisable() in next frame, will destroy the session, session will call dispose on the runner.
    Destroy(gameObject);
  }

  public void Dispose() {
    // Called by the Session.Destroy().
    _activeRunners.Remove(this);
  }

  public static void Init(Boolean force = false) {
    // verify using Unity unsafe utils
    MemoryLayoutVerifier.Platform = new QuantumUnityMemoryLayoutVerifierPlatform();

    // set native platform
    Native.Utils = new QuantumUnityNativeUtility();

    // load lookup table
    FPMathUtils.LoadLookupTables(force);

    // Init file loading from inside Quantum
    FileLoader.Init(new UnityFileLoader(QuantumEditorSettings.Instance.DatabasePath));

    // init profiler
    Quantum.Profiling.HostProfiler.Init(x => UnityEngine.Profiling.Profiler.BeginSample(x),
                  () => UnityEngine.Profiling.Profiler.EndSample());

    // init thread profiling (2019.x and up)
    Quantum.Profiling.HostProfiler.InitThread((a, b) => UnityEngine.Profiling.Profiler.BeginThreadProfiling(a, b),
                                          () => UnityEngine.Profiling.Profiler.EndThreadProfiling());

    // init debug draw functions
    Draw.Init(DebugDraw.Ray, DebugDraw.Ray3D, DebugDraw.Line, DebugDraw.Line3D, DebugDraw.Circle, DebugDraw.Sphere, DebugDraw.Rectangle, DebugDraw.Clear);

    // init quantum logger
    Log.Init(Debug.Log, Debug.LogWarning, Debug.LogError, Debug.LogException);

    // init photon logger
    DeterministicLog.Init(Debug.Log, Debug.LogWarning, Debug.LogError, Debug.LogException);
  }

  public static QuantumRunner StartGame(String clientId, StartParameters param) {
    Log.Info("Starting Game");

    // set a default runner id if none is given
    if (param.RunnerId == null) {
      param.RunnerId = "DEFAULT";
    }

    // init debug
    Init();

    // Make sure the runtime config has a simulation config set.
    if (param.RuntimeConfig.SimulationConfig.Id == 0) {
      param.RuntimeConfig.SimulationConfig.Id = SimulationConfig.DEFAULT_ID;
    }

    IResourceManager resourceManager = param.ResourceManagerOverride ?? UnityDB.ResourceManager;

    var simulationConfig = (SimulationConfig)resourceManager.GetAsset(param.RuntimeConfig.SimulationConfig.Id);

    if (param.GameMode == DeterministicGameMode.Multiplayer) {
      if (param.NetworkClient == null) {
        throw new Exception("Requires a NetworkClient to start multiplayer mode");
      }

      if (param.NetworkClient.IsConnected == false) {
        throw new Exception("Not connected to photon");
      }

      if (param.NetworkClient.InRoom == false) {
        throw new Exception("Can't start networked game when not in a room");
      }
    }

    // Make copy of deterministic config here, because we write to it
    var deterministicConfig = DeterministicSessionConfig.FromByteArray(DeterministicSessionConfig.ToByteArray(param.DeterministicConfig));
    deterministicConfig.PlayerCount = param.PlayerCount;

    // Create the runner
    var runner = CreateInstance();
    runner.Id             = param.RunnerId;
    runner.DeltaTimeType  = simulationConfig.DeltaTimeType;
    runner.NetworkClient  = param.NetworkClient;
    runner.RecordingFlags = param.RecordingFlags;
    // Create the game
    runner.Game = new QuantumGame(new QuantumGame.StartParameters() {
      ResourceManager = resourceManager, 
      AssetSerializer = new QuantumUnityJsonSerializer(), 
      CallbackDispatcher = QuantumCallback.Dispatcher,
      EventDispatcher = QuantumEvent.Dispatcher,
      InstantReplaySettings = param.InstantReplayConfig,
      HeapExtraCount = param.HeapExtraCount,
    });

    // new "local mode" runs as "replay" (with Game providing input polling), to avoid rollbacks of the local network debugger.
    // old Local mode can still be used for debug purposes (but RunnerLocalDebug now uses replay mode).
    // if (param.LocalInputProvider == null && param.GameMode == DeterministicGameMode.Local)
    //   param.LocalInputProvider = runner.Game;

    DeterministicPlatformInfo info;
    info            = new DeterministicPlatformInfo();
    info.Allocator  = new QuantumUnityNativeAllocator();
    info.TaskRunner = QuantumTaskRunnerJobs.GetInstance();

#if UNITY_EDITOR
    info.Runtime      = DeterministicPlatformInfo.Runtimes.Mono;
    info.RuntimeHost  = DeterministicPlatformInfo.RuntimeHosts.UnityEditor;
    info.Architecture = DeterministicPlatformInfo.Architectures.x86;
#if UNITY_EDITOR_WIN
    info.Platform = DeterministicPlatformInfo.Platforms.Windows;
#elif UNITY_EDITOR_OSX
    info.Platform = DeterministicPlatformInfo.Platforms.OSX;
#endif
#else
    info.RuntimeHost = DeterministicPlatformInfo.RuntimeHosts.Unity;
#if ENABLE_IL2CPP
    info.Runtime = DeterministicPlatformInfo.Runtimes.IL2CPP;
#else
    info.Runtime = DeterministicPlatformInfo.Runtimes.Mono;
#endif
#if UNITY_STANDALONE_WIN
    info.Platform = DeterministicPlatformInfo.Platforms.Windows;
#elif UNITY_STANDALONE_OSX
    info.Platform = DeterministicPlatformInfo.Platforms.OSX;
#elif UNITY_STANDALONE_LINUX
    info.Platform = DeterministicPlatformInfo.Platforms.Linux;
#elif UNITY_IOS
    info.Platform = DeterministicPlatformInfo.Platforms.IOS;
#elif UNITY_ANDROID
    info.Platform = DeterministicPlatformInfo.Platforms.Android;
#elif UNITY_TVOS
    info.Platform = DeterministicPlatformInfo.Platforms.TVOS;
#elif UNITY_XBOXONE
    info.Platform = DeterministicPlatformInfo.Platforms.XboxOne;
#elif UNITY_PS4
    info.Platform = DeterministicPlatformInfo.Platforms.PlayStation4;
#elif UNITY_SWITCH
    info.Platform = DeterministicPlatformInfo.Platforms.Switch;
#endif
#endif

    DeterministicSessionArgs args;
    args.Mode          = param.GameMode;
    args.RuntimeConfig = RuntimeConfig.ToByteArray(param.RuntimeConfig);
    args.SessionConfig = deterministicConfig;
    args.Game          = runner.Game;
    args.Communicator  = GetCommunicator(param.GameMode, param.NetworkClient, param.QuitBehaviour);
    args.Replay        = param.ReplayProvider;
    args.InitialTick   = param.InitialFrame;
    args.FrameData     = param.FrameData;
    args.PlatformInfo  = info;

    // Create the session
    try {
      runner.Session = new DeterministicSession(args);
    }
    catch (Exception e) {
      Debug.LogException(e);
      runner.Dispose();
      return null;
    }

    // For convenience, to be able to access the runner by the session.
    runner.Session.Runner = runner;

    // Join local players
    runner.Session.Join(clientId, Math.Max(1, param.LocalPlayerCount), param.InitialFrame);

#if QUANTUM_REMOTE_PROFILER
    if (!Application.isEditor) {
      var client = new QuantumProfilingClient(clientId, deterministicConfig, info);
      runner.Game.ProfilerSampleGenerated += (sample) => {
        client.SendProfilingData(sample);
        client.Update();
      };
    }
#endif

    return runner;
  }

  /// <summary>
  /// This cannot be called during the execution of Runner.Update() and Session.Update() methods.
  /// For this immediate needs to be false, which waits until the main thread is outside of Session.Update() to continue the shutdown of all runners.
  /// </summary>
  /// <param name="immediate">Destroy the sessions immediately or wait to Session.Update to complete.</param>
  /// <returns>At least on runner is active and will shut down.</returns>
  public static bool ShutdownAll(bool immediate = false) {
    var result = _activeRunners.Count > 0;
    if (immediate) {
      while (_activeRunners.Count > 0) {
        _activeRunners.Last().Shutdown();
      }
    }
    else {
      foreach (var runner in _activeRunners)
        runner._shutdownRequested = true;
    }

    return result;
  }

  public static QuantumRunner FindRunner(string id) {
    foreach (var runner in _activeRunners) {
      if (runner.Id == id)
        return runner;
    }
    return null;
  }

  public static QuantumRunner FindRunner(IDeterministicGame game) {
    foreach (var runner in _activeRunners) {
      if (runner.Game == game)
        return runner;
    }
    return null;
  }

  [Obsolete("Use FindRunner")]
  internal static QuantumRunner FindRunnerForGame(IDeterministicGame game) => FindRunner(game);

  private static QuantumNetworkCommunicator GetCommunicator(DeterministicGameMode mode,
                                                            LoadBalancingClient   networkClient, QuantumNetworkCommunicator.QuitBehaviour quitBehaviour) {
    if (mode != DeterministicGameMode.Multiplayer) {
      return null;
    }

    return new QuantumNetworkCommunicator(networkClient, quitBehaviour);
  }

  static QuantumRunner CreateInstance() {
    GameObject go = new GameObject("QuantumRunner");
    var runner = go.AddComponent<QuantumRunner>();

    runner._shutdownRequested = false;

    _activeRunners.Add(runner);

    DontDestroyOnLoad(go);

    return runner;
  }
}
