using Photon.Deterministic;
using System;
using Quantum.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Quantum;
using System.IO;
using Quantum.Profiling;
using Quantum.Allocator;
using System.Threading;
using Quantum.Prototypes;
using Quantum.Task;

// Core/Collision.cs
﻿
namespace Quantum {
  
  /// <summary>
  /// Interface for receiving callbacks once per frame while two non-trigger 2D colliders are touching.
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics2D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics2dApi
  public interface ISignalOnCollision2D : ISignal {
    /// <summary>
    /// Called once per frame while two non-trigger 2D colliders are touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="CollisionInfo2D"/> with data about the collision.</param>
    /// \ingroup Physics2dApi
    void OnCollision2D(Frame frame, CollisionInfo2D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once two non-trigger 2D colliders start touching.
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics2D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics2dApi
  public interface ISignalOnCollisionEnter2D : ISignal {
    /// <summary>
    /// Called once two non-trigger 2D colliders start touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="CollisionInfo2D"/> with data about the collision.</param>
    /// \ingroup Physics2dApi
    void OnCollisionEnter2D(Frame frame, CollisionInfo2D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once two non-trigger 2D colliders stop touching.
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics2D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics2dApi
  public interface ISignalOnCollisionExit2D : ISignal {
    /// <summary>
    /// Called once two non-trigger 2D colliders stop touching.
    /// </summary>
    /// <param name="frame">The frame in which the entities stopped touching.</param>
    /// <param name="info">The <see cref="ExitInfo2D"/> with the entities that were touching.</param>
    /// \ingroup Physics2dApi
    void OnCollisionExit2D(Frame frame, ExitInfo2D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once per frame while a non-trigger and a trigger 2D colliders are touching.
  /// <remarks>No collision is checked between two kinematic colliders that are both trigger or both non-trigger.</remarks>
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics2D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics2dApi
  public interface ISignalOnTrigger2D : ISignal {
    /// <summary>
    /// Called once per frame while a non-trigger and a trigger 2D colliders are touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="TriggerInfo2D"/> with data about the trigger collision.</param>
    /// \ingroup Physics2dApi
    void OnTrigger2D(Frame frame, TriggerInfo2D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once a non-trigger and a trigger 2D colliders start touching.
  /// <remarks>No collision is checked between two kinematic colliders that are both trigger or both non-trigger.</remarks>
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics2D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics2dApi
  public interface ISignalOnTriggerEnter2D : ISignal {
    /// <summary>
    /// Called once a non-trigger and a trigger 2D colliders start touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="TriggerInfo2D"/> with data about the trigger collision.</param>
    /// \ingroup Physics2dApi
    void OnTriggerEnter2D(Frame frame, TriggerInfo2D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once a non-trigger and a trigger 2D colliders stop touching.
  /// <remarks>No collision is checked between two kinematic colliders that are both trigger or both non-trigger.</remarks>
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics2D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics2dApi
  public interface ISignalOnTriggerExit2D : ISignal {
    /// <summary>
    /// Called once a non-trigger and a trigger 2D colliders stop touching.
    /// </summary>
    /// <param name="frame">The frame in which the entities stopped touching.</param>
    /// <param name="info">The <see cref="ExitInfo2D"/> with the entities that were touching.</param>
    /// \ingroup Physics2dApi
    void OnTriggerExit2D(Frame frame, ExitInfo2D info);
  }
  
  /// <summary>
  /// Interface for receiving callbacks once per frame while two non-trigger 3D colliders are touching.
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics3D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics3dApi
  public interface ISignalOnCollision3D : ISignal {
    /// <summary>
    /// Called once per frame while two non-trigger 3D colliders are touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="CollisionInfo3D"/> with data about the collision.</param>
    /// \ingroup Physics3dApi
    void OnCollision3D(Frame frame, CollisionInfo3D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once two non-trigger 3D colliders start touching.
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics3D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics3dApi
  public interface ISignalOnCollisionEnter3D : ISignal {
    /// <summary>
    /// Called once two non-trigger 3D colliders start touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="CollisionInfo3D"/> with data about the collision.</param>
    /// \ingroup Physics3dApi
    void OnCollisionEnter3D(Frame frame, CollisionInfo3D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once two non-trigger 3D colliders stop touching.
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics3D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics3dApi
  public interface ISignalOnCollisionExit3D : ISignal {
    /// <summary>
    /// Called once two non-trigger 3D colliders stop touching.
    /// </summary>
    /// <param name="frame">The frame in which the entities stopped touching.</param>
    /// <param name="info">The <see cref="ExitInfo3D"/> with the entities that were touching.</param>
    /// \ingroup Physics3dApi
    void OnCollisionExit3D(Frame frame, ExitInfo3D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once per frame while a non-trigger and a trigger 3D colliders are touching.
  /// <remarks>No collision is checked between two kinematic colliders that are both trigger or both non-trigger.</remarks>
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics3D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics3dApi
  public interface ISignalOnTrigger3D : ISignal {
    /// <summary>
    /// Called once per frame while a non-trigger and a trigger 3D colliders are touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="TriggerInfo3D"/> with data about the trigger collision.</param>
    /// \ingroup Physics3dApi
    void OnTrigger3D(Frame frame, TriggerInfo3D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once a non-trigger and a trigger 3D colliders start touching.
  /// <remarks>No collision is checked between two kinematic colliders that are both trigger or both non-trigger.</remarks>
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics3D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics3dApi
  public interface ISignalOnTriggerEnter3D : ISignal {
    /// <summary>
    /// Called once a non-trigger and a trigger 3D colliders start touching.
    /// </summary>
    /// <param name="frame">The frame in which the collision happened.</param>
    /// <param name="info">The <see cref="TriggerInfo3D"/> with data about the trigger collision.</param>
    /// \ingroup Physics3dApi
    void OnTriggerEnter3D(Frame frame, TriggerInfo3D info);
  }

  /// <summary>
  /// Interface for receiving callbacks once a non-trigger and a trigger 3D colliders stop touching.
  /// <remarks>No collision is checked between two kinematic colliders that are both trigger or both non-trigger.</remarks>
  /// <remarks>At least one of the entities involved in a collision must have the respective <see cref="CallbackFlags"/> set for the callback to be called.</remarks>
  /// <remarks>See <see cref="Physics3D.SetCallbacks"/> for setting the callbacks flags to an entity.</remarks>
  /// </summary>
  /// \ingroup Physics3dApi
  public interface ISignalOnTriggerExit3D : ISignal {
    /// <summary>
    /// Called once a non-trigger and a trigger 3D colliders stop touching.
    /// </summary>
    /// <param name="frame">The frame in which the entities stopped touching.</param>
    /// <param name="info">The <see cref="ExitInfo3D"/> with the entities that were touching.</param>
    /// \ingroup Physics3dApi
    void OnTriggerExit3D(Frame frame, ExitInfo3D info);
  }
}

// Core/Frame.cs
﻿
namespace Quantum {
  /// <summary>
  /// The user implementation of <see cref="FrameBase"/> that resides in the project quantum_state and has access to all user relevant classes.
  /// </summary>
  /// \ingroup FrameClass
  public unsafe partial class Frame : Core.FrameBase {

    public const int DumpFlag_NoSimulationConfig = 1 << 1;
    public const int DumpFlag_NoRuntimeConfig = 1 << 3;
    public const int DumpFlag_NoDeterministicSessionConfig = 1 << 4;
    public const int DumpFlag_NoRuntimePlayers = 1 << 5;
    public const int DumpFlag_NoDynamicDB = 1 << 6;

    struct RuntimePlayerData {
      public Int32         ActorId;
      public Byte[]        Data;
      public RuntimePlayer Player;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    _globals_* _globals;

    // configs
    RuntimeConfig              _runtimeConfig;
    SimulationConfig           _simulationConfig;
    DeterministicSessionConfig _sessionConfig;

    // systems
    SystemBase[] _systemsAll;
    SystemBase[] _systemsRoots;

    // player data
    PersistentMap<Int32, RuntimePlayerData> _playerData;

    ISignalOnPlayerDataSet[] _ISignalOnPlayerDataSet;

    // 2D Physics collision signals
    ISignalOnCollision2D[]      _ISignalOnCollision2DSystems;
    ISignalOnCollisionEnter2D[] _ISignalOnCollisionEnter2DSystems;
    ISignalOnCollisionExit2D[]  _ISignalOnCollisionExit2DSystems;

    // 2D Physics trigger signals
    ISignalOnTrigger2D[]      _ISignalOnTrigger2DSystems;
    ISignalOnTriggerEnter2D[] _ISignalOnTriggerEnter2DSystems;
    ISignalOnTriggerExit2D[]  _ISignalOnTriggerExit2DSystems;

    // 3D Physics collision signals
    ISignalOnCollision3D[]      _ISignalOnCollision3DSystems;
    ISignalOnCollisionEnter3D[] _ISignalOnCollisionEnter3DSystems;
    ISignalOnCollisionExit3D[]  _ISignalOnCollisionExit3DSystems;

    // 3D Physics trigger signals
    ISignalOnTrigger3D[]      _ISignalOnTrigger3DSystems;
    ISignalOnTriggerEnter3D[] _ISignalOnTriggerEnter3DSystems;
    ISignalOnTriggerExit3D[]  _ISignalOnTriggerExit3DSystems;

    ISignalOnNavMeshWaypointReached[] _ISignalOnNavMeshWaypointReachedSystems;
    ISignalOnNavMeshSearchFailed[]    _ISignalOnNavMeshSearchFailedSystems;
    ISignalOnNavMeshMoveAgent[]       _ISignalOnNavMeshMoveAgentSystems;

    ISignalOnMapChanged[]                   _ISignalOnMapChangedSystems;
    ISignalOnEntityPrototypeMaterialized[]  _ISignalOnEntityPrototypeMaterializedSystems;

    /// <summary>
    /// Access the global struct with generated values from the DSL.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public _globals_* Global { get { return _globals; } }

    /// <summary>
    /// The randomization session started with the seed from the <see cref="RuntimeConfig"/> used to start the simulation with.
    /// </summary>
    /// <para>Supports determinism under roll-backs.</para>
    /// <para>If random is used in conjunction with the prediction area feature the session needs to be stored on the entities themselves.</para>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public RNGSession* RNG { get { return &_globals->RngSession; } }

    /// <summary>
    /// Defines the amount of player in this Quantum session.
    /// </summary>
    /// The value is takes from the Deterministic session config.
    public Int32 PlayerCount { get { return _sessionConfig.PlayerCount; } }

    public override NavMeshRegionMask* NavMeshRegionMask => &_globals->NavMeshRegions;

    public override FrameMetaData* FrameMetaData => &_globals->FrameMetaData;

    public override CommitCommandsModes CommitCommandsMode => SimulationConfig.Entities.CommitCommandsMode;

    /// <summary>
    /// Access the signal API.\n
    /// Signals are function signatures used as a decoupled inter-system communication API (a bit like a publisher/subscriber API or observer pattern).
    /// </summary>
    /// Custom signals are defined in the DSL.
    public FrameSignals Signals;

    /// <summary>
    /// Access the event API.\n
    /// Events are a fine-grained solution to communicate things that happen inside the simulation to the rendering engine (they should never be used to modify/update part of the game state).
    /// </summary>
    /// Custom events are defined in the DSL.
    public FrameEvents Events;

    /// <summary>
    /// Access to the assets API
    /// </summary>
    public FrameAssets Assets;

    /// <summary>
    /// The frame user context
    /// </summary>
    public new FrameContextUser Context {
      get { return (FrameContextUser)base.Context; }
    }

    /// <summary>
    /// The <see cref="RuntimeConfig"/> used for this session.
    /// </summary>
    public RuntimeConfig RuntimeConfig { get { return _runtimeConfig; } internal set { _runtimeConfig = value; } }

    /// <summary>
    /// The <see cref="SimulationConfig"/> used for this session.
    /// </summary>
    public SimulationConfig SimulationConfig { get { return _simulationConfig; } internal set { _simulationConfig = value; } }

    /// <summary>
    /// The <see cref="DeterministicSessionConfig"/> used for this session.
    /// </summary>
    public DeterministicSessionConfig SessionConfig { get { return _sessionConfig; } internal set { _sessionConfig = value; } }

    /// <summary>
    /// All systems running in the session.
    /// </summary>
    public SystemBase[] SystemsAll { get { return _systemsAll; } }

    /// <summary>
    /// See <see cref="SimulationRate"/>. This getter acquires the value from the <see cref="SessionConfig"/> though.
    /// </summary>
    public override int UpdateRate { get { return _sessionConfig.UpdateFPS; } }

    /// <summary>
    /// Globally access the physics settings which are taken from the <see cref="SimulationConfig"/> during the Frame constructor.
    /// </summary>
    public sealed override PhysicsSceneSettings* PhysicsSceneSettings { get { return &_globals->PhysicsSettings; } }

    /// <summary>
    /// Delta time in seconds. Can be set during run-time.
    /// </summary>
    public override FP DeltaTime {
      get { return _globals->DeltaTime; }
      set { _globals->DeltaTime = value; }
    }

    /// <summary>
    /// Retrieves the Quantum map asset. Can be set during run-time.
    /// </summary>
    /// If assigned value is different than the current one, signal <see cref="ISignalOnMapChanged"/> is raised.
    public sealed override Map Map {
      get { return FindAsset<Map>(_globals->Map.Id); }
      set {
        AssetRefMap newValue      = value;
        var         previousValue = _globals->Map;
        if (previousValue.Id != newValue.Id) {
          _globals->Map = newValue;
          Signals.OnMapChanged(previousValue);
        }
      }
    }

    public Frame(FrameContext context, SystemBase[] systemsAll, SystemBase[] systemsRoots, DeterministicSessionConfig sessionConfig, RuntimeConfig runtimeConfig, SimulationConfig simulationConfig, FP deltaTime) : base(context) {
      Assert.Check(context != null);

      _systemsAll   = systemsAll;
      _systemsRoots = systemsRoots;

      _runtimeConfig    = runtimeConfig;
      _simulationConfig = simulationConfig;
      _sessionConfig    = sessionConfig;

      //_destroy    = new Queue<EntityRef>(1024);
      _playerData = new PersistentMap<Int32, RuntimePlayerData>();

      AllocGen();
      InitGen();

      Assets     = new FrameAssets(this);
      Events     = new FrameEvents(this);
      Signals    = new FrameSignals(this);
      Unsafe     = new FrameBaseUnsafe(this);

      // player data set signal
      _ISignalOnPlayerDataSet = BuildSignalsArray<ISignalOnPlayerDataSet>();

      // 2D Physics collision signals
      _ISignalOnCollision2DSystems      = BuildSignalsArray<ISignalOnCollision2D>();
      _ISignalOnCollisionEnter2DSystems = BuildSignalsArray<ISignalOnCollisionEnter2D>();
      _ISignalOnCollisionExit2DSystems  = BuildSignalsArray<ISignalOnCollisionExit2D>();

      // 2D Physics trigger signals
      _ISignalOnTrigger2DSystems      = BuildSignalsArray<ISignalOnTrigger2D>();
      _ISignalOnTriggerEnter2DSystems = BuildSignalsArray<ISignalOnTriggerEnter2D>();
      _ISignalOnTriggerExit2DSystems  = BuildSignalsArray<ISignalOnTriggerExit2D>();

      // 3D Physics collision signals
      _ISignalOnCollision3DSystems      = BuildSignalsArray<ISignalOnCollision3D>();
      _ISignalOnCollisionEnter3DSystems = BuildSignalsArray<ISignalOnCollisionEnter3D>();
      _ISignalOnCollisionExit3DSystems  = BuildSignalsArray<ISignalOnCollisionExit3D>();

      // 3D Physics trigger signals
      _ISignalOnTrigger3DSystems      = BuildSignalsArray<ISignalOnTrigger3D>();
      _ISignalOnTriggerEnter3DSystems = BuildSignalsArray<ISignalOnTriggerEnter3D>();
      _ISignalOnTriggerExit3DSystems  = BuildSignalsArray<ISignalOnTriggerExit3D>();

      _ISignalOnNavMeshWaypointReachedSystems = BuildSignalsArray<ISignalOnNavMeshWaypointReached>();
      _ISignalOnNavMeshSearchFailedSystems    = BuildSignalsArray<ISignalOnNavMeshSearchFailed>();
      _ISignalOnNavMeshMoveAgentSystems       = BuildSignalsArray<ISignalOnNavMeshMoveAgent>();

      // map changed signal
      _ISignalOnMapChangedSystems = BuildSignalsArray<ISignalOnMapChanged>();

      // prototype materialized signal
      _ISignalOnEntityPrototypeMaterializedSystems = BuildSignalsArray<ISignalOnEntityPrototypeMaterialized>();
      if ( _ISignalOnEntityPrototypeMaterializedSystems.Length > 0 ) {
        base._SignalOnEntityPrototypeMaterialized = (entity, prototype) => Signals.OnEntityPrototypeMaterialized(entity, prototype);
      }


      // assign map, rng session, etc.
      _globals->Map        = FindAsset<Map>(runtimeConfig.Map.Id);
      _globals->RngSession = new RNGSession(runtimeConfig.Seed);
      _globals->DeltaTime  = deltaTime;

      // set default enabled systems
      for (Int32 i = 0; i < _systemsAll.Length; ++i) {
        if (_systemsAll[i].StartEnabled) {
          BitSet256.Set(&_globals->Systems, _systemsAll[i].RuntimeIndex);
        }
      }

      // init physics settings
      _globals->PhysicsSettings.Gravity                      = simulationConfig.Physics.Gravity;
      _globals->PhysicsSettings.SolverIterations             = simulationConfig.Physics.SolverIterations;
      _globals->PhysicsSettings.UseAngularVelocity           = simulationConfig.Physics.UseAngularVelocity;
      _globals->PhysicsSettings.PenetrationAllowance         = simulationConfig.Physics.PenetrationAllowance;
      _globals->PhysicsSettings.PenetrationCorrection        = simulationConfig.Physics.PenetrationCorrection;
      _globals->PhysicsSettings.MinLinearIntegration         = simulationConfig.Physics.MinLinearIntegration;
      _globals->PhysicsSettings.MinAngularIntegration        = simulationConfig.Physics.MinAngularIntegration;
      _globals->PhysicsSettings.AllowSleeping                = simulationConfig.Physics.AllowSleeping;
      _globals->PhysicsSettings.SleepTimeSec                 = simulationConfig.Physics.SleepTimeSec;
      _globals->PhysicsSettings.LinearSleepTolerance         = simulationConfig.Physics.LinearSleepTolerance;
      _globals->PhysicsSettings.AngularSleepToleranceInRad   = simulationConfig.Physics.AngularSleepToleranceInRad;
      _globals->PhysicsSettings.UseVerticalTransform         = simulationConfig.Physics.UseVerticalTransform;
      _globals->PhysicsSettings.InitLayers(simulationConfig.Physics);

      // Init navmesh regions to all bit fields to be set
      ClearAllNavMeshRegions();

      // user callbacks
      AllocUser();
      InitUser();
    }

    /// <summary>
    /// Set the prediction area.
    /// </summary>
    /// <param name="position">Center of the prediction area</param>
    /// <param name="radius">Radius of the prediction area</param>
    /// <para>The Prediction Culling feature must be explicitly enabled in <see cref="SimulationConfig.UsePredictionArea"/>.</para>
    /// <para>This can be safely called from the main-thread.</para>
    /// <para>Prediction Culling allows developers to save CPU time in games where the player has only a partial view of the game scene.
    /// Quantum prediction and rollbacks, which are time consuming, will only run for important entities that are visible to the local player(s). Leaving anything outside that area to be simulated only once per tick with no rollbacks as soon as the inputs are confirmed from server.
    /// It is safe and simple to activate and, depending on the game, the performance difference can be quite large.Imagine a 30Hz game to constantly rollback ten ticks for every confirmed input (with more players, the predictor eventually misses at least for one of them). This requires the game simulation to be lightweight to be able to run at almost 300Hz(because of the rollbacks). With Prediction Culling enabled the full frames will be simulated at the expected 30Hz all the time while the much smaller prediction area is the only one running within the prediction buffer.</para>
    public void SetPredictionArea(FPVector3 position, FP radius) {
      Context.SetPredictionArea(position, radius);
    }

    /// <summary>
    /// See <see cref="SetPredictionArea(FPVector3, FP)"/>.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    public void SetPredictionArea(FPVector2 position, FP radius) {
      Context.SetPredictionArea(position.XOY, radius);
    }

    /// <summary>
    /// Test is a position is inside the prediction area.
    /// </summary>
    /// <param name="position">Position</param>
    /// <returns>True if the position is inside the prediction area.</returns>
    public Boolean InPredictionArea(FPVector3 position) {
      return Context.InPredictionArea(this, position);
    }

    /// <summary>
    /// See <see cref="InPredictionArea(FPVector3)"/>.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Boolean InPredictionArea(FPVector2 position) {
      return Context.InPredictionArea(this, position);
    }

    public override Byte[] Serialize(DeterministicFrameSerializeMode mode) {
      FrameSerializer serializer;
      serializer         = new FrameSerializer(mode, this, new byte[1024 * 1024 * 20]);
      serializer.Writing = true;

      SerializeState(serializer);

      var packed = ByteUtils.PackByteBlocks(
          BitConverter.GetBytes((int)mode), 
          BitConverter.GetBytes(Number),
          BitConverter.GetBytes(CalculateChecksum()),
          SerializeRuntimePlayers(), 
          serializer.ToArray(),
          SerializeDynamicAssetDB()
        );

      // make sure not pointers are dangling
      serializer.VerifyNoUnresolvedPointers();

      return ByteUtils.GZipCompressBytes(packed);
    }

    public override void Deserialize(Byte[] data) {
      var blocks = ByteUtils.ReadByteBlocks(ByteUtils.GZipDecompressBytes(data)).ToArray();
      
      var mode   = (DeterministicFrameSerializeMode)BitConverter.ToInt32(blocks[0], 0);
      
      Number = BitConverter.ToInt32(blocks[1], 0);
      
      var checksum = BitConverter.ToUInt64(blocks[2], 0);

      DeserializeRuntimePlayers(blocks[3]);
      DeserializeDynamicAssetDB(blocks[5]);

      FrameSerializer serializer;
      serializer         = new FrameSerializer(mode, this, blocks[4]);
      serializer.Reading = true;

      SerializeState(serializer);

      serializer.VerifyNoUnresolvedPointers();

      if (CalculateChecksum() != checksum) {
        throw new Exception($"Checksum of deserialized frame does not match checksum in the source data");
      }
    }

    void SerializeState(FrameSerializer serializer) {
      FrameBase.Serialize(this, serializer);
      _globals_.Serialize(_globals, serializer);
      SerializeEntitiesGen(serializer);
      SerializeUser(serializer);
    }

    Byte[] SerializeRuntimePlayers() {
      BitStream stream;
      stream         = new BitStream(1024 * 10);
      stream.Writing = true;

      stream.WriteInt(_playerData.Count);

      foreach (var player in _playerData.Iterator()) {
        stream.WriteInt(player.Key);
        stream.WriteInt(player.Value.ActorId);
        stream.WriteByteArrayLengthPrefixed(player.Value.Data);
        player.Value.Player.Serialize(stream);
      }

      return stream.ToArray();
    }

    Byte[] SerializeDynamicAssetDB() {
      return _dynamicAssetDb.Serialize(Context.AssetSerializer);
    }

    void DeserializeRuntimePlayers(Byte[] bytes) {
      BitStream stream;
      stream         = new BitStream(bytes);
      stream.Reading = true;

      var count = stream.ReadInt();
      _playerData = new PersistentMap<int, RuntimePlayerData>();
      for (Int32 i = 0; i < count; ++i) {
        var player = stream.ReadInt();

        RuntimePlayerData data;
        data.ActorId = stream.ReadInt();
        data.Data    = stream.ReadByteArrayLengthPrefixed();
        data.Player  = new RuntimePlayer();
        data.Player.Serialize(stream);

        _playerData = _playerData.Add(player, data);
      }
    }

    void DeserializeDynamicAssetDB(Byte[] bytes) {
      _dynamicAssetDb.Deserialize(bytes, Context.AssetSerializer);
    }

    /// <summary>
    /// Dump the frame in human readable form into a string.
    /// </summary>
    /// <returns>Frame representation</returns>
    public sealed override String DumpFrame(int dumpFlags = 0) {
      var printer = new FramePrinter();
      printer.Reset(this);

      // frame info
      printer.AddLine($"#### FRAME DUMP FOR {Number} IsVerified={IsVerified} ####");

      if ((dumpFlags & DumpFlag_NoSimulationConfig) != DumpFlag_NoSimulationConfig) {
        printer.AddLine();
        printer.AddObject("# " + nameof(SimulationConfig), SimulationConfig);
      }

      if ((dumpFlags & DumpFlag_NoRuntimeConfig) != DumpFlag_NoRuntimeConfig) {
        printer.AddLine();
        printer.AddObject("# " + nameof(RuntimeConfig), RuntimeConfig);
      }

      if ((dumpFlags & DumpFlag_NoDeterministicSessionConfig) != DumpFlag_NoDeterministicSessionConfig) {
        printer.AddLine();
        printer.AddObject("# " + nameof(SessionConfig), SessionConfig);
      }

      if ((dumpFlags & DumpFlag_NoRuntimePlayers) != DumpFlag_NoRuntimePlayers) {
        printer.AddLine();
        printer.AddLine("# PLAYERS");
        {
          printer.ScopeBegin();
          foreach (var kv in _playerData.Iterator()) {
            printer.AddObject($"[{kv.Key}]", kv.Value);
          }
          printer.ScopeEnd();
        }
      }

      // globals state
      printer.AddLine();
      printer.AddPointer("# GLOBALS", _globals);

      // print entities
      printer.AddLine();
      printer.AddLine("# ENTITIES");
      Print(this, printer);

      if ((dumpFlags & DumpFlag_NoDynamicDB) != DumpFlag_NoDynamicDB) {
        printer.AddLine();
        printer.AddLine("# DYNAMICDB");
        {
          printer.ScopeBegin();
          printer.AddLine("Dump: ");
          printer.ScopeBegin();
          var data = SerializeDynamicAssetDB();
          fixed (byte* p = data) {
            UnmanagedUtils.PrintBytesHex(p, data.Length, 32, printer);
          }
          printer.ScopeEnd();
          printer.ScopeEnd();
        }
      }

      // heap state
      if ((dumpFlags & DumpFlag_NoHeap) != DumpFlag_NoHeap) {
        printer.AddLine();
        printer.AddLine("# HEAP");
        Allocator.Heap.Print(_heap, printer);
      }

      return printer.ToString();
    }

    /// <summary>
    /// Calculates a checksum for the current game state.
    /// </summary>
    /// <para>Make sure to enable <see cref="DeterministicSessionConfig.ChecksumCrossPlatformDeterminism"/> when running the game of different platforms.</para>
    /// <returns>Checksum for the current game state.</returns>
    public sealed override UInt64 CalculateChecksum() {
      var crc            = (ulong)Number;
      var checksumStream = new FrameChecksumerBitStream(crc);

      FrameSerializer serializer;
      serializer         = new FrameSerializer(DeterministicFrameSerializeMode.Serialize, this, checksumStream);
      serializer.Writing = true;

      // checksum globals
      _globals_.Serialize(_globals, serializer);

      // checksum entity registry
      FrameBase.Serialize(this, serializer);

      // checksum heap
      return checksumStream.Checksum; //Allocator.Heap.CRC(checksumStream.Checksum, _heap);
    }

    /// <summary>
    /// Copies the complete frame memory.
    /// </summary>
    /// <param name="frame">Input frame object</param>
    protected sealed override void Copy(DeterministicFrame frame) {
      var f = (Frame)frame;

      // copy player data
      _playerData = f._playerData;

      // copy heap from frame
      Allocator.Heap.Copy(Context.Allocator, _heap, f._heap);

      // copy entity registry
      FrameBase.Copy(this, f);

      // dynamic DB
      _dynamicAssetDb.CopyFrom(f._dynamicAssetDb);

      // perform native copy
      CopyFromGen(f);
      CopyFromUser(f);
    }

    public sealed override void Free() {
      base.Free();

      // 
      FreeGen();
      FreeUser();
    }

    /// <summary>
    /// Test if a system is enabled.
    /// </summary>
    /// <typeparam name="T">System type</typeparam>
    /// <returns>True if the system is enabled</returns>
    /// Logs an error if the system type is not found.
    public Boolean SystemIsEnabled<T>() where T : SystemBase {
      var system = FindSystem<T>();
      if (system.Item0 == null) {
        return false;
      }

      return BitSet256.IsSet(&_globals->Systems, system.Item1);
    }
    
    public Boolean SystemIsEnabled(Type t) {
      var system = FindSystem(t);
      if (system.Item0 == null) {
        return false;
      }

      return BitSet256.IsSet(&_globals->Systems, system.Item1);
    }

    /// <summary>
    /// Enable a system.
    /// </summary>
    /// <typeparam name="T">System type</typeparam>
    /// Logs an error if the system type is not found.
    public void SystemEnable<T>() where T : SystemBase {
      SystemEnable(typeof(T));
    }
    
    public void SystemEnable(Type t)  {
      var system = FindSystem(t);
      if (system.Item0 == null) {
        return;
      }

      if (BitSet256.IsSet(&_globals->Systems, system.Item1) == false) {
        // set flag
        BitSet256.Set(&_globals->Systems, system.Item1);

        try {
          system.Item0.OnEnabled(this);
        } catch (Exception exn) {
          Log.Exception(exn);
        }
      }
    }

    /// <summary>
    /// Disables a system.
    /// </summary>
    /// <typeparam name="T">System type</typeparam>
    /// Logs an error if the system type is not found.
    /// <example><code>
    /// // test for a certain asset and disable the system during its OnInit method
    /// public override void OnInit(Frame f) {
    ///   var testSettings = f.FindAsset<NavMeshAgentsSettings>(f.Map.UserAsset.Id);
    ///   if (testSettings == null) {
    ///     f.SystemDisable<NavMeshAgentTestSystem>();
    ///     return;
    ///    }
    ///    //..
    ///  }
    /// </code></example>
    public void SystemDisable<T>() where T : SystemBase {
      SystemDisable(typeof(T));
    }

    public void SystemDisable(Type t) {
      var system = FindSystem(t);
      if (system.Item0 == null) {
        return;
      }

      if (BitSet256.IsSet(&_globals->Systems, system.Item1)) {
        // clear flag
        BitSet256.Clear(&_globals->Systems, system.Item1);

        try {
          system.Item0.OnDisabled(this);
        } catch (Exception exn) {
          Log.Exception(exn);
        }
      }
    }
    
    QTuple<SystemBase, Int32> FindSystem<T>() {
      return FindSystem(typeof(T));
    }
    
    QTuple<SystemBase, Int32> FindSystem(Type t) {
      for (Int32 i = 0; i < _systemsAll.Length; ++i) {
        if (_systemsAll[i].GetType() == t) {
          return QTuple.Create(_systemsAll[i], i);
        }
      }

      Log.Error("System '{0}' not found, did you forget to add it to SystemSetup.CreateSystems ?", t.Name);
      return new QTuple<SystemBase, Int32>(null, -1);
    }


    T[] BuildSignalsArray<T>() {
      return _systemsAll.Where(x => x is T).Cast<T>().ToArray();
    }

    void BuildSignalsArrayOnComponentAdded<T>() where T : unmanaged, IComponent {
      Assert.Check(ComponentTypeId<T>.Id > 0);

      var array = _systemsAll.Where(x => x is ISignalOnComponentAdded<T>).Cast<ISignalOnComponentAdded<T>>().ToArray();
      if (array.Length > 0) {
        _ComponentSignalsOnAdded[ComponentTypeId<T>.Id] = (entity, componentData) => {
          var component = (T*)componentData;
          var systems   = &(_globals->Systems);
          for (Int32 i = 0; i < array.Length; ++i) {
            if (BitSet256.IsSet(systems, array[i].RuntimeIndex)) {
              array[i].OnAdded(this, entity, component);
            }
          }
        };
      } else {
        _ComponentSignalsOnAdded[ComponentTypeId<T>.Id] = null;
      }
    }

    void BuildSignalsArrayOnComponentRemoved<T>() where T : unmanaged, IComponent {
      Assert.Check(ComponentTypeId<T>.Id > 0);

      var array = _systemsAll.Where(x => x is ISignalOnComponentRemoved<T>).Cast<ISignalOnComponentRemoved<T>>().ToArray();
      if (array.Length > 0) {
        _ComponentSignalsOnRemoved[ComponentTypeId<T>.Id] = (entity, componentData) => {
          var component = (T*)componentData;
          var systems   = &(_globals->Systems);
          for (Int32 i = 0; i < array.Length; ++i) {
            if (BitSet256.IsSet(systems, array[i].RuntimeIndex)) {
              array[i].OnRemoved(this, entity, component);
            }
          }
        };
      } else {
        _ComponentSignalsOnRemoved[ComponentTypeId<T>.Id] = null;
      }
    }

    void AddEvent(EventBase evnt) {
      // set evnt.Tick
      evnt.Tick = Number;

      // add ast last
      Context.Events.AddLast(evnt);
    }

    // partial declarations populated from code generator
    partial void InitGen();
    partial void FreeGen();
    partial void AllocGen();
    partial void CopyFromGen(Frame                    frame);
    partial void SerializeEntitiesGen(FrameSerializer serializer);

    partial void InitUser();
    partial void FreeUser();
    partial void AllocUser();
    partial void CopyFromUser(Frame frame);

    partial void SerializeUser(FrameSerializer serializer);
    partial void DumpFrameUser(ref String      dump);


    /// <summary>
    /// Gets the runtime player configuration data for a certain player.
    /// </summary>
    /// <param name="player">Player ref</param>
    /// <returns>Player config or null if player was not found</returns>
    public RuntimePlayer GetPlayerData(PlayerRef player) {
      RuntimePlayerData data;

      if (_playerData.TryFind(player, out data)) {
        return data.Player;
      }

      return null;
    }

    /// <summary>
    /// Converts a Quantum PlayerRef to an ActorId (Photon client id).
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <returns>ActorId or null if payer was not found</returns>
    public Int32? PlayerToActorId(PlayerRef player) {
      RuntimePlayerData data;

      if (_playerData.TryFind(player, out data)) {
        return data.ActorId;
      }

      return null;
    }

    /// <summary>
    /// Returns the first player that is using a certain ActorId (Photon client id).
    /// </summary>
    /// <param name="actorId">Actor id</param>
    /// <returns>Player reference or null if actor id was not found</returns>
    /// The first player because multiple players from the same Photon client can join.
    public PlayerRef? ActorIdToFirstPlayer(Int32 actorId) {
      foreach (var kvp in _playerData.Iterator()) {
        if (kvp.Value.ActorId == actorId) {
          return kvp.Key;
        }
      }

      return null;
    }

    /// <summary>
    /// Returns all players with a certain ActorId (Photon client id).
    /// </summary>
    /// <param name="actorId">Actor id</param>
    /// <returns>Array of player references</returns>
    public PlayerRef[] ActorIdToAllPlayers(Int32 actorId) {
      return _playerData.Iterator().Where(x => x.Value.ActorId == actorId).Select(x => (PlayerRef)x.Key).ToArray();
    }

    public void UpdatePlayerData() {
      UInt64 set = 0;

      for (Int32 i = 0; i < PlayerCount; ++i) {
        var rpc = GetRawRpc(i);
        if (rpc != null) {
          var flags = GetPlayerInputFlags(i);
          if ((flags & DeterministicInputFlags.Command) != DeterministicInputFlags.Command) {
            var playerDataOriginal = _playerData;

            try {
              // create player data
              RuntimePlayerData data;
              data.Data    = rpc;
              data.ActorId = BitConverter.ToInt32(rpc, rpc.Length - 4);
              data.Player  = Quantum.RuntimePlayer.FromByteArray(rpc);

              // set data
              _playerData = _playerData.AddOrSet(i, data);

              // set mask
              set |= 1UL << FPMath.Clamp(i, 0, 63);
            } catch {
              _playerData = playerDataOriginal;
            }
          }
        }
      }

      if (set != 0UL) {
        for (Int32 i = 0; i < PlayerCount; ++i) {
          var b = 1UL << i;
          if ((set & b) == b) {
            try {
              Signals.OnPlayerDataSet(i);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }
      }
    }
  }
}

// Core/FrameAssets.cs
namespace Quantum {
  partial class Frame {
    public partial struct FrameAssets {
      Frame _f;

      public FrameAssets(Frame f) {
        _f = f;
      }

      public EntityView View(string view) {
        return _f.FindAsset<EntityView>(view);
      }

      public EntityPrototype Prototype(string prototype) {
        return _f.FindAsset<EntityPrototype>(prototype);
      }
      
      public EntityView View(AssetRefEntityView view) {
        return _f.FindAsset<EntityView>(view.Id);
      }
      
      public EntityPrototype Prototype(AssetRefEntityPrototype prototype) {
        return _f.FindAsset<EntityPrototype>(prototype.Id);
      }
      
      public Map Map(AssetRefMap assetRef) {
        return _f.FindAsset<Map>(assetRef.Id);
      }

      public PhysicsMaterial PhysicsMaterial(AssetRefPhysicsMaterial assetRef) {
        return _f.FindAsset<PhysicsMaterial>(assetRef.Id);
      }

      public PolygonCollider PolygonCollider(AssetRefPolygonCollider assetRef) {
        return _f.FindAsset<PolygonCollider>(assetRef.Id);
      }

      public CharacterController3DConfig CharacterController3DConfig(AssetRefCharacterController3DConfig assetRef) {
        return _f.FindAsset<CharacterController3DConfig>(assetRef.Id);
      }

      public CharacterController2DConfig CharacterController2DConfig(AssetRefCharacterController2DConfig assetRef) {
        return _f.FindAsset<CharacterController2DConfig>(assetRef.Id);
      }

      public NavMesh NavMesh(AssetRefNavMesh assetRef) {
        return _f.FindAsset<NavMesh>(assetRef.Id);
      }

      public NavMeshAgentConfig NavMeshAgentConfig(AssetRefNavMeshAgentConfig assetRef) {
        return _f.FindAsset<NavMeshAgentConfig>(assetRef.Id);
      }

      public SimulationConfig TerrainCollider(AssetRefSimulationConfig assetRef) {
        return _f.FindAsset<SimulationConfig>(assetRef.Id);
      }

      public TerrainCollider TerrainCollider(AssetRefTerrainCollider assetRef) {
        return _f.FindAsset<TerrainCollider>(assetRef.Id);
      }
    }
  }
}

// Core/FrameContextUser.cs
namespace Quantum {
  public partial class FrameContextUser : Core.FrameContext {
    public FrameContextUser(Args args) 
      : base(args) {
      
    }
  }
}

// Core/FrameEvents.cs
﻿
namespace Quantum {
  partial class Frame {
    public partial struct FrameEvents {
      Frame _f;

      public FrameEvents(Frame f) {
        _f = f;
      }
    }
  }
}


// Core/FrameSignals.cs
﻿
namespace Quantum {
  public unsafe interface ISignalOnComponentAdded<T> : ISignal where T : unmanaged, IComponent {
    void OnAdded(Frame f, EntityRef entity, T* component);
  }

  public unsafe interface ISignalOnComponentRemoved<T> : ISignal where T : unmanaged, IComponent {
    void OnRemoved(Frame f, EntityRef entity, T* component);
  } 
  
  public unsafe interface ISignalOnMapChanged : ISignal {
    void OnMapChanged(Frame f, AssetRefMap previousMap);
  }

  public unsafe interface ISignalOnEntityPrototypeMaterialized : ISignal {
    void OnEntityPrototypeMaterialized(Frame f, EntityRef entity, EntityPrototypeRef prototypeRef);
  }

  partial class Frame {
    public unsafe partial struct FrameSignals {
      Frame _f;

      public FrameSignals(Frame f) {
        _f = f;
      }

      public void OnPlayerDataSet(PlayerRef player) {
        var array   = _f._ISignalOnPlayerDataSet;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnPlayerDataSet(_f, player);
          }
        }
      }

      public void OnMapChanged(AssetRefMap previousMap) {
        var array = _f._ISignalOnMapChangedSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnMapChanged(_f, previousMap);
          }
        }
      }

      public void OnEntityPrototypeMaterialized(EntityRef entity, EntityPrototypeRef prototypeRef) {
        var array = _f._ISignalOnEntityPrototypeMaterializedSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnEntityPrototypeMaterialized(_f, entity, prototypeRef);
          }
        }
      }

      public void OnNavMeshWaypointReached(EntityRef entity, FPVector2 waypoint, Navigation.WaypointFlag waypointFlags, ref bool resetAgent) {
        var array   = _f._ISignalOnNavMeshWaypointReachedSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnNavMeshWaypointReached(_f, entity, waypoint, waypointFlags, ref resetAgent);
          }
        }
      }

      public void OnNavMeshSearchFailed(EntityRef entity, ref bool resetAgent) {
        var array   = _f._ISignalOnNavMeshSearchFailedSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnNavMeshSearchFailed(_f, entity, ref resetAgent);
          }
        }
      }

      public void OnNavMeshMoveAgent(EntityRef entity, FPVector2 desiredDirection) {
        var array = _f._ISignalOnNavMeshMoveAgentSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnNavMeshMoveAgent(_f, entity, desiredDirection);
          }
        }
      }

      public void OnCollision2D(CollisionInfo2D info) {
        var array   = _f._ISignalOnCollision2DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollision2D(_f, info);
          }
        }
      }

      public void OnCollisionEnter2D(CollisionInfo2D info) {
        var array   = _f._ISignalOnCollisionEnter2DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollisionEnter2D(_f, info);
          }
        }
      }

      public void OnCollisionExit2D(ExitInfo2D info) {
        var array   = _f._ISignalOnCollisionExit2DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollisionExit2D(_f, info);
          }
        }
      }
      
      public void OnTrigger2D(TriggerInfo2D info) {
        var array   = _f._ISignalOnTrigger2DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTrigger2D(_f, info);
          }
        }
      }

      public void OnTriggerEnter2D(TriggerInfo2D info) {
        var array   = _f._ISignalOnTriggerEnter2DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTriggerEnter2D(_f, info);
          }
        }
      }

      public void OnTriggerExit2D(ExitInfo2D info) {
        var array   = _f._ISignalOnTriggerExit2DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTriggerExit2D(_f, info);
          }
        }
      }
      
      public void OnCollision3D(CollisionInfo3D info) {
        var array   = _f._ISignalOnCollision3DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollision3D(_f, info);
          }
        }
      }

      public void OnCollisionEnter3D(CollisionInfo3D info) {
        var array   = _f._ISignalOnCollisionEnter3DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollisionEnter3D(_f, info);
          }
        }
      }

      public void OnCollisionExit3D(ExitInfo3D info) {
        var array   = _f._ISignalOnCollisionExit3DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollisionExit3D(_f, info);
          }
        }
      }
      
      public void OnTrigger3D(TriggerInfo3D info) {
        var array   = _f._ISignalOnTrigger3DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTrigger3D(_f, info);
          }
        }
      }

      public void OnTriggerEnter3D(TriggerInfo3D info) {
        var array   = _f._ISignalOnTriggerEnter3DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTriggerEnter3D(_f, info);
          }
        }
      }

      public void OnTriggerExit3D(ExitInfo3D info) {
        var array   = _f._ISignalOnTriggerExit3DSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTriggerExit3D(_f, info);
          }
        }
      }
    }
  }
}


// Core/NavMeshSignals.cs
﻿
namespace Quantum
{
  /// <summary>
  /// Signal is fired when an agent reaches a waypoint.
  /// </summary>
  /// <remarks>Requires enabled <see cref="Navigation.Config.EnableNavigationCallbacks"/> in <see cref="SimulationConfig.Navigation"/>.</remarks>
  /// \ingroup NavigationApi
  public unsafe interface ISignalOnNavMeshWaypointReached : ISignal {
    /// <param name="f">Current frame object</param>
    /// <param name="entity">The entity the navmesh agent component belongs to</param>
    /// <param name="waypoint">The current waypoint position</param>
    /// <param name="waypointFlags">The current waypoint flags</param>
    /// <param name="resetAgent">Set this to true if the agent should reset its internal state (default is false). Required when a new target was set during the callback.</param>
    void OnNavMeshWaypointReached(Frame f, EntityRef entity, FPVector2 waypoint, Navigation.WaypointFlag waypointFlags, ref bool resetAgent);
  }

  /// <summary>
  /// Signal is fired when the agent could not find a path in the agent update after using <see cref="NavMeshSteeringAgent.SetTarget(Core.FrameBase, FPVector2, NavMesh, bool)"/>
  /// </summary>
  /// <remarks>Requires enabled <see cref="Navigation.Config.EnableNavigationCallbacks"/> in <see cref="SimulationConfig.Navigation"/>.</remarks>
  /// \ingroup NavigationApi
  public unsafe interface ISignalOnNavMeshSearchFailed: ISignal {
    /// <param name="f">Current frame object</param>
    /// <param name="entity">The entity the navmesh agent component belongs to</param>
    /// <param name="resetAgent">Set this to true if the agent should reset its internal state (default is true).</param>
    void OnNavMeshSearchFailed(Frame f, EntityRef entity, ref bool resetAgent);
  }

  /// <summary>
  /// Signal is called when the agent should move. The desired direction is influence by avoidance.
  /// </summary>
  /// <remarks>The agent velocity should be set in the callback.</remarks>
  /// \ingroup NavigationApi
  public unsafe interface ISignalOnNavMeshMoveAgent: ISignal {
    void OnNavMeshMoveAgent(Frame f, EntityRef entity, FPVector2 desiredDirection);
  }
}


// Core/RecordingFlags.cs
﻿
namespace  Quantum {
  [Flags]
  public enum RecordingFlags {
    None      = 0,
    Input     = 1 << 0,
    Checksums = 1 << 1,
    Default   = Input | Checksums,
    All       = 0xFF
  }
}

// Core/RuntimeConfig.cs
﻿
namespace Quantum {
  /// <summary>
  /// In contrast to the <see cref="SimulationConfig"/>, which has only static configuration data, the RuntimeConfig holds information that can be different from game to game.
  /// </summary>
  /// By default is defines for example what map to load and the random start seed. It is assembled from scratch each time starting a game.
  /// <para>Developers can add custom data to quantum_code/quantum.state/RuntimeConfig.User.cs (don't forget to fill out the serialization methods).</para>
  /// <para>Like the <see cref="DeterministicSessionConfig"/> this config is distributed to every other client after the first player connected and joined the Quantum plugin.</para>
  [Serializable]
  public partial class RuntimeConfig {
    /// <summary> Seed to initialize the randomization session under <see cref="Frame.RNG"/>. </summary>
    public Int32 Seed;
    /// <summary> Asset reference of the Quantum map used with the upcoming game session. </summary>
    public AssetRefMap Map;
    /// <summary> Asset reference to the SimulationConfig used with the upcoming game session. </summary>
    public AssetRefSimulationConfig SimulationConfig;

    /// <summary>
    /// Serializing the members to be send to the server plugin and other players.
    /// </summary>
    /// <param name="stream">Input output stream</param>
    public void Serialize(BitStream stream) {
      stream.Serialize(ref Seed);
      stream.Serialize(ref Map.Id.Value);
      stream.Serialize(ref SimulationConfig.Id.Value);
      SerializeUserData(stream);
    }

    /// <summary>
    /// Dump the content into a human readable form.
    /// </summary>
    /// <returns>String representation</returns>
    public String Dump() {
      String dump = "";
      DumpUserData(ref dump);

      StringBuilder sb = new StringBuilder();
      sb.Append(dump);
      sb.Append("\n");
      sb.AppendLine("Seed: " + Seed);
      sb.AppendLine($"Map.Guid: {Map.ToString()}");
      sb.AppendLine($"SimulationConfig.Guid: {SimulationConfig.ToString()} ");

      return sb.ToString();
    }

    partial void DumpUserData(ref String dump);
    partial void SerializeUserData(BitStream stream);

    /// <summary>
    /// Serialize the class into a byte array.
    /// </summary>
    /// <param name="config">Config to serialized</param>
    /// <returns>Byte array</returns>
    public static Byte[] ToByteArray(RuntimeConfig config) {
      BitStream stream;
      
      stream = new BitStream(new Byte[1024]);
      stream.Writing = true;

      config.Serialize(stream);

      return stream.ToArray();
    }

    /// <summary>
    /// Deserialize the class from a byte array.
    /// </summary>
    /// <param name="data">Config class in byte array form</param>
    /// <returns>New instance of the deserialized class</returns>
    public static RuntimeConfig FromByteArray(Byte[] data) {
      BitStream stream;
      stream = new BitStream(data);
      stream.Reading = true;

      RuntimeConfig config;
      config = new RuntimeConfig();
      config.Serialize(stream);

      return config;
    }
  }
}


// Core/RuntimePlayer.cs
﻿
namespace Quantum {

  public interface ISignalOnPlayerDataSet : ISignal {
    void OnPlayerDataSet(Frame f, PlayerRef player);
  }

  [Serializable]
  public partial class RuntimePlayer {
    public void Serialize(BitStream stream) {
      SerializeUserData(stream);
    }

    public String Dump() {
      String dump = "";
      DumpUserData(ref dump);
      return dump ?? "";
    }

    partial void DumpUserData(ref String dump);
    partial void SerializeUserData(BitStream stream);

    public static Byte[] ToByteArray(RuntimePlayer player) {
      BitStream stream;

      stream = new BitStream(new Byte[8192]);
      stream.Writing = true;

      player.Serialize(stream);

      return stream.ToArray();
    }

    public static RuntimePlayer FromByteArray(Byte[] data) {
      BitStream stream;
      stream = new BitStream(data);
      stream.Reading = true;

      RuntimePlayer player;
      player = new RuntimePlayer();
      player.Serialize(stream);

      return player;
    }
  }
}


// Core/SimulationConfig.cs
﻿
namespace Quantum {
  /// <summary>
  /// The SimulationConfig holds parameters used in the ECS layer and inside core systems like physics and navigation.
  /// </summary>
  [Serializable, AssetObjectConfig(GenerateLinkingScripts = true, GenerateAssetCreateMenu = false, GenerateAssetResetMethod = false)]
  public partial class SimulationConfig : AssetObject {
    public const long DEFAULT_ID = (long)DefaultAssetGuids.SimulationConfig;
    
    /// <summary>
    /// Global navmesh configurations.
    /// </summary>
    public Navigation.Config Navigation;
    /// <summary>
    /// Global physics configurations.
    /// </summary>
    public PhysicsCommon.Config Physics;
    /// <summary>
    /// Global entities configuration
    /// </summary>
    public FrameBase.EntitiesConfig Entities;
    /// <summary>
    /// This option will trigger a Unity scene load during the Quantum start sequence.\n
    /// This might be convenient to start with but once the starting sequence is customized disable it and implement the scene loading by yourself.
    /// </summary>
    public Boolean AutoLoadSceneFromMap = true;
    /// <summary>
    /// Configure how the client tracks the time to progress the Quantum simulation.
    /// </summary>
    public SimulationUpdateTime DeltaTimeType = SimulationUpdateTime.Default;
    /// <summary>
    /// Define the max heap size for one page of memory the frame class uses for custom allocations like QList<> for example.
    /// </summary>
    /// <remarks>2^15 = 32.768 bytes</remarks>
    /// <remarks><code>TotalHeapSizeInBytes = (1 << HeapPageShift) * HeapPageCount</code></remarks>
    public int HeapPageShift = 15;
    /// <summary>
    /// Define the max heap page count for memory the frame class uses for custom allocations like QList<> for example.
    /// </summary>
    /// <remarks><code>TotalHeapSizeInBytes = (1 << HeapPageShift) * HeapPageCount</code></remarks>
    public int HeapPageCount = 256;
    /// <summary>
    /// Sets extra heaps to allocate for a session in case you need to
    /// create 'auxiliary' frames than actually required for the simulation itself
    /// </summary>
    public int HeapExtraCount = 0;
    /// <summary>
    /// Override the number of threads used internally.
    /// </summary>
    public int ThreadCount = 2;
    /// <summary>
    /// How long to store checksumed verified frames. The are used to generate a frame dump in case of a checksum error happening. Not used in Replay and Local mode.
    /// </summary>
    public FP ChecksumSnapshotHistoryLengthSeconds = 3;
  }

  [Serializable, StructLayout(LayoutKind.Explicit)]
  public unsafe struct AssetRefSimulationConfig : IEquatable<AssetRefSimulationConfig> {
    public const int SIZE = sizeof(ulong);

    [FieldOffset(0)]
    public AssetGuid Id;

    public static implicit operator AssetRefSimulationConfig(Map value) {
      var r = default(AssetRefSimulationConfig);
      if (value != null) {
        r.Id = value.Guid;
      }
      return r;
    }

    public static void Serialize(void* ptr, FrameSerializer serializer) {
      var p = (AssetRefSimulationConfig*)ptr;
      AssetGuid.Serialize(&p->Id, serializer);
    }

    public override string ToString() {
      return AssetRef.ToString(Id);
    }

    public bool Equals(AssetRefSimulationConfig other) {
      return Id.Equals(other.Id);
    }

    public override bool Equals(object obj) {
      return obj is AssetRefSimulationConfig other && Equals(other);
    }

    public override int GetHashCode() {
      return Id.GetHashCode();
    }
    
    public static bool operator ==(AssetRefSimulationConfig a, AssetRefSimulationConfig b) {
      return a.Id == b.Id;
    }
    
    public static bool operator !=(AssetRefSimulationConfig a, AssetRefSimulationConfig b) {
      return a.Id != b.Id;
    }
  }

  public static class AssetRefSimulationConfigExt {
    public static SimulationConfig FindAsset(this Core.FrameBase f, AssetRefSimulationConfig assetRef) {
      return f.FindAsset<SimulationConfig>(assetRef.Id);
    }
  }
}


// Core/SimulationUpdateTime.cs
﻿namespace Quantum {
  /// <summary>
  /// The type of measuring time progressions to update the local simulation.
  /// </summary>
  /// <para>Caveat: Changing it will make every client use the setting which might be undesirable when only used for debugging.</para>
  public enum SimulationUpdateTime {
    /// <summary>
    /// Internal stopwatch. Recommended for releasing games.
    /// </summary>
    Default,
    /// <summary>
    /// Engine (Unity) delta time. Extremely useful when pausing the Unity simulation during debugging for example.
    /// </summary>
    /// Caveat: the setting can cause issues with time synchronization when initializing online matches: the time tracking can be inaccurate under load (e.g.level loading) and result in a lot of large extra time syncs request and canceled inputs for a client when starting an online game.
    EngineDeltaTime,
    /// <summary>
    /// Engine unscaled delta time.
    /// </summary>
    EngineUnscaledDeltaTime
  }
}

// Core/TypeRegistry.cs

namespace Quantum {
  public partial class TypeRegistry {
    readonly Dictionary<Type, int> _types = new Dictionary<Type, int>();

    public ReadOnlyDictionary<Type, int> Types {
      get {
        return new ReadOnlyDictionary<Type, int>(_types);
      }
    }

    public TypeRegistry() {
      AddBuiltIns();
      AddGenerated();
    }

    void Register(Type type, int size) {
      if (_types.ContainsKey(type)) {
        return;
      }
      
      _types.Add(type, size);
    }

    void AddBuiltIns() {
      Register(typeof(EntityRef), EntityRef.SIZE);
      Register(typeof(ComponentReference), ComponentReference.SIZE);
      Register(typeof(AssetRef), AssetRef.SIZE);
      Register(typeof(Shape2DType), 1);
      Register(typeof(Shape3DType), 1);
      Register(typeof(Shape2D), Shape2D.SIZE);
      Register(typeof(Shape2D.BoxShape), Shape2D.BoxShape.SIZE);
      Register(typeof(Shape2D.CircleShape), Shape2D.CircleShape.SIZE);
      Register(typeof(Shape2D.PolygonShape), Shape2D.PolygonShape.SIZE);
      Register(typeof(Shape2D.EdgeShape), Shape2D.EdgeShape.SIZE);
      Register(typeof(Shape2D.CompoundShape2D), Shape2D.CompoundShape2D.SIZE);
      Register(typeof(Shape3D), Shape3D.SIZE);
      Register(typeof(Shape3D.BoxShape), Shape3D.BoxShape.SIZE);
      Register(typeof(Shape3D.SphereShape), Shape3D.SphereShape.SIZE);
      Register(typeof(Shape3D.MeshShape), Shape3D.MeshShape.SIZE);
      Register(typeof(Shape3D.TerrainShape), Shape3D.TerrainShape.SIZE);
      Register(typeof(Shape3D.CompoundShape3D), Shape3D.CompoundShape3D.SIZE);
      Register(typeof(Joint2D), Joint2D.SIZE);
      Register(typeof(Joint2DType), 4);
      Register(typeof(Joint2D.SpringDamperJoint), Joint2D.SpringDamperJoint.SIZE);
      Register(typeof(Joint3D), Joint3D.SIZE);
      Register(typeof(Joint3DType), 4);
      Register(typeof(Joint3D.SpringDamperJoint), Joint3D.SpringDamperJoint.SIZE);
      Register(typeof(QBoolean), QBoolean.SIZE);

      // register internal heap types - heap struct itself does not need to be registered
      Allocator.Heap.RegisterInternalTypes(Register);
      
      // register internal physics types
      PhysicsCommon.RegisterInternalTypes(Register);
      
      // register collection memory integrity checks
      Collections.QCollectionsUtils.RegisterTypes(Register);
    }
    
    partial void AddGenerated();
  }
}

// Game/CallbackDispatcher.cs
﻿
namespace Quantum {
  public class CallbackDispatcher : DispatcherBase, Quantum.ICallbackDispatcher {

    protected static Dictionary<Type, Int32> GetBuiltInTypes() {
      return new Dictionary<Type, Int32>() {
        { typeof(CallbackChecksumComputed), CallbackChecksumComputed.ID },
        { typeof(CallbackChecksumError), CallbackChecksumError.ID },
        { typeof(CallbackChecksumErrorFrameDump), CallbackChecksumErrorFrameDump.ID },
        { typeof(CallbackEventCanceled), CallbackEventCanceled.ID },
        { typeof(CallbackEventConfirmed), CallbackEventConfirmed.ID },
        { typeof(CallbackGameDestroyed), CallbackGameDestroyed.ID },
        { typeof(CallbackGameStarted), CallbackGameStarted.ID },
        { typeof(CallbackGameResynced), CallbackGameResynced.ID },
        { typeof(CallbackInputConfirmed), CallbackInputConfirmed.ID },
        { typeof(CallbackPollInput), CallbackPollInput.ID },
        { typeof(CallbackSimulateFinished), CallbackSimulateFinished.ID },
        { typeof(CallbackUpdateView), CallbackUpdateView.ID },
      };
    }

    public CallbackDispatcher() : base(GetBuiltInTypes()) { }
    protected CallbackDispatcher(Dictionary<Type, Int32> callbackTypes) : base(callbackTypes) { }

    public bool Publish(CallbackBase e) {
      return base.InvokeMeta(e.ID, e);
    }
  }
}


// Game/EventDispatcher.cs
﻿
namespace Quantum {
  public class EventDispatcher : DispatcherBase, IEventDispatcher {

    private static Dictionary<Type, Int32> GetEventTypes() {
      var result = new Dictionary<Type, Int32> {
        { typeof(EventBase), 0 }
      };

      for (int eventID = 0; eventID < Frame.FrameEvents.EVENT_TYPE_COUNT; ++eventID) {
        result.Add(Frame.FrameEvents.GetEventType(eventID), eventID + 1);
      }

      return result;
    }

    public EventDispatcher() : base(GetEventTypes()) { }

    public unsafe bool Publish(EventBase e) {

      int eventDepth = 0;
      for (int id = e.Id; id >= 0; id = Frame.FrameEvents.GetParentEventID(id)) {
        ++eventDepth;
      }

      int* eventIdStack = stackalloc int[eventDepth];
      for (int id = e.Id, i = 0; id >= 0; id = Frame.FrameEvents.GetParentEventID(id), i++) {
        eventIdStack[i] = id;
      }

      bool hadActiveHandlers = false;

      // start with the EventBase
      int metaIndex = 0;

      for (; ; ) {
        hadActiveHandlers |= base.InvokeMeta(metaIndex, e);

        if (--eventDepth >= 0) {
          // choose next event
          metaIndex = eventIdStack[eventDepth] + 1;
        } else {
          break;
        }
      }

      return hadActiveHandlers;
    }
  }
}


// Game/InstantReplaySettings.cs
﻿
namespace Quantum {
  [Serializable]
  public struct InstantReplaySettings {
    public int SnapshotsPerSecond;
    public FP LenghtSeconds;

    public static InstantReplaySettings Default => new InstantReplaySettings() {
      LenghtSeconds = 3,
      SnapshotsPerSecond = 1,
    };

    public static InstantReplaySettings FromLength(FP length, int snapshotsPerSecond) {
      return new InstantReplaySettings() {
        LenghtSeconds = length,
        SnapshotsPerSecond = snapshotsPerSecond
      };
    }

    public override string ToString() {
      return $"({nameof(SnapshotsPerSecond)}: {SnapshotsPerSecond}, {nameof(LenghtSeconds)}: {LenghtSeconds})";
    }
  }
}


// Game/QuantumGame.Snapshots.cs
﻿
namespace Quantum {
  public partial class QuantumGame {

    DeterministicFrameRingBuffer _checksumSnapshotBuffer;
    DeterministicFrameRingBuffer _instantReplaySnapshotBuffer;

    bool _instantReplaySnapshotsRecording;
    Int32 _commonSnapshotInterval;
    Int32 _instantReplaySnapshotInterval;

    void SnapshotsOnDestroy() {
      _checksumSnapshotBuffer?.Clear();
      _checksumSnapshotBuffer = null;
      _instantReplaySnapshotBuffer?.Clear();
      _instantReplaySnapshotBuffer = null;
    }

    void SnapshotsOnSimulateFinished(DeterministicFrame state) {

      if (!state.IsVerified) {
        return;
      }

      HostProfiler.Start("QuantumGame.RecordingSnapshots");

      if (_checksumSnapshotBuffer != null) {
        // in case replay interval is less than checksum interval and replay is not being recorded,
        // there's no need to sample at a common rate
        Int32 interval;
        if (_commonSnapshotInterval > 0 && _instantReplaySnapshotsRecording) {
          Assert.Check(_instantReplaySnapshotBuffer == null);
          interval = _commonSnapshotInterval;
        } else {
          interval = Session.SessionConfig.ChecksumInterval;
        }

        if ((state.Number % interval) == 0) {
          _checksumSnapshotBuffer.PushBack(state, this, _context);
        }
      }

      if (_instantReplaySnapshotsRecording && _instantReplaySnapshotBuffer != null) {
        Assert.Check(_commonSnapshotInterval <= 0);
        if (_instantReplaySnapshotBuffer.Count == 0 || (state.Number % _instantReplaySnapshotInterval) == 0) {
          _instantReplaySnapshotBuffer.PushBack(state, this, _context);
        }
      }

      HostProfiler.End();
    }

    Int32 SnapshotsCreateBuffers(Int32 simulationRate, Int32 checksumInterval, FP checksumTimeWindow, Int32 replayInterval, FP replayTimeWindow) {

      Int32 checksumBufferSize = SnapshotsGetMinBufferSize(simulationRate, checksumTimeWindow, checksumInterval);
      Int32 replayBufferSize = SnapshotsGetMinBufferSize(simulationRate, replayTimeWindow, replayInterval);

      if (checksumBufferSize > 0 && replayBufferSize > 0) {
        // possibility for a shared buffer
        var common = SnapshotsGetCommonBufferSettings(checksumBufferSize, checksumInterval, replayBufferSize, replayInterval);
        if (common != null) {
          _commonSnapshotInterval = common.Value.Item1;
          _checksumSnapshotBuffer = new DeterministicFrameRingBuffer(common.Value.Item0);
          return common.Value.Item0;
        }

        // shared buffer not possible
        Log.Warn($"Unable to create a shared buffer for checksumed frames and replay snapshots. This is not optimal. Check the documentation for details.");
      }

      if (checksumBufferSize > 0) {
        _checksumSnapshotBuffer = new DeterministicFrameRingBuffer(checksumBufferSize);
      }

      if (replayBufferSize > 0) {
        _instantReplaySnapshotInterval = replayInterval;
        _instantReplaySnapshotBuffer = new DeterministicFrameRingBuffer(replayBufferSize);
      }

      return checksumBufferSize + replayBufferSize;
    }

    static Int32 SnapshotsGetMinBufferSize(Int32 simulationRate, FP duration, Int32 samplingRate) {
      return samplingRate <= 0 ? 0 : FPMath.CeilToInt(1 + simulationRate * duration / samplingRate);
    }

    static QTuple<Int32, Int32>? SnapshotsGetCommonBufferSettings(Int32 bufferASize, Int32 bufferAInterval, Int32 bufferBSize, Int32 bufferBInterval) {
      if (bufferASize > 0 && bufferBSize > 0) {
        // possibility for a shared buffer

        bool intervalAIsMultipleOfB = bufferAInterval % bufferBInterval == 0;
        if (intervalAIsMultipleOfB) {
          Int32 m = bufferAInterval / bufferBInterval;
          if (bufferASize * m <= bufferBSize) {
            return QTuple.Create(bufferBSize, bufferBInterval);
          }
        }

        bool intervalBIsMultipleOfA = bufferBInterval % bufferAInterval == 0;
        if (intervalBIsMultipleOfA) {
          Int32 m = bufferBInterval / bufferAInterval;
          if (bufferBSize * m <= bufferASize) {
            return QTuple.Create(bufferASize, bufferAInterval);
          }
        }
      }

      return null;
    }
  }
}

// Game/QuantumGame.cs
﻿
namespace Quantum {
  /// <summary>
  /// QuantumGame acts as an interface to the simulation from the client code's perspective.
  /// </summary>
  /// Access and method to this class is always safe from the clients point of view.
  public unsafe partial class QuantumGame : IDeterministicGame {
    public event Action<ProfilerContextData> ProfilerSampleGenerated;

    public struct StartParameters {
      public IResourceManager       ResourceManager;
      public IAssetSerializer       AssetSerializer;
      public ICallbackDispatcher    CallbackDispatcher;
      public IEventDispatcher       EventDispatcher;
      public InstantReplaySettings  InstantReplaySettings;
      public int                    HeapExtraCount;
    }


    /// <summary>
    /// Stores the different frames the simulation uses during one tick.
    /// </summary>
    public class FramesContainer {
      public Frame Verified;
      public Frame Predicted;
      public Frame PredictedPrevious;
      public Frame PreviousUpdatePredicted;
    }

    // Caveat: Only set after the first CreateFrame() call
    public class ConfigurationsContainer {
      public RuntimeConfig Runtime;
      public SimulationConfig Simulation;
    }

    /// <summary> Access the frames of various times available during one tick. </summary>
    public FramesContainer Frames { get; }

    /// <summary> Access the configurations that the simulation is running with. </summary>
    public ConfigurationsContainer Configurations { get; }

    /// <summary> Access the Deterministic session object to query more internals. </summary>
    public DeterministicSession Session { get; private set; }

    /// <summary> Used for position interpolation on the client for smoother interpolation results. </summary>
    public Single InterpolationFactor { get; private set; }

    /// <summary> </summary>
    public InstantReplaySettings InstantReplayConfig { get; private set; }

    /// <summary> Extra heaps to allocate for a session in case you need to create 'auxiliary' frames than actually required for the simulation itself. </summary>
    public int HeapExtraCount { get; }


    Byte[] _inputStreamReadZeroArray;
    IResourceManager _resourceManager;
    IAssetSerializer _assetSerializer;
    ICallbackDispatcher _callbackDispatcher;
    IEventDispatcher _eventDispatcher;

    FrameSerializer _inputSerializerRead;
    FrameSerializer _inputSerializerWrite;

    SystemBase[] _systemsRoot;
    SystemBase[] _systemsAll;

    FrameContext _context;
    TypeRegistry _typeRegistry;
    bool _polledInputInThisSimulation;

    public QuantumGame(in StartParameters startParams) {
      _typeRegistry = new TypeRegistry();
      Frames = new FramesContainer();
      Configurations = new ConfigurationsContainer();
      _resourceManager = startParams.ResourceManager;
      _assetSerializer = startParams.AssetSerializer;
      _callbackDispatcher = startParams.CallbackDispatcher;
      _eventDispatcher = startParams.EventDispatcher;
      InstantReplayConfig = startParams.InstantReplaySettings;
      HeapExtraCount = startParams.HeapExtraCount;

      InitCallbacks();
    }

    [Obsolete]
    public QuantumGame(IResourceManager manager, IAssetSerializer assetSerializer, ICallbackDispatcher callbackDispatcher, IEventDispatcher eventDispatcher)
      : this(new StartParameters() {
        ResourceManager = manager,
        AssetSerializer = assetSerializer,
        CallbackDispatcher = callbackDispatcher,
        EventDispatcher = eventDispatcher,
      }) { }

    /// <summary>
    /// Returns an array that is unique on  every client and represents the indexes for players that your local machine controls in the Quantum simulation.
    /// </summary>
    /// <returns>Array of player indices</returns>
    public Int32[] GetLocalPlayers() {
      return Session.LocalPlayerIndices;
    }

    /// <summary>
    ///  Helps to decide if a PlayerRef is associated with the local player.
    /// </summary>
    /// <param name="playerRef">Player reference</param>
    /// <returns>True if the player is the local player</returns>
    public Boolean PlayerIsLocal(PlayerRef playerRef) {
      if (playerRef == PlayerRef.None) {
        return false;
      }

      for (Int32 i = 0; i < Session.LocalPlayerIndices.Length; i++) {
        if (Session.LocalPlayerIndices[i] == playerRef) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="command">Command to send</param>
    /// Commands are similar to input, they drive the simulation, but do not have to be send regularly.
    /// <example><code>
    /// RemoveUnitCommand command = new RemoveUnitCommand();
    /// command.CellIndex = 42;
    /// QuantumRunner.Default.Game.SendCommand(command);
    /// </code></example>
    public void SendCommand(DeterministicCommand command) {
      var players = GetLocalPlayers();
      if (players.Length > 0) {
        Session.SendCommand(players[0], command);
      } else {
        Log.Error("No local player found to send command for");
      }
    }

    /// <summary>
    /// Sends a command to the server.
    /// </summary>
    /// <param name="player">Specify the player index (PlayerRef) when you have multiple players controlled from the same machine.</param>
    /// <param name="command">Command to send</param>
    /// <para>See <see cref="SendCommand(DeterministicCommand)"/></para>
    /// <para>Games that only have one local player can ignore the player index field.</para>
    public void SendCommand(Int32 player, DeterministicCommand command) {
      Session.SendCommand(player, command);
    }

    /// <summary>
    /// Send data for one local player to join the online match.
    /// </summary>
    /// <param name="player">Local player index</param>
    /// <param name="data">Player data</param>
    /// After starting, joining the Quantum Game and after the OnGameStart signal has been fired each player needs to call the SendPlayerData method to be added as a player in every ones simulation.\n
    /// The reason this needs to be called explicitly is that it greatly simplifies late-joining players.
    public void SendPlayerData(Int32 player, RuntimePlayer data) {
      Session.SetPlayerData(player, RuntimePlayer.ToByteArray(data));
    }

    public void OnDestroy() {
      SnapshotsOnDestroy();
      InvokeOnDestroy();
    }

    public Frame CreateFrame() {
      return (Frame)((IDeterministicGame)this).CreateFrame(_context);
    }

    DeterministicFrame IDeterministicGame.CreateFrame(IDisposable context) {
      return new Frame((FrameContextUser)context, _systemsAll, _systemsRoot, Session.SessionConfig, Configurations.Runtime, Configurations.Simulation, Session.DeltaTime);
    }

    DeterministicFrame IDeterministicGame.CreateFrame(IDisposable context, Byte[] data) {
      Frame f = CreateFrame();
      f.Deserialize(data);
      return f;
    }

    public DeterministicFrame GetVerifiedFrame(int tick) {
      if (_checksumSnapshotBuffer != null) {
        var result = _checksumSnapshotBuffer.Find(tick, DeterministicFrameSnapshotBufferFindMode.Equal);
        if (result == null) {
          Log.Warn($"Unable to find verified frame for tick {tick}, increase ChecksumInterval or increase ChecksumFrameBufferSize.");
        }
        return result;
      }
      return null;
    }

    public IDisposable CreateFrameContext() {
      if (_context == null) {
        Assert.Check(_systemsAll == null);
        Assert.Check(_systemsRoot == null);

        // create asset database
        var assetDB = _resourceManager.CreateAssetDatabase();

        // de-serialize runtime config, session is the one from the server
        Configurations.Runtime = RuntimeConfig.FromByteArray(Session.RuntimeConfig);
        Configurations.Simulation = assetDB.FindAsset<SimulationConfig>(Configurations.Runtime.SimulationConfig.Id, true);

        // register commands
        Session.CommandSerializer.RegisterPrototypes(CommandSetup.CreateCommands(Configurations.Runtime, Configurations.Simulation));

        // initialize systems
        _systemsRoot = SystemSetup.CreateSystems(Configurations.Runtime, Configurations.Simulation);
        _systemsAll = _systemsRoot.SelectMany(x => x.Hierarchy).ToArray();

        Int32 heapCount = 4;
        heapCount += Math.Max(0, Configurations.Simulation.HeapExtraCount);
        heapCount += Math.Max(0, HeapExtraCount);
        heapCount += SnapshotsCreateBuffers(Session.SessionConfig.UpdateFPS,
          Session.GameMode == DeterministicGameMode.Multiplayer ? Session.SessionConfig.ChecksumInterval : 0, Configurations.Simulation.ChecksumSnapshotHistoryLengthSeconds,
          InstantReplayConfig.SnapshotsPerSecond == 0 ? 0 : Session.SessionConfig.UpdateFPS / InstantReplayConfig.SnapshotsPerSecond, InstantReplayConfig.LenghtSeconds);

        // set system runtime indices
        for (Int32 i = 0; i < _systemsAll.Length; ++i) {
          _systemsAll[i].RuntimeIndex = i;
        }

        // set core count override
        Session.PlatformInfo.CoreCount = Configurations.Simulation.ThreadCount;

        FrameContext.Args args;
        args.AssetDatabase = assetDB;
        args.PlatformInfo = Session.PlatformInfo;
        args.IsLocalPlayer = Session.IsLocalPlayer;
        args.HeapConfig = new Heap.Config(Configurations.Simulation.HeapPageShift, Configurations.Simulation.HeapPageCount, heapCount);
        args.PhysicsConfig = Configurations.Simulation.Physics;
        args.NavigationConfig = Configurations.Simulation.Navigation;
        args.CommandSerializer = Session.CommandSerializer;
        args.AssetSerializer = _assetSerializer;

        // toggle various parts of the context code
        args.UsePhysics2D = _systemsAll.FirstOrDefault(x => x is PhysicsSystem2D) != null;
        args.UsePhysics3D = _systemsAll.FirstOrDefault(x => x is PhysicsSystem3D) != null;
        args.UseNavigation = _systemsAll.FirstOrDefault(x => x is NavigationSystem) != null;
        args.UseCullingArea = _systemsAll.FirstOrDefault(x => x is CullingSystem2D) != null || _systemsAll.FirstOrDefault(x => x is CullingSystem3D) != null;

        // create frame context
        _context = new FrameContextUser(args);
      }

      return _context;
    }

    /// <summary>
    /// Set the prediction area.
    /// </summary>
    /// <param name="position">Center of the prediction area</param>
    /// <param name="radius">Radius of the prediction area</param>
    /// <para>The Prediction Culling feature must be explicitly enabled in <see cref="SimulationConfig.UsePredictionArea"/>.</para>
    /// <para>This can be safely called from the main-thread.</para>
    /// <para>Prediction Culling allows developers to save CPU time in games where the player has only a partial view of the game scene.
    /// Quantum prediction and rollbacks, which are time consuming, will only run for important entities that are visible to the local player(s). Leaving anything outside that area to be simulated only once per tick with no rollbacks as soon as the inputs are confirmed from server.
    /// It is safe and simple to activate and, depending on the game, the performance difference can be quite large.Imagine a 30Hz game to constantly rollback ten ticks for every confirmed input (with more players, the predictor eventually misses at least for one of them). This requires the game simulation to be lightweight to be able to run at almost 300Hz(because of the rollbacks). With Prediction Culling enabled the full frames will be simulated at the expected 30Hz all the time while the much smaller prediction area is the only one running within the prediction buffer.</para>
    public void SetPredictionArea(FPVector3 position, FP radius) {
      _context.SetPredictionArea(position, radius);
    }

    /// <summary>
    /// See <see cref="SetPredictionArea(FPVector3, FP)"/>.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    public void SetPredictionArea(FPVector2 position, FP radius) {
      _context.SetPredictionArea(position.XOY, radius);
    }

    public void OnGameEnded() {
      InvokeOnGameEnded();
    }

    public void OnGameStart(DeterministicFrame f) {
      // init event invoker
      InitEventInvoker(Session.RollbackWindow);

      Frames.Predicted = (Frame)f;
      Frames.PredictedPrevious = (Frame)f;
      Frames.Verified = (Frame)f;
      Frames.PreviousUpdatePredicted = (Frame)f;

      InvokeOnGameStart();

      // init systems on latest frame
      InitSystems(f);
    }

    public void OnGameResync() {
      _checksumSnapshotBuffer?.Clear();
      _instantReplaySnapshotBuffer?.Clear();

      // reset physics engines statics
      Frames.Verified.Physics2D.Init();
      Frames.Verified.Physics3D.Init();
      
      InvokeOnGameResync();
    }

    public DeterministicFrameInputTemp OnLocalInput(Int32 frame, Int32 player) {
      var input = default(QTuple<Input, DeterministicInputFlags>);

      // poll input
      try {
        bool isFirst = _polledInputInThisSimulation == false;
        _polledInputInThisSimulation = true;
        input = InvokeOnPollInput(frame, player, isFirst);
      } catch (Exception exn) {
        Log.Error("## Input Code Threw Exception ##");
        Log.Exception(exn);
      }

      if (_inputSerializerWrite == null) {
        _inputSerializerWrite = new FrameSerializer(DeterministicFrameSerializeMode.Serialize, null, new Byte[1024]);
      }

      // clear old data
      _inputSerializerWrite.Reset();
      _inputSerializerWrite.Writing = true;
      _inputSerializerWrite.InputMode = true;

      // pack into stream
      Input.Write(_inputSerializerWrite, input.Item0);

      // return temp input
      return DeterministicFrameInputTemp.Predicted(frame, player, _inputSerializerWrite.Stream.Data, _inputSerializerWrite.Stream.BytesRequired, input.Item1);
    }

    public void OnSimulate(DeterministicFrame state) {
      HostProfiler.Start("QuantumGame.OnSimulate");

      var f = (Frame)state;

      try {
        // reset profiling
        HostProfiler.Start("Init Profiler");
        f.Context.ProfilerContext.Reset();
        var profiler = f.Context.ProfilerContext.GetProfilerForTaskThread(0);
        HostProfiler.End();

        HostProfiler.Start("ApplyInputs");
        ApplyInputs(f);
        HostProfiler.End();

        HostProfiler.Start("OnSimulateBegin");
        f.Context.OnFrameSimulationBegin(f);
        f.OnFrameSimulateBegin();
        f.Context.TaskContext.BeginFrame(f);
        HostProfiler.End();

        var handle = f.Context.TaskContext.AddRootTask();

        HostProfiler.Start("UpdatePlayerData");
        f.UpdatePlayerData();
        HostProfiler.End();

        profiler.Start("Scheduling Tasks #ff9900");
        HostProfiler.Start("Scheduling Tasks");

        var systems = &f.Global->Systems;

        for (Int32 i = 0; i < _systemsRoot.Length; ++i) {
          if (BitSet256.IsSet(systems, _systemsRoot[i].RuntimeIndex)) {
            try {
              handle = _systemsRoot[i].OnSchedule(f, handle);
            } catch (Exception exn) {
              LogSimulationException(exn);
            }
          }
        }

        HostProfiler.End();
        profiler.End();

        try {
          f.Context.TaskContext.EndFrame();
          f.OnFrameSimulateEnd();
          f.Context.OnFrameSimulationEnd();
        } catch (Exception exn) {
          Log.Exception(exn);
        }

#if PROFILER_REPORT
        if (ProfilerSampleGenerated != null) {
          var data = f.Context.ProfilerContext.CreateReport(f.Number, f.IsVerified);
          ProfilerSampleGenerated(data);
        }
#endif

#if PROFILER_FRAME_AVERAGE
      f.Context.ProfilerContext.StoreFrameTime();
      Log.Info("Frame Average: " +  f.Context.ProfilerContext.GetFrameTimeAverage());
#endif
      } catch (Exception exn) {
        LogSimulationException(exn);
      }

      HostProfiler.End();
    }

    public void OnSimulateFinished(DeterministicFrame state) {
      SnapshotsOnSimulateFinished(state);
      InvokeOnSimulateFinished(state);
    }

    public void OnUpdateDone() {
      Frames.Predicted = (Frame)Session.FramePredicted;
      Frames.PredictedPrevious = (Frame)Session.FramePredictedPrevious;
      Frames.Verified = (Frame)Session.FrameVerified;
      Frames.PreviousUpdatePredicted = (Frame)Session.PreviousUpdateFramePredicted;

      var f = (float)(Session.AccumulatedTime / Frames.Predicted.DeltaTime.AsFloat);
      InterpolationFactor = f < 0.0f ? 0.0f : f > 1.0f ? 1.0f : f; // Clamp01

      InvokeOnUpdateView();
      InvokeEvents();
    }

    public void AssignSession(DeterministicSession session) {
      Session = session;

      DeterministicSessionConfig sessionConfig;
      Session.GetLocalConfigs(out sessionConfig, out _);

      // verify player count is in correct range
      if (sessionConfig.PlayerCount < 1 || sessionConfig.PlayerCount > Quantum.Input.MAX_COUNT) {
        throw new Exception(String.Format("Invalid player count {0} (needs to be in 1-{1} range)", sessionConfig.PlayerCount, Quantum.Input.MAX_COUNT));
      }

      // verify all types
      var verifier = new MemoryLayoutVerifier(MemoryLayoutVerifier.Platform ?? new MemoryLayoutVerifier.DefaultPlatform());
      var result = verifier.Verify(_typeRegistry.Types);
      if (result.Count > 0) {
        throw new Exception("MemoryIntegrity Check Failed: " + System.Environment.NewLine + String.Join(System.Environment.NewLine, result.ToArray()));
      } else {
        Log.Info("Memory Integrity Verified");
      }
    }

    public void OnChecksumError(DeterministicTickChecksumError error, DeterministicFrame[] frames) {
      InvokeOnChecksumError(error, frames);
    }

    public void OnChecksumComputed(Int32 frame, ulong checksum) {
      InvokeOnChecksumComputed(frame, checksum);
      ReplayToolsOnChecksumComputed(frame, checksum);
    }

    public void OnSimulationEnd() {
      _context.OnSimulationEnd();
    }

    public void OnSimulationBegin() {
      _polledInputInThisSimulation = false;
      _context.OnSimulationBegin();
    }

    public void OnInputConfirmed(DeterministicFrameInputTemp input) {
      InvokeOnInputConfirmed(input);
      ReplayToolsOnInputConfirmed(input);
    }

    public void OnChecksumErrorFrameDump(int actorId, int frameNumber, DeterministicSessionConfig sessionConfig, byte[] runtimeConfig, byte[] frameData, byte[] extraData) {
      InvokeOnChecksumErrorFrameDump(actorId, frameNumber, sessionConfig, runtimeConfig, frameData, extraData);
    }

    public int GetInputInMemorySize() {
      return sizeof(Input);
    }

    public Int32 GetInputSerializedFixedSize() {
      var stream = new FrameSerializer(DeterministicFrameSerializeMode.Serialize, null, 1024);
      stream.Writing = true;
      stream.InputMode = true;
      Input.Write(stream, new Input());
      return stream.ToArray().Length;
    }

    void InitSystems(DeterministicFrame df) {
      var f = (Frame)df;

      try {
        f.Context.OnFrameSimulationBegin(f);

        // call init on ALL systems
        for (Int32 i = 0; i < _systemsAll.Length; ++i) {
          try {
            _systemsAll[i].OnInit(f);

            if (f.CommitCommandsMode == CommitCommandsModes.InBetweenSystems) {
              f.Unsafe.CommitAllCommands();
            }
          } catch (Exception exn) {
            LogSimulationException(exn);
          }
        }

        // TODO: this seems like a good place to fire OnMapChanged,
        // if we want to do that for the initial map

        // call OnEnabled on all systems which start enabled
        for (Int32 i = 0; i < _systemsAll.Length; ++i) {
          if (_systemsAll[i].StartEnabled) {
            try {
              _systemsAll[i].OnEnabled(f);
              
              if (f.CommitCommandsMode == CommitCommandsModes.InBetweenSystems) {
                f.Unsafe.CommitAllCommands();
              }
            } catch (Exception exn) {
              LogSimulationException(exn);
            }
          }
        }

        f.Context.OnFrameSimulationEnd();
      } catch (Exception e) {
        LogSimulationException(e);
      }

      // invoke events from OnInit/OnEnabled
      InvokeEvents();
    }

    public void DeserializeInputInto(int player, byte[] data, byte* buffer) {
      if (_inputSerializerRead == null) {
        _inputStreamReadZeroArray = new Byte[1024];
        _inputSerializerRead = new FrameSerializer(DeterministicFrameSerializeMode.Serialize, null, new Byte[1024]);
      }

      _inputSerializerRead.Reset();
      _inputSerializerRead.Frame = null;
      _inputSerializerRead.Reading = true;
      _inputSerializerRead.InputMode = true;

      if (data == null || data.Length == 0) {
        _inputSerializerRead.CopyFromArray(_inputStreamReadZeroArray);
      } else {
        _inputSerializerRead.CopyFromArray(data);
      }

      try {
        *(Input*)buffer = Input.Read(_inputSerializerRead);
      } catch (Exception exn) {
        *(Input*)buffer = default;

        // log exception
        Log.Error("Received invalid input data from player {0}, could not deserialize.", player);
        Log.Exception(exn);
      }
    }

    void ApplyInputs(Frame f) {
      for (Int32 i = 0; i < Session.PlayerCount; i++) {
        var raw = f.GetRawInput(i);
        if (raw == null) {
          Log.Error($"Got null input for player {i}");
        } else {
          f.SetPlayerInput(i, *(Input*)raw);
        }
      }
    }

    Boolean ReadInputFromStream(out Input input) {
      try {
        input = Input.Read(_inputSerializerRead);
        return true;
      } catch {
        input = default(Input);
        return false;
      }
    }

    void LogSimulationException(Exception exn) {
      Log.Error("## Simulation Code Threw Exception ##");
      Log.Exception(exn);
    }

    public byte[] GetExtraErrorFrameDumpData() {
      return _context.AssetSerializer.SerializeAssets(new[] { Configurations.Simulation });
    }
  }
}

// Game/QuantumGame.ReplayTools.cs
﻿
namespace Quantum {
  public partial class QuantumGame {

    public InputProvider RecordedInputs { get; private set; }
    public ChecksumFile RecordedChecksums { get; private set; }

    ChecksumFile _checksumsToVerify;

    public static void ExportDatabase(IEnumerable<AssetObject> assets, IAssetSerializer serializer, string folderPath, int serializationBufferSize, string dbExtension = ".json") {

      {
        var filePath = Path.Combine(folderPath, "assetDB" + dbExtension);
        var bytes = serializer.SerializeAssets(assets);
        File.WriteAllBytes(filePath, bytes);
      }

      // Export navmesh files from the memory.
      // They have to reside in the same folder as the database file.
      var navMeshes = assets.OfType<NavMesh>();
      foreach (var n in navMeshes) {
        // Uses it's own binary serialization code.
        var bytestream = new ByteStream(new System.Byte[serializationBufferSize]);
        n.Serialize(bytestream, true);

        var filePath = Path.Combine(folderPath, n.DataFilepath);
        var directoryName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryName)) {
          Directory.CreateDirectory(directoryName);
        }

        File.WriteAllBytes(filePath, bytestream.ToArray());
      }

      // Export mesh triangles from each map
      var maps = assets.OfType<Map>();
      foreach (var map in maps) {
        if (!string.IsNullOrEmpty(map.StaticColliders3DTrianglesBinaryFile)) {
          var bytestream = new ByteStream(new System.Byte[serializationBufferSize]);
          map.SerializeStaticColliderTriangles(bytestream, true);

          var filePath = Path.Combine(folderPath, map.StaticColliders3DTrianglesBinaryFile);
          var directoryName = Path.GetDirectoryName(filePath);
          if (!Directory.Exists(directoryName)) {
            Directory.CreateDirectory(directoryName);
          }

          File.WriteAllBytes(filePath, bytestream.ToArray());
        }
      }
    }


    [Obsolete("Use GetInstantReplaySnapshot(int)")]
    public Frame GetRecordedSnapshot(int frame) {
      return GetInstantReplaySnapshot(frame);
    }

    public Frame GetInstantReplaySnapshot(int frame) {
      if (!_instantReplaySnapshotsRecording) {
        Log.Error("Can't find any recorded snapshots. Use StartRecordingSnapshots to start recording.");
        return null;
      }

      var buffer = (_commonSnapshotInterval > 0 ? _checksumSnapshotBuffer : _instantReplaySnapshotBuffer);
      Assert.Check(buffer != null);

      var result = buffer.Find(frame, DeterministicFrameSnapshotBufferFindMode.ClosestLessThanOrEqual);
      if (result == null) {
        result = buffer.Find(frame, DeterministicFrameSnapshotBufferFindMode.Closest);
        if (result == null) {
          Log.Warn("Unable to find a replay snapshot for frame {0}. No snapshots were saved.", frame);
        } else {
          Log.Warn("Unable to find a replay snapshot for frame {0} or earlier. The closest match is {1}." +
                   "Increase the max replay length.", frame, result.Number);
        }
      }

      return (Frame)result;
    }

    public void GetInstantReplaySnapshots(int startFrame, int endFrame, List<Frame> frames) {
      if (!_instantReplaySnapshotsRecording) {
        Log.Error("Can't find any recorded snapshots. Use StartRecordingSnapshots to start recording.");
        return;
      }

      var buffer = (_commonSnapshotInterval > 0 ? _checksumSnapshotBuffer : _instantReplaySnapshotBuffer);
      Assert.Check(buffer != null);

      var firstFrame = buffer.Find(startFrame, DeterministicFrameSnapshotBufferFindMode.ClosestLessThanOrEqual);
      var minFrameNumber = firstFrame?.Number ?? startFrame;

      foreach (Frame frame in buffer.Data) {
        if (frame == null) {
          continue;
        }

        if (frame.Number >= minFrameNumber && frame.Number <= endFrame)
          frames.Add(frame);
      }
    }

    public ReplayFile CreateSavegame() {
      if (Frames.Verified == null) {
        Log.Error("Cannot create a savegame. Frames verified not found.");
        return null;
      }

      return new ReplayFile {
        DeterministicConfig = Frames.Verified.SessionConfig,
        RuntimeConfig = Frames.Verified.RuntimeConfig,
        InputHistory = null,
        Length = Frames.Verified.Number,
        Frame = Frames.Verified.Serialize(DeterministicFrameSerializeMode.Serialize)
      };
    }

    public ReplayFile GetRecordedReplay() {
      if (Frames.Verified == null) {
        Log.Error("Cannot create a replay. Frames current or verified are not valid, yet.");
        return null;
      }

      if (RecordedInputs == null) {
        Log.Error("Cannot create a replay, because no recorded input was found. Use StartRecordingInput to start recording or setup RecordingFlags.");
        return null;
      }

      var verifiedFrame = Frames.Verified.Number;

      return new ReplayFile {
        DeterministicConfig = Frames.Verified.SessionConfig,
        RuntimeConfig = Frames.Verified.RuntimeConfig,
        InputHistory = RecordedInputs.ExportToList(verifiedFrame),
        Length = verifiedFrame
      };
    }


    private void ReplayToolsOnInputConfirmed(DeterministicFrameInputTemp input) {
      if (RecordedInputs == null)
        return;
      RecordedInputs.OnInputConfirmed(this, input);
    }

    private void ReplayToolsOnChecksumComputed(Int32 frame, ulong checksum) {
      if (RecordedChecksums != null) {
        RecordedChecksums.RecordChecksum(this, frame, checksum);
      }
      if (_checksumsToVerify != null) {
        _checksumsToVerify.VerifyChecksum(this, frame, checksum);
      }
    }

    public void StartRecordingInput(Int32? startFrame = null) {
      if (Session == null) {
        Log.Error("Can't start input recording, because the session is invalid. Wait for the OnGameStart callback.");
        return;
      }
      if (RecordedInputs == null) {
        if (startFrame.HasValue) {
          RecordedInputs = new InputProvider(Session.SessionConfig.PlayerCount, startFrame.Value, 60 * 60, 0);
        } else {
          // start frame is the session RollbackWindow
          RecordedInputs = new InputProvider(Session.SessionConfig);
        }
        Log.Info("QuantumGame.ReplayTools: Input recording started");
      }
    }

    public void StartRecordingChecksums() {
      if (RecordedChecksums == null) {
        RecordedChecksums = new ChecksumFile();
        Log.Info("QuantumGame.ReplayTools: Checksum recording started");
      }
    }

    public void StartVerifyingChecksums(ChecksumFile checksums) {
      if (_checksumsToVerify == null) {
        _checksumsToVerify = checksums;
        Log.Info("QuantumGame.ReplayTools: Checksum verification started");
      }
    }

    public void StartRecordingInstantReplaySnapshots() {
      if (_instantReplaySnapshotsRecording) {
        return;
      }

      if (InstantReplayConfig.LenghtSeconds <= 0 || InstantReplayConfig.SnapshotsPerSecond <= 0) {
        Assert.Check(_instantReplaySnapshotBuffer == null);
        Assert.Check(_commonSnapshotInterval <= 0);
        Log.Error($"Can't start recording replay snapshots with these settings: {InstantReplayConfig}");
        return;
      }

      _instantReplaySnapshotsRecording = true;
    }

    [Obsolete("Use StartRecordingInstantReplaySnapshots() instead and StartParameters properties instead.")]
    public void StartRecordingSnapshots(float bufferSizeSec, int snapshotFrequencyPerSec) {
    }
  }
}

// Game/TriggeredSetPool.cs
﻿
namespace Quantum {
  public class TriggeredSetPool {
    Stack<Dictionary<Int32, Boolean>[]> _triggeredPool = new Stack<Dictionary<Int32, Boolean>[]>();

    public void Init(Int32 size) {
      while (_triggeredPool.Count < size) {
        _triggeredPool.Push(CreateNew());
      }
    }

    public Dictionary<Int32, Boolean>[] Alloc() {
      Dictionary<Int32, Boolean>[] set;

      if (_triggeredPool.Count > 0) {
        set = _triggeredPool.Pop();
      } else {
        set = CreateNew();
      }

      return set;
    }

    public void Free(Dictionary<Int32, Boolean>[] set) {
      if (set != null) {
        for (Int32 i = 0; i < Frame.FrameEvents.EVENT_TYPE_COUNT; ++i) {
          set[i].Clear();
        }

        // push on pool
        _triggeredPool.Push(set);
      }
    }

    Dictionary<Int32, Boolean>[] CreateNew() {
      var set = new Dictionary<Int32, Boolean>[Frame.FrameEvents.EVENT_TYPE_COUNT];

      for (Int32 i = 0; i < Frame.FrameEvents.EVENT_TYPE_COUNT; ++i) {
        set[i] = new Dictionary<Int32, Boolean>();
      }

      return set;
    }
  }
}

// Game/QuantumGame.EventDispatcher.cs
﻿
namespace Quantum {
  public partial class QuantumGame {
    TriggeredSetPool _triggeredSetPool;
    Dictionary<Int32, Dictionary<Int32, Boolean>[]> _eventsTriggered;
    List<Int32> _eventFramesVerified;

    void InitEventInvoker(Int32 size) {
      // allocate dictionary with pre-defined capacity
      _eventsTriggered = new Dictionary<Int32, Dictionary<Int32, Boolean>[]>(size);
      _eventFramesVerified = new List<int>(size);

      // init trigger set pool with empty hashsets
      _triggeredSetPool = new TriggeredSetPool();
      _triggeredSetPool.Init(size);
    }

    void RaiseEvent(EventBase evnt) {
      try {
        evnt.Game = this;
        _eventDispatcher?.Publish(evnt);
      } catch (Exception exn) {
        Log.Error("## Event Callback Threw Exception ##");
        Log.Exception(exn);
      }
    }

    Dictionary<Int32, Boolean>[] GetTriggered(int frame) {
      if (_eventsTriggered.TryGetValue(frame, out var triggered) == false) {
        _eventsTriggered.Add(frame, triggered = _triggeredSetPool.Alloc());
      }

      return triggered;
    }

    void InvokeEvents() {
      while (_context.Events.Count > 0) {
        var head = _context.Events.PopHead();
        _context.ReleaseEvent(head);

        if (head.Synced) {
          if (Session.IsFrameVerified(head.Tick)) {
            RaiseEvent(head);
          }
        } else {
          // calculate hash code
          var hash = head.GetHashCode();
          var triggered = GetTriggered(head.Tick);

          // if this was already raised, do nothing
          if (triggered[head.Id].ContainsKey(hash) == false) {
            // dont trigger this again
            triggered[head.Id].Add(hash, false);

            // trigger event
            RaiseEvent(head);
          }

          // if frame is verified, CONFIRM the event in the temp collection of hashes
          if (Session.IsFrameVerified(head.Tick)) {
            // confirm this event is definitive...
            triggered[head.Id][hash] = true;
          }
        }
      }

      // temp collection we use to keep track of verified frames
      _eventFramesVerified.Clear();

      // find any verified triggered sets
      foreach (var kvp in _eventsTriggered) {
        if (Session.IsFrameVerified(kvp.Key)) {
          _eventFramesVerified.Add(kvp.Key);
        }
      }

      // invoke confirmed/canceled event callbacks
      for (int i = 0; i < _eventFramesVerified.Count; ++i) {
        var frame = _eventFramesVerified[i];
        var triggered = _eventsTriggered[frame];

        _eventsTriggered.Remove(frame);

        for (int eventTypeID = 0; eventTypeID < triggered.Length; eventTypeID++) {
          var eventCollection = triggered[eventTypeID];
          foreach (var kvp in eventCollection) {
            var hash = kvp.Key;
            var key = new EventKey(frame, eventTypeID, hash);
            var confirmed = eventCollection[hash];
            InvokeOnEvent(key, confirmed);
          }
        }

        _triggeredSetPool.Free(triggered);
      }
    }
  }
}

// Game/QuantumGameCallbacks.cs
﻿
namespace Quantum {

  public enum CallbackId {
    PollInput,
    GameStarted,
    GameResynced,
    GameDestroyed,
    UpdateView,
    SimulateFinished,
    EventCanceled,
    EventConfirmed,
    ChecksumError,
    ChecksumErrorFrameDump,
    InputConfirmed,
    ChecksumComputed,
    UserCallbackIdStart,
  }

  public sealed class CallbackPollInput : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.PollInput;
    internal CallbackPollInput(QuantumGame game) : base(ID, game) { }

    public Int32 Frame;
    public Int32 Player;

    public void SetInput(Input input, DeterministicInputFlags flags) {
      IsInputSet = true;
      Input = input;
      Flags = flags;
    }

    public void SetInput(QTuple<Input, DeterministicInputFlags> input) {
      SetInput(input.Item0, input.Item1);
    }

    public bool IsFirstInThisUpdate { get; internal set; }
    public bool IsInputSet { get; internal set; }
    public Input Input { get; private set; }
    public DeterministicInputFlags Flags { get; private set; }
  }

  public sealed class CallbackGameStarted : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.GameStarted;
    internal CallbackGameStarted(QuantumGame game) : base(ID, game) { }
  }

  public sealed class CallbackGameResynced : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.GameResynced;
    internal CallbackGameResynced(QuantumGame game) : base(ID, game) { }
  }

  public sealed class CallbackGameDestroyed : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.GameDestroyed;
    internal CallbackGameDestroyed(QuantumGame game) : base(ID, game) { }
  }

  public sealed class CallbackUpdateView : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.UpdateView;
    internal CallbackUpdateView(QuantumGame game) : base(ID, game) { }
  }

  public sealed class CallbackSimulateFinished : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.SimulateFinished;
    internal CallbackSimulateFinished(QuantumGame game) : base(ID, game) { }

    public Frame Frame;
  }

  public sealed class CallbackEventCanceled : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.EventCanceled;
    internal CallbackEventCanceled(QuantumGame game) : base(ID, game) { }

    public EventKey EventKey;
  }

  public sealed class CallbackEventConfirmed : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.EventConfirmed;
    internal CallbackEventConfirmed(QuantumGame game) : base(ID, game) { }

    public EventKey EventKey;
  }

  public sealed class CallbackChecksumError : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.ChecksumError;
    internal CallbackChecksumError(QuantumGame game) : base(ID, game) { }

    public DeterministicTickChecksumError Error;
    internal DeterministicFrame[] _rawFrames;
    internal Frame[] _convertedFrame;

    public int FrameCount => Frames.Length;
    public Frame GetFrame(int index) => (Frame)Frames[index];

    public Frame[] Frames {
      get {
        if (_convertedFrame == null) {
          _convertedFrame = new Frame[_rawFrames.Length];
          for (int i = 0; i < _rawFrames.Length; ++i) {
            _convertedFrame[i] = (Frame)_rawFrames[i];
          }
        }
        return _convertedFrame;
      }
    }
  }

  public sealed class CallbackChecksumErrorFrameDump : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.ChecksumErrorFrameDump;
    internal CallbackChecksumErrorFrameDump(QuantumGame game) : base(ID, game) { }

    public Int32 ActorId;
    public Int32 FrameNumber;
    public Byte[] FrameData;
    public Byte[] RuntimeConfigBytes;
    public Byte[] SimulationConfigBytes;
    public DeterministicSessionConfig SessionConfig;

    private Frame _frameToOverride;
    private IAssetSerializer _assetSerializer;

    private Byte[] _overridenFrameData;
    private SimulationConfig _overridenSimulationConfig;
    private DeterministicSessionConfig _overridenSessionConfig;
    private RuntimeConfig _overridenRuntimeConfig;

    private QTuple<bool, string> _frameDump;
    private QTuple<bool, Frame> _frame;
    private QTuple<bool, RuntimeConfig> _runtimeConfig;
    private QTuple<bool, SimulationConfig> _simulationConfig;

    internal void Clear() {

      try {
        if (_overridenRuntimeConfig != null) {
          _frameToOverride.RuntimeConfig = _overridenRuntimeConfig;
        }
        if (_overridenSessionConfig != null) {
          _frameToOverride.SessionConfig = _overridenSessionConfig;
        }

        if (_overridenSimulationConfig != null) {
          _frameToOverride.SimulationConfig = _overridenSimulationConfig;
        }

        if (_overridenFrameData != null) {
          _frameToOverride.Deserialize(_overridenFrameData);
        }
      } finally {
        _frameToOverride = null;
        _overridenFrameData = null;
        _overridenSimulationConfig = null;
        _overridenSessionConfig = null;
        _overridenRuntimeConfig = null;

        _runtimeConfig = default;
        _simulationConfig = default;
        _frame = default;
        _frameDump = default;
        SessionConfig = null;
      }
    }

    public Frame Frame {
      get {
        if (!_frame.Item0) {
          _frame = QTuple.Create(true, (Frame)null);
          if (_frameToOverride != null) {
            var originalFrameData = _frameToOverride.Serialize(DeterministicFrameSerializeMode.Serialize);
            try {
              _frameToOverride.Deserialize(FrameData);
              _frame = QTuple.Create(true, _frameToOverride);
              _overridenFrameData = originalFrameData;
            } catch (System.Exception) {
              // revert to the old data
              _frameToOverride.Deserialize(originalFrameData);
            }

            _overridenRuntimeConfig = _frameToOverride.RuntimeConfig;
            _overridenSessionConfig = _frameToOverride.SessionConfig;
            _overridenSimulationConfig = _frameToOverride.SimulationConfig;
            _frameToOverride.SessionConfig = SessionConfig;

            if (RuntimeConfig != null) {
              _frameToOverride.RuntimeConfig = RuntimeConfig;
            }

            if (SimulationConfig != null) {
              _frameToOverride.SimulationConfig = SimulationConfig;
            }
          }
        }
        return _frame.Item1;
      }
    }

    public string FrameDump {
      get {
        if (!_frameDump.Item0) {
          if (Frame != null) {
            int dumpFlags = Frame.DumpFlag_NoHeap;
            if (RuntimeConfig == null) {
              dumpFlags |= Frame.DumpFlag_NoRuntimeConfig;
            }
            if (SimulationConfig == null) {
              dumpFlags |= Frame.DumpFlag_NoSimulationConfig;
            }
            _frameDump = QTuple.Create(true, Frame.DumpFrame(dumpFlags));
          } else {
            unsafe {
              byte[] actualData = FrameData;
              bool wasCompressed = false;
              try {
                actualData = ByteUtils.GZipDecompressBytes(FrameData);
                wasCompressed = true;
              } catch { }

              fixed (byte* p = actualData) {
                var printer = new FramePrinter();
                printer.AddLine($"#### RAW FRAME DUMP (was compressed: {wasCompressed}) ####");
                printer.ScopeBegin();
                UnmanagedUtils.PrintBytesHex(p, FrameData.Length, 32, printer);
                printer.ScopeEnd();
                _frameDump = QTuple.Create(true, printer.ToString());
              }
            }
          }
        }

        return _frameDump.Item1;
      }
    }


    public RuntimeConfig RuntimeConfig {
      get {
        if (!_runtimeConfig.Item0) {
          try {
            _runtimeConfig = QTuple.Create(true, RuntimeConfig.FromByteArray(RuntimeConfigBytes));
          } catch (Exception ex) {
            Log.Exception(ex);
            _runtimeConfig = QTuple.Create(true, (RuntimeConfig)null);
          }
        }
        return _runtimeConfig.Item1;
      }
    }

    public SimulationConfig SimulationConfig {
      get {
        if (_simulationConfig.Item0) {
          try {
            _simulationConfig = QTuple.Create(true, _assetSerializer.DeserializeAssets(SimulationConfigBytes).Cast<SimulationConfig>().Single());
          } catch (Exception ex) {
            Log.Exception(ex);
            _simulationConfig = QTuple.Create(true, (SimulationConfig)null);
          }
        }
        return _simulationConfig.Item1;
      }
    }

    internal void Init(Frame frame, IAssetSerializer assetSerializer) {
      _frameToOverride = frame;
      _assetSerializer = assetSerializer;
    }
  }

  public sealed class CallbackInputConfirmed : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.InputConfirmed;
    internal CallbackInputConfirmed(QuantumGame game) : base(ID, game) { }
    public DeterministicFrameInputTemp Input;
  }

  public sealed class CallbackChecksumComputed : QuantumGame.CallbackBase {
    public new const Int32 ID = (int)CallbackId.ChecksumComputed;
    internal CallbackChecksumComputed(QuantumGame game) : base(ID, game) { }

    public Int32 Frame;
    public UInt64 Checksum;
  }

  partial class QuantumGame {

    public class CallbackBase : Quantum.CallbackBase {
      public new QuantumGame Game {
        get => (QuantumGame)base.Game;
        set => base.Game = value;
      }

      public CallbackBase(int id, QuantumGame game) : base(id, game) {
      }


      public static Type GetCallbackType(CallbackId id) {
        switch (id) {
          case CallbackId.ChecksumComputed: return typeof(CallbackChecksumComputed);
          case CallbackId.ChecksumError: return typeof(CallbackChecksumError);
          case CallbackId.ChecksumErrorFrameDump: return typeof(CallbackChecksumErrorFrameDump);
          case CallbackId.EventCanceled: return typeof(CallbackEventCanceled);
          case CallbackId.EventConfirmed: return typeof(CallbackEventConfirmed);
          case CallbackId.GameDestroyed: return typeof(CallbackGameDestroyed);
          case CallbackId.GameStarted: return typeof(CallbackGameStarted);
          case CallbackId.InputConfirmed: return typeof(CallbackInputConfirmed);
          case CallbackId.PollInput: return typeof(CallbackPollInput);
          case CallbackId.SimulateFinished: return typeof(CallbackSimulateFinished);
          case CallbackId.UpdateView: return typeof(CallbackUpdateView);
          default: throw new ArgumentOutOfRangeException(nameof(id));
        }
      }
    }

    // callback objects
    private CallbackChecksumComputed _callbackChecksumComputed;
    private CallbackChecksumError _callbackChecksumError;
    private CallbackChecksumErrorFrameDump _callbackChecksumErrorFrameDump;
    private CallbackEventCanceled _callbackEventCanceled;
    private CallbackEventConfirmed _callbackEventConfirmed;
    private CallbackGameDestroyed _callbackGameDestroyed;
    private CallbackGameStarted _callbackGameStarted;
    private CallbackGameResynced _callbackGameResynced;
    private CallbackInputConfirmed _callbackInputConfirmed;
    private CallbackPollInput _callbackPollInput;
    private CallbackSimulateFinished _callbackSimulateFinished;
    private CallbackUpdateView _callbackUpdateView;

    public void InitCallbacks() {
      _callbackChecksumComputed = new CallbackChecksumComputed(this);
      _callbackChecksumError = new CallbackChecksumError(this);
      _callbackChecksumErrorFrameDump = new CallbackChecksumErrorFrameDump(this);
      _callbackEventCanceled = new CallbackEventCanceled(this);
      _callbackEventConfirmed = new CallbackEventConfirmed(this);
      _callbackGameDestroyed = new CallbackGameDestroyed(this);
      _callbackGameStarted = new CallbackGameStarted(this);
      _callbackGameResynced = new CallbackGameResynced(this);
      _callbackInputConfirmed = new CallbackInputConfirmed(this);
      _callbackPollInput = new CallbackPollInput(this);
      _callbackSimulateFinished = new CallbackSimulateFinished(this);
      _callbackUpdateView = new CallbackUpdateView(this);
    }

    public void InvokeOnGameEnded() {
      // not implemented 
    }

    public void InvokeOnDestroy() {
      try {
        _callbackDispatcher?.Publish(_callbackGameDestroyed);
      } catch (Exception ex) {
        Log.Exception(ex);

      }
    }

    void InvokeOnGameStart() {
      try {
        _callbackDispatcher?.Publish(_callbackGameStarted);
      } catch (Exception ex) {
        Log.Exception(ex);
      }
    }

    void InvokeOnGameResync() {
      try {
        _callbackDispatcher?.Publish(_callbackGameResynced);
      } catch (Exception ex) {
        Log.Exception(ex);
      }
    }


    QTuple<Input, DeterministicInputFlags> InvokeOnPollInput(int frame, int player, bool isFirstInThisUpdate) {

      try {
        _callbackPollInput.IsInputSet = false;
        _callbackPollInput.Frame = frame;
        _callbackPollInput.Player = player;
        _callbackPollInput.IsFirstInThisUpdate = isFirstInThisUpdate;
        _callbackDispatcher?.Publish(_callbackPollInput);
        if (_callbackPollInput.IsInputSet) {
          return QTuple.Create(_callbackPollInput.Input, _callbackPollInput.Flags);
        }
        return default;
      } catch (Exception ex) {
        Log.Exception(ex);
        return default;
      }
    }

    void InvokeOnUpdateView() {
      HostProfiler.Start("QuantumGame.InvokeOnUpdateView");
      try {
        _callbackDispatcher?.Publish(_callbackUpdateView);
      } catch (Exception ex) {
        Log.Exception(ex);
      }
      HostProfiler.End();
    }

    public void InvokeOnSimulateFinished(DeterministicFrame state) {
      HostProfiler.Start("QuantumGame.InvokeOnSimulateFinished");
      try {
        _callbackSimulateFinished.Frame = (Frame)state;
        _callbackDispatcher?.Publish(_callbackSimulateFinished);
      } catch (Exception ex) {
        Log.Exception(ex);
      }

      _callbackSimulateFinished.Frame = null;
      HostProfiler.End();
    }

    public void InvokeOnChecksumError(DeterministicTickChecksumError error, DeterministicFrame[] frames) {
      try {
        _callbackChecksumError.Error = error;
        _callbackChecksumError._rawFrames = frames;
        _callbackChecksumError._convertedFrame = null;
        try {
          _callbackDispatcher?.Publish(_callbackChecksumError);
        } finally {
          _callbackChecksumError._rawFrames = null;
          _callbackChecksumError._convertedFrame = null;
        }
      } catch (Exception ex) {
        Log.Exception(ex);
      }
    }

    public void InvokeOnChecksumComputed(Int32 frame, ulong checksum) {
      try {
        _callbackChecksumComputed.Frame = frame;
        _callbackChecksumComputed.Checksum = checksum;
        _callbackDispatcher?.Publish(_callbackChecksumComputed);
      } catch (Exception ex) {
        Log.Exception(ex);
      }
    }

    public void InvokeOnInputConfirmed(DeterministicFrameInputTemp input) {
      try {
        _callbackInputConfirmed.Input = input;
        try {
          _callbackDispatcher?.Publish(_callbackInputConfirmed);
        } finally {
          _callbackInputConfirmed.Input = default;
        }
      } catch (Exception ex) {
        Log.Exception(ex);
      }
    }

    public void InvokeOnChecksumErrorFrameDump(Int32 actorId, Int32 frameNumber, DeterministicSessionConfig sessionConfig, byte[] runtimeConfig, byte[] frameData, byte[] extraData) {
      HostProfiler.Start("QuantumGame.InvokeOnChecksumErrorFrameDump");
      try {

        // find the frame that's going to be overwritten: 
        Frame frameToOverwrite = null;

        if (_checksumSnapshotBuffer?.Capacity > 0) {
          if (_checksumSnapshotBuffer.Count == 0) {
            _checksumSnapshotBuffer.PushBack(Frames.Verified, this, _context);
          }
          frameToOverwrite = (Frame)_checksumSnapshotBuffer.PeekBack();
        } else {
          // TODO: use replay buffer maybe? or one of predicted?
          Log.Warn("Unable to acquire a frame to decode the snapshot. The snapshot will appear as raw binary data. Increase ChecksumFrameBufferSize.");
        }

        try {
          _callbackChecksumErrorFrameDump.Init(frameToOverwrite, _context.AssetSerializer);
          _callbackChecksumErrorFrameDump.ActorId = actorId;
          _callbackChecksumErrorFrameDump.FrameNumber = frameNumber;
          _callbackChecksumErrorFrameDump.FrameData = frameData;
          _callbackChecksumErrorFrameDump.SessionConfig = sessionConfig;
          _callbackChecksumErrorFrameDump.RuntimeConfigBytes = runtimeConfig;
          _callbackChecksumErrorFrameDump.SimulationConfigBytes = extraData;

          _callbackDispatcher?.Publish(_callbackChecksumErrorFrameDump);

        } finally {
          _callbackChecksumErrorFrameDump.Clear();
        }
      } catch (Exception ex) {
        HostProfiler.End();
        Log.Exception(ex);
      }
    }

    private void InvokeOnEvent(EventKey key, bool confirmed) {
      try {
        if (confirmed) {
          _callbackEventConfirmed.EventKey = key;
          _callbackDispatcher?.Publish(_callbackEventConfirmed);
        } else {
          // call event cancelation, passing: game (this), frame (f), event hash...
          // also pass the index from eventCollection (trhis is the event type ID);
          _callbackEventCanceled.EventKey = key;
          _callbackDispatcher?.Publish(_callbackEventCanceled);
        }
      } catch (Exception ex) {
        Log.Exception(ex);
      }
    }
  }
}

// Replay/DotNetTaskRunner.cs
﻿
namespace Quantum {
  public class DotNetTaskRunner : IDeterministicPlatformTaskRunner {
    int _length;
    bool[] _done = new bool[128];
    public void Schedule(Action[] delegates) {
      // store how many we're executing
      _length = delegates.Length;
      // clear current state
      Array.Clear(_done, 0, _done.Length);
      // barrier this
      Thread.MemoryBarrier();
      // queue work
      for (int i = 0; i < delegates.Length; ++i) {
        ThreadPool.QueueUserWorkItem(Wrap(i, delegates[i]));
      }
    }
    public void WaitForComplete() {
      throw new NotImplementedException();
    }
    public bool PollForComplete() {
      for (int i = 0; i < _length; ++i) {
        if (Volatile.Read(ref _done[i]) == false) {
          return false;
        }
      }
      return true;
    }
    WaitCallback Wrap(int index, Action callback) {
      return _ => {
        try {
          Photon.Deterministic.Assert.Check(Volatile.Read(ref _done[index]) == false);
          callback();
        } catch (Exception exn) {
          Log.Exception(exn);
        } finally {
          Volatile.Write(ref _done[index], true);
        }
      };
    }
  }
}

// Replay/FlatEntityPrototypeContainer.cs
﻿
namespace Quantum.Prototypes {
  [Serializable]
  public partial class FlatEntityPrototypeContainer {
    [FixedArray(0, 1)] public List<CharacterController2D_Prototype> CharacterController2D;
    [FixedArray(0, 1)] public List<CharacterController3D_Prototype> CharacterController3D;
    [FixedArray(0, 1)] public List<NavMeshAvoidanceAgent_Prototype> NavMeshAvoidanceAgent;
    [FixedArray(0, 1)] public List<NavMeshAvoidanceObstacle_Prototype> NavMeshAvoidanceObstacle;
    [FixedArray(0, 1)] public List<NavMeshPathfinder_Prototype> NavMeshPathfinder;
    [FixedArray(0, 1)] public List<NavMeshSteeringAgent_Prototype> NavMeshSteeringAgent;
    [FixedArray(0, 1)] public List<PhysicsBody2D_Prototype> PhysicsBody2D;
    [FixedArray(0, 1)] public List<PhysicsBody3D_Prototype> PhysicsBody3D;
    [FixedArray(0, 1)] public List<PhysicsCollider2D_Prototype> PhysicsCollider2D;
    [FixedArray(0, 1)] public List<PhysicsCollider3D_Prototype> PhysicsCollider3D;
    [FixedArray(0, 1)] public List<PhysicsCallbacks2D_Prototype> PhysicsCallbacks2D;
    [FixedArray(0, 1)] public List<PhysicsCallbacks3D_Prototype> PhysicsCallbacks3D;
    [FixedArray(0, 1)] public List<Transform2D_Prototype> Transform2D;
    [FixedArray(0, 1)] public List<Transform2DVertical_Prototype> Transform2DVertical;
    [FixedArray(0, 1)] public List<Transform3D_Prototype> Transform3D;
    [FixedArray(0, 1)] public List<View_Prototype> View;

    public void Collect(List<ComponentPrototype> target) {
      Collect(CharacterController2D, target);
      Collect(CharacterController3D, target);
      Collect(NavMeshAvoidanceAgent, target);
      Collect(NavMeshAvoidanceObstacle, target);
      Collect(NavMeshPathfinder, target);
      Collect(NavMeshSteeringAgent, target);
      Collect(PhysicsBody2D, target);
      Collect(PhysicsBody3D, target);
      Collect(PhysicsCollider2D, target);
      Collect(PhysicsCollider3D, target);
      Collect(PhysicsCallbacks2D, target);
      Collect(PhysicsCallbacks3D, target);
      Collect(Transform2D, target);
      Collect(Transform2DVertical, target);
      Collect(Transform3D, target);
      Collect(View, target);
      CollectGen(target);
    }

    public unsafe partial class StoreVisitor : ComponentPrototypeVisitor {
      public FlatEntityPrototypeContainer Storage;
      public override void Visit(CharacterController2D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.CharacterController2D);
      }
      public override void Visit(CharacterController3D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.CharacterController3D);
      }
      public override void Visit(NavMeshAvoidanceAgent_Prototype prototype) {
        Storage.Store(prototype, ref Storage.NavMeshAvoidanceAgent);
      }
      public override void Visit(NavMeshAvoidanceObstacle_Prototype prototype) {
        Storage.Store(prototype, ref Storage.NavMeshAvoidanceObstacle);
      }
      public override void Visit(NavMeshPathfinder_Prototype prototype) {
        Storage.Store(prototype, ref Storage.NavMeshPathfinder);
      }
      public override void Visit(NavMeshSteeringAgent_Prototype prototype) {
        Storage.Store(prototype, ref Storage.NavMeshSteeringAgent);
      }
      public override void Visit(PhysicsBody2D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.PhysicsBody2D);
      }
      public override void Visit(PhysicsBody3D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.PhysicsBody3D);
      }
      public override void Visit(PhysicsCollider2D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.PhysicsCollider2D);
      }
      public override void Visit(PhysicsCollider3D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.PhysicsCollider3D);
      }
      public override void Visit(PhysicsCallbacks2D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.PhysicsCallbacks2D);
      }
      public override void Visit(PhysicsCallbacks3D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.PhysicsCallbacks3D);
      }
      public override void Visit(Transform2D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.Transform2D);
      }
      public override void Visit(Transform2DVertical_Prototype prototype) {
        Storage.Store(prototype, ref Storage.Transform2DVertical);
      }
      public override void Visit(Transform3D_Prototype prototype) {
        Storage.Store(prototype, ref Storage.Transform3D);
      }
      public override void Visit(View_Prototype prototype) {
        Storage.Store(prototype, ref Storage.View);
      }
    }

    partial void CollectGen(List<ComponentPrototype> target);

    private void Collect<TPrototype>(List<TPrototype> source, List<ComponentPrototype> destination) where TPrototype : ComponentPrototype {
      if (source == null)
        return;
      for (int i = 0; i < source.Count; ++i)
        destination.Add(source[i]);
    }

    private void Store<T>(T value, ref List<T> destination) { 
      Assert.Check(value.GetType() == typeof(T));
      if (destination == null)
        destination = new List<T>(1);
      destination.Add(value);
    }
  }
}


// Replay/JsonAssetSerializerBase.cs
﻿
namespace Quantum {
  public abstract class JsonAssetSerializerBase : IAssetSerializer {
    private List<ComponentPrototype> _prototypeBuffer = new List<ComponentPrototype>();

    public bool IsPrettyPrintEnabled { get; set; } = false;

    public byte[] SerializeReplay(ReplayFile replay) {
      var json = ToJson(replay);
      return Encoding.UTF8.GetBytes(json);
    }

    public ReplayFile DeserializeReplay(byte[] data) {
      var json = Encoding.UTF8.GetString(data);
      return (ReplayFile)FromJson(json, typeof(ReplayFile));
    }

    public byte[] SerializeChecksum(ChecksumFile checksums) {
      var json = ToJson(checksums);
      return Encoding.UTF8.GetBytes(json);
    }

    public ChecksumFile DeserializeChecksum(byte[] data) {
      var json = Encoding.UTF8.GetString(data);
      return (ChecksumFile)FromJson(json, typeof(ChecksumFile));
    }

    public byte[] SerializeAssets(IEnumerable<AssetObject> assets) {
      FlatDatabaseFile db = new FlatDatabaseFile();
      List<UserAssetSurrogate> userAssets = new List<UserAssetSurrogate>();

      var visitor = new AssetVisitor() { Storage = db };

      foreach (var asset in assets) {
        if (asset is IBuiltInAssetObject) {
          ((IBuiltInAssetObject)asset).Dispatch(visitor);
        } else {
          var surrogate = new UserAssetSurrogate() {
            Type = asset.GetType().AssemblyQualifiedName,
            Json = ToJson(asset),
          };
          userAssets.Add(surrogate);
        }
      }

      db.UserAssets = userAssets;

      var json = ToJson(db);
      return Encoding.UTF8.GetBytes(json);
    }

    public string SerializeAssetReadable(AssetObject asset) {

      object objectToSerialize = asset;
      if ( asset is EntityPrototype ep ) {
        objectToSerialize = CreateSurrogate(ep);
      } else if ( asset is Map map) {
        objectToSerialize = CreateSurrogate(map);
      }

      return ToJson(objectToSerialize);
    }


    public IEnumerable<AssetObject> DeserializeAssets(byte[] bytes) {
      var json = Encoding.UTF8.GetString(bytes);
      var db = (FlatDatabaseFile)FromJson(json, typeof(FlatDatabaseFile));

      List<AssetObject> result = new List<AssetObject>();
      Collect(db.CharacterController2DConfig, result);
      Collect(db.CharacterController3DConfig, result);
      Collect(db.EntityView, result);
      Collect(db.NavMesh, result);
      Collect(db.NavMeshAgentConfig, result);
      Collect(db.PhysicsMaterial, result);
      Collect(db.PolygonCollider, result);
      Collect(db.TerrainCollider, result);

      if (db.EntityPrototype != null) {
        foreach (var surrogate in db.EntityPrototype) {
          result.Add(CreateFromSurrogate(surrogate));
        }
      }

      if (db.Map != null) {
        foreach (var surrogate in db.Map) {
          result.Add(CreateFromSurrogate(surrogate));
        }
      }

      if (db.UserAssets != null) {
        foreach (var surrogate in db.UserAssets) {
          var type = Type.GetType(surrogate.Type);
          var asset = (AssetObject)FromJson(surrogate.Json, type);
          result.Add(asset);
        }
      }

      return result;
    }

    protected void Collect<AssetType>(List<AssetType> source, List<AssetObject> destination) where AssetType : AssetObject {
      if (source == null)
        return;
      for (int i = 0; i < source.Count; ++i)
        destination.Add(source[i]);
    }

    protected abstract object FromJson(string json, Type type);

    protected abstract string ToJson(object obj);

    private static EntityPrototypeSurrogate CreateSurrogate(EntityPrototype asset) {
      var visitor = new FlatEntityPrototypeContainer.StoreVisitor() {
        Storage = new FlatEntityPrototypeContainer()
      };

      foreach (var prototype in asset.Container.Components) {
        prototype.Dispatch(visitor);
      }

      return new EntityPrototypeSurrogate() {
        Identifier = asset.Identifier,
        Container = visitor.Storage
      };
    }

    private static MapSurrogate CreateSurrogate(Map asset) {
      var mapEntities = new FlatEntityPrototypeContainer[asset.MapEntities.Length];

      var visitor = new FlatEntityPrototypeContainer.StoreVisitor();

      for (int i = 0; i < mapEntities.Length; ++i) {
        visitor.Storage = mapEntities[i] = new FlatEntityPrototypeContainer();
        foreach (var prototype in asset.MapEntities[i].Components) {
          prototype.Dispatch(visitor);
        }
      }

      return new MapSurrogate() {
        Map = asset,
        MapEntities = mapEntities,
      };
    }

    private EntityPrototype CreateFromSurrogate(EntityPrototypeSurrogate surrogate) {
      try {
        Assert.Check(_prototypeBuffer.Count == 0);
        surrogate.Container.Collect(_prototypeBuffer);
        return new EntityPrototype() {
          Identifier = surrogate.Identifier,
          Container = new EntityPrototypeContainer() {
            Components = _prototypeBuffer.ToArray()
          }
        };
      } finally {
        _prototypeBuffer.Clear();
      }
    }

    private Map CreateFromSurrogate(MapSurrogate surrogate) {
      Assert.Check(_prototypeBuffer.Count == 0);

      var entityCount = surrogate.MapEntities.Length;
      var mapEntities = new EntityPrototypeContainer[entityCount];

      for (int i = 0; i < entityCount; ++i) {
        try {
          surrogate.MapEntities[i].Collect(_prototypeBuffer);
          mapEntities[i] = new EntityPrototypeContainer() {
            Components = _prototypeBuffer.ToArray()
          };
        } finally {
          _prototypeBuffer.Clear();
        }
      }

      var map = surrogate.Map;
      map.MapEntities = mapEntities;
      return map;
    }

    [Serializable]
    public class EntityPrototypeSurrogate {
      public FlatEntityPrototypeContainer Container;
      public AssetObjectIdentifier Identifier;
    }

    [Serializable]
    public class MapSurrogate {
      public Map Map;
      public FlatEntityPrototypeContainer[] MapEntities;
    }

    [Serializable]
    public class UserAssetSurrogate {
      public string Json;
      public string Type;
    }

    private class AssetVisitor : IAssetObjectVisitor {
      public FlatDatabaseFile Storage;

      void IAssetObjectVisitor.Visit(CharacterController2DConfig asset) {
        Storage.CharacterController2DConfig.Add(asset);
      }

      void IAssetObjectVisitor.Visit(CharacterController3DConfig asset) {
        Storage.CharacterController3DConfig.Add(asset);
      }

      void IAssetObjectVisitor.Visit(EntityPrototype asset) {
        Storage.EntityPrototype.Add(CreateSurrogate(asset));
      }

      void IAssetObjectVisitor.Visit(EntityView asset) {
        Storage.EntityView.Add(asset);
      }

      void IAssetObjectVisitor.Visit(Map asset) {
        Storage.Map.Add(CreateSurrogate(asset));
      }

      void IAssetObjectVisitor.Visit(NavMesh asset) {
        Storage.NavMesh.Add(asset);
      }

      void IAssetObjectVisitor.Visit(NavMeshAgentConfig asset) {
        Storage.NavMeshAgentConfig.Add(asset);
      }

      void IAssetObjectVisitor.Visit(PhysicsMaterial asset) {
        Storage.PhysicsMaterial.Add(asset);
      }

      void IAssetObjectVisitor.Visit(PolygonCollider asset) {
        Storage.PolygonCollider.Add(asset);
      }

      void IAssetObjectVisitor.Visit(TerrainCollider asset) {
        Storage.TerrainCollider.Add(asset);
      }
    }

    [Serializable]
    private sealed class FlatDatabaseFile {
      public List<CharacterController2DConfig> CharacterController2DConfig = new List<CharacterController2DConfig>();
      public List<CharacterController3DConfig> CharacterController3DConfig = new List<CharacterController3DConfig>();
      public List<EntityPrototypeSurrogate> EntityPrototype = new List<EntityPrototypeSurrogate>();
      public List<EntityView> EntityView = new List<EntityView>();
      public List<MapSurrogate> Map = new List<MapSurrogate>();
      public List<NavMesh> NavMesh = new List<NavMesh>();
      public List<NavMeshAgentConfig> NavMeshAgentConfig = new List<NavMeshAgentConfig>();
      public List<PhysicsMaterial> PhysicsMaterial = new List<PhysicsMaterial>();
      public List<PolygonCollider> PolygonCollider = new List<PolygonCollider>();
      public List<TerrainCollider> TerrainCollider = new List<TerrainCollider>();
      public List<UserAssetSurrogate> UserAssets = new List<UserAssetSurrogate>();
    }
  }
}

// Replay/ChecksumFile.cs
﻿
namespace Quantum {

  [Serializable]
  public class ChecksumFile {
    public static int GrowSize = 60 * 60; // one minute of recording at 60 FPS

    [Serializable]
    public struct ChecksumEntry {
      public int Frame;
      // This is super annoying: Unity JSON cannot read the unsigned long data type. 
      // We can convert on this level, keeping the ULong CalculateChecksum() signature and encode the 
      // checksum as a long for serialization. Any other ideas?
      public long ChecksumAsLong;
    }

    public ChecksumEntry[] Checksums;

    private Int32 writeIndex;

    public Dictionary<int, ChecksumEntry> ToDictionary() {
      return Checksums.Where(item => item.Frame != 0).ToDictionary(item => item.Frame, item => item);
    }

    internal void RecordChecksum(QuantumGame game, Int32 frame, ulong checksum) {
      if (Checksums == null) {
        Checksums = new ChecksumEntry[GrowSize];
      }

      if (writeIndex + 1 > Checksums.Length) {
        Array.Resize(ref Checksums, Checksums.Length + GrowSize);
      }

      Checksums[writeIndex].Frame = frame;
      Checksums[writeIndex].ChecksumAsLong = ChecksumFileHelper.UlongToLong(checksum);
      writeIndex++;
    }

    internal void VerifyChecksum(QuantumGame game, Int32 frame, ulong checksum) {
      if (Checksums.Length > 0) {
        var readIndex = (frame - Checksums[0].Frame) / game.Session.SessionConfig.ChecksumInterval;
        Assert.Check(Checksums[readIndex].Frame == frame);
        if (Checksums[readIndex].ChecksumAsLong != ChecksumFileHelper.UlongToLong(checksum)) {
          Log.Error($"Checksum mismatch in frame {frame}: {Checksums[readIndex].ChecksumAsLong} != {ChecksumFileHelper.UlongToLong(checksum)}");
        }
      }
    }
  }

  public static class ChecksumFileHelper {
    public static unsafe long UlongToLong(ulong value) {
      return *((long*)&value);
    }

    public static unsafe ulong LongToULong(long value) {
      return *((ulong*)&value);
    }
  }
}


// Replay/InputProvider.cs
﻿
namespace Quantum {
  public class InputProvider : IDeterministicReplayProvider {
    private int                         _playerCount;
    private int                         _growSize;
    private int                         _startFrame;
    private DeterministicTickInputSet[] _inputs;

    private int MaxFrame => _inputs.Length + _startFrame;

    public InputProvider(DeterministicSessionConfig config, int capacity = 60 * 60, int growSize = 0) : this(config.PlayerCount, config.RollbackWindow, capacity, growSize) {
    }

    [Obsolete("Use 'InputProvider(DeterministicTickInputSet[])' instead.")]
    public InputProvider(DeterministicSessionConfig config, DeterministicTickInputSet[] inputList) : this(inputList) {}

    public InputProvider(DeterministicTickInputSet[] inputList) {
      ImportFromList(inputList);
    }

    public InputProvider(int playerCount, int startFrame, int capacity, int growSize) {
      _playerCount = playerCount;
      _startFrame  = startFrame;
      _growSize    = growSize;

      if (capacity > 0) {
        Allocate(capacity);
      }
    }

    [Obsolete("Use 'ImportFromList(DeterministicTickInputSet[])' instead.")]
    public void ImportFromList(DeterministicTickInputSet[] inputList, int startFrame) {
      // all InputProvider features expect the first input on the list to be the starting frame,
      // setting it to a different number will result in misbehavior
      ImportFromList(inputList);
    }

    public void ImportFromList(DeterministicTickInputSet[] inputList) {
      _startFrame = inputList.Length == 0 ? 0 : inputList[0].Tick;

      // Use external list as our own
      _inputs = inputList;
      for (int i = 0; i < _inputs.Length; i++) {
        for (int j = 0; j < inputList[i].Inputs.Length; j++) {
          inputList[i].Inputs[j].Sent = true;
        }
      }
    }

    public DeterministicTickInputSet[] ExportToList(int verifiedFrame) {
      var size = _inputs.Length;
      while (size > 0 && _inputs[size - 1].Inputs.Any(x => x.Tick == 0 || x.Tick > verifiedFrame)) {
        // Truncate non-verified and incomplete input from the end
        size--;
      }

      if (size <= 0) {
        return new DeterministicTickInputSet[0];
      }

      var result = new DeterministicTickInputSet[size];
      Array.Copy(_inputs, result, size);

      return result;
    }

    public void OnInputConfirmed(QuantumGame game, DeterministicFrameInputTemp input) {
      if (input.Frame < _startFrame) {
        // if starting to record from a frame following a snapshot,
        // confirmed inputs from previous frames can still arrive
        return;
      }

      if (input.Frame >= MaxFrame) {
        var minSize  = Math.Max(input.Frame - _startFrame, _inputs.Length);
        var growSize = _growSize > 0 ? minSize + _growSize : minSize * 2;
        Allocate(growSize);
      }

      _inputs[ToIndex(input.Frame)].Inputs[input.Player].Set(input);
    }

    public void InjectInput(DeterministicTickInput input, bool localReplay) {
      if (input.Tick >= MaxFrame) {
        var minSize  = Math.Max(input.Tick - _startFrame, _inputs.Length);
        var growSize = _growSize > 0 ? minSize + _growSize : minSize * 2;
        Allocate(growSize);
      }

      _inputs[ToIndex(input.Tick)].Inputs[input.PlayerIndex].CopyFrom(input);

      if (localReplay) {
        _inputs[ToIndex(input.Tick)].Inputs[input.PlayerIndex].Sent = true;
      }
    }

    public void AddRpc(int player, byte[] data, bool command) {
    }

    public bool CanSimulate(int frame) {
      if (frame < MaxFrame) {
        return _inputs[ToIndex(frame)].IsComplete() != false;
      }

      return false;
    }

    public QTuple<byte[], bool> GetRpc(int frame, int player) {
      if (frame < MaxFrame) {
        return QTuple.Create(
                             _inputs[ToIndex(frame)].Inputs[player].Rpc,
                             (_inputs[ToIndex(frame)].Inputs[player].Flags & DeterministicInputFlags.Command) == DeterministicInputFlags.Command);
      }

      return default;
    }

    public DeterministicFrameInputTemp GetInput(int frame, int player) {
      if (frame < MaxFrame) {
        var input = _inputs[ToIndex(frame)].Inputs[player];
        return DeterministicFrameInputTemp.Verified(frame, player, null, input.DataArray, input.DataLength, input.Flags);
      }

      return default;
    }

    private int ToIndex(int frame) {
      return frame - _startFrame;
    }

    private void Allocate(int size) {
      var oldSize = 0;
      if (_inputs == null) {
        _inputs = new DeterministicTickInputSet[size];
      } else {
        oldSize = _inputs.Length;
        Array.Resize(ref _inputs, size);
      }

      for (int i = oldSize; i < _inputs.Length; i++) {
        _inputs[i].Tick   = i + _startFrame;
        _inputs[i].Inputs = new DeterministicTickInput[_playerCount];
        for (int j = 0; j < _playerCount; j++) {
          _inputs[i].Inputs[j] = new DeterministicTickInput();
        }
      }
    }
  }

  public static class InputProviderExtensions {
    public static void CopyFrom(this DeterministicTickInput input, DeterministicTickInput otherInput) {
      input.Sent        = otherInput.Sent;
      input.Tick        = otherInput.Tick;
      input.PlayerIndex = otherInput.PlayerIndex;
      input.DataLength  = otherInput.DataLength;
      input.Flags       = otherInput.Flags;

      if (otherInput.DataArray != null) {
        input.DataArray = new byte[otherInput.DataArray.Length];
        Array.Copy(otherInput.DataArray, input.DataArray, otherInput.DataArray.Length);
      }

      if (otherInput.Rpc != null) {
        input.Rpc = new byte[otherInput.Rpc.Length];
        Array.Copy(otherInput.Rpc, input.Rpc, otherInput.Rpc.Length);
      }
    }

    public static void Set(this DeterministicTickInput input, DeterministicFrameInputTemp temp) {
      input.Tick        = temp.Frame;
      input.PlayerIndex = temp.Player;
      input.DataArray   = temp.CloneData();
      input.DataLength = temp.DataLength;
      input.Flags       = temp.Flags;
      input.Rpc         = temp.Rpc;
    }

    public static bool IsComplete(this DeterministicTickInputSet set) {
      for (int i = 0; i < set.Inputs.Length; i++) {
        if (set.Inputs[i].Tick == 0) {
          return false;
        }
      }

      return true;
    }

    public static bool IsFinished(this DeterministicTickInputSet set) {
      for (int i = 0; i < set.Inputs.Length; i++) {
        if (set.Inputs[i].Tick == 0 ||
            set.Inputs[i].Sent == false) {
          return false;
        }
      }

      return true;
    }
  }
}

// Replay/ReplayFile.cs
﻿
namespace Quantum {

  [Serializable]
  public class ReplayFile {
    public RuntimeConfig RuntimeConfig;
    public DeterministicSessionConfig DeterministicConfig;
    public DeterministicTickInputSet[] InputHistory;
    public Int32 Length;
    public byte[] Frame;
  }
}


// Replay/SessionContainer.cs
﻿
namespace Quantum {
  public class SessionContainer {
    DeterministicSessionConfig _sessionConfig;
    RuntimeConfig              _runtimeConfig;

    public DeterministicSession         session;
    public DeterministicSessionConfig config  => _sessionConfig ?? replayFile.DeterministicConfig;
    public RuntimeConfig runtimeConfig        => _runtimeConfig ?? replayFile.RuntimeConfig;
    public Native.Allocator allocator         => _allocator.Value;


    public IDeterministicReplayProvider provider;
    public IDeterministicGame           game;
    public IResourceManager             resourceManager;
    public ReplayFile                   replayFile;
    public IAssetSerializer             assetSerializer;
    public IEventDispatcher             eventDispatcher;
    public ICallbackDispatcher          callbackDispatcher;

    public static Boolean          _loadedAllStatics = false;
    public static Object           _lock             = new Object();

    private static Lazy<Native.Allocator> _allocator = new Lazy<Native.Allocator>(() => CreateNativeAllocator());

    public static Native.Allocator CreateNativeAllocator() {
      switch (System.Environment.OSVersion.Platform) {
        case PlatformID.Unix:
        case PlatformID.MacOSX:
          return new Native.LIBCAllocator();
        default:
          return new Native.MSVCRTAllocator();
      }
    }

    public static Native.Utility CreateNativeUtils() {
      switch (System.Environment.OSVersion.Platform) {
        case PlatformID.Unix:
        case PlatformID.MacOSX:
          return new Native.LIBCUtility();

        default:
          return new Native.MSVCRTUtility();
      }
    }

    public void Start(bool logInitForConsole = true) {
      
      if (!_loadedAllStatics) {
        lock (_lock) {
          if (!_loadedAllStatics) {
            // console first
            if (logInitForConsole) {
              Log.InitForConsole();
            }

            // try to figure out platform if not set
            if (Native.Utils == null) {
              Native.Utils = CreateNativeUtils();
            }

            if (MemoryLayoutVerifier.Platform == null) {
              MemoryLayoutVerifier.Platform = new MemoryLayoutVerifier.DefaultPlatform();
            }
          }
        }
      }

      var simulationConfig = (SimulationConfig)resourceManager.GetAsset(runtimeConfig.SimulationConfig.Id);

      lock (_lock) {
        if (!_loadedAllStatics) {
          Layers.Init(simulationConfig.Physics.Layers, simulationConfig.Physics.LayerMatrix);
          _loadedAllStatics = true;
        }
      }

      game = new QuantumGame(new QuantumGame.StartParameters() {
        ResourceManager = resourceManager, 
        AssetSerializer = assetSerializer, 
        CallbackDispatcher = callbackDispatcher, 
        EventDispatcher = eventDispatcher,
      });

      if (provider == null) {
        provider = new InputProvider(config);
      }

      DeterministicPlatformInfo info;
      info              = new DeterministicPlatformInfo();
      info.Allocator    = allocator;
      info.Architecture = DeterministicPlatformInfo.Architectures.x86;
      info.RuntimeHost  = DeterministicPlatformInfo.RuntimeHosts.PhotonServer;
      info.Runtime      = DeterministicPlatformInfo.Runtimes.NetFramework;
      info.TaskRunner   = new DotNetTaskRunner();

      switch (System.Environment.OSVersion.Platform) {
        case PlatformID.Unix:
          info.Platform = DeterministicPlatformInfo.Platforms.Linux;
          break;

        case PlatformID.MacOSX:
          info.Platform = DeterministicPlatformInfo.Platforms.OSX;
          break;

        default:
          info.Platform = DeterministicPlatformInfo.Platforms.Windows;
          break;
      }

      DeterministicSessionArgs args;
      args.Game                  = game;
      args.Mode                  = DeterministicGameMode.Replay;
      args.Replay                = provider;
      args.FrameData             = null;
      args.Communicator          = null;
      args.InitialTick           = 0;
      args.SessionConfig         = config;
      args.PlatformInfo          = info;
      args.RuntimeConfig         = RuntimeConfig.ToByteArray(runtimeConfig);

      session = new DeterministicSession(args);
      session.Join("server", config.PlayerCount);
    }

    public void Service(double? dt = null) {
      session.Update(dt);
    }

    public void Destroy() {
      if (session != null)
        session.Destroy();
      session = null;

      //DB.Dispose();
    }

    public SessionContainer() {
      _sessionConfig = null;
      _runtimeConfig = null;
    }

    public SessionContainer(DeterministicSessionConfig sessionConfig, RuntimeConfig runtimeConfig) {
      _sessionConfig = sessionConfig;
      _runtimeConfig = runtimeConfig;
    }
  }
}


// Systems/Base/SystemBase.cs
﻿
namespace Quantum {
  public abstract partial class SystemBase {
    Int32? _runtimeIndex;
    String _scheduleSample;

    public Int32 RuntimeIndex {
      get {
        return (Int32)_runtimeIndex;
      }
      set {
        if (_runtimeIndex.HasValue) {
          Log.Error("Can't change systems runtime index after game has started");
        } else {
          _runtimeIndex = value;
        }
      }
    }

    public virtual IEnumerable<SystemBase> ChildSystems {
      get {
        return new SystemBase[0];
      }
    }

    public IEnumerable<SystemBase> Hierarchy {
      get {
        yield return this;

        foreach (var child in ChildSystems) {
          foreach (var childHierarchy in child.Hierarchy) {
            yield return childHierarchy;
          }
        }
      }
    }

    public virtual Boolean StartEnabled {
      get { return true; }
    }

    public SystemBase() {
      _scheduleSample = GetType().Name + ".Schedule";
    }

    public SystemBase(string scheduleSample) {
      _scheduleSample = scheduleSample;
    }
    
    public virtual void OnInit(Frame f) {
      
    }

    public virtual void OnEnabled(Frame f) {
      
    }

    public virtual void OnDisabled(Frame f) {
      
    }

    public TaskHandle OnSchedule(Frame f, TaskHandle taskHandle) {
#if DEBUG
      var profiler = f.Context.ProfilerContext.GetProfilerForTaskThread(0);
      try {
        profiler.Start(_scheduleSample);
#endif
        
        return Schedule(f, taskHandle);
        
#if DEBUG
      } finally {
        profiler.End();
      }
#endif
    }

    protected abstract TaskHandle Schedule(Frame f, TaskHandle taskHandle);
  }
}

// Systems/Base/SystemGroup.cs

namespace Quantum {
  public unsafe class SystemGroup : SystemBase {
    SystemBase[] _children;

    public sealed override IEnumerable<SystemBase> ChildSystems {
      get { return _children; }
    }

    public SystemGroup(String name, params SystemBase[] children) : base(name + ".Schedule") {
      _children = children;
    }

    protected sealed override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      if (_children != null) {
        var systems = &f.Global->Systems;
        for (var i = 0; i < _children.Length; ++i) {
          if (BitSet256.IsSet(systems, _children[i].RuntimeIndex)) {
            try {
              taskHandle = _children[i].OnSchedule(f, taskHandle);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }
      }

      return taskHandle;
    }
  }
}

// Systems/Base/SystemMainThread.cs

namespace Quantum {
  public abstract unsafe class SystemMainThread : SystemBase {
    TaskDelegateHandle _updateHandle;
    SystemMainThread[] _children;

    String _update;

    public sealed override IEnumerable<SystemBase> ChildSystems {
      get { return _children; }
    }

    public SystemMainThread() {
      _update   = GetType().Name + ".Update";
      _children = new SystemMainThread[0];
    }

    protected SystemMainThread(string name, params SystemMainThread[] children)
      : base(name + ".Schedule") {
      Assert.Check(name != null);
      Assert.Check(children != null);

      _children = children;
      _update   = name + ".Update";
    }

    protected TaskHandle ScheduleUpdate(Frame f, TaskHandle taskHandle) {
      if (_updateHandle.IsValid == false) {
        f.Context.TaskContext.RegisterDelegate(TaskCallback, _update, ref _updateHandle);
      }

      return f.Context.TaskContext.AddMainThreadTask(_updateHandle, null, taskHandle);
    }

    protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      return ScheduleUpdate(f, taskHandle);
    }

    void TaskCallback(FrameThreadSafe frame, int start, int count, void* arg) {
      Update((Frame)frame);

      if (((FrameBase)frame).CommitCommandsMode == CommitCommandsModes.InBetweenSystems) {
        ((FrameBase)frame).Unsafe.CommitAllCommands();
      }

      if (_children != null) {
#if DEBUG
        var profiler = frame.Context.ProfilerContext.GetProfilerForTaskThread(frame.Thread);
#endif

        for (int i = 0; i < _children.Length; ++i) {
#if DEBUG
          try {
            profiler.Start(_children[i]._update);
#endif

            _children[i].TaskCallback(frame, start, count, arg);
#if DEBUG
          }
          finally {
            profiler.End();
          }
#endif
        }
      }
    }

    public abstract void Update(Frame f);
  }
}

// Systems/Base/SystemMainThreadFilter.cs
namespace Quantum {
  public abstract unsafe class SystemMainThreadFilter<T> : SystemMainThread where T : unmanaged {
    public virtual bool UseCulling {
      get { return true; }
    }

    public sealed override void Update(Frame f) {
      // grab iterator
      var it = f.Unsafe.FilterStruct<T>();

      // set culling flag
      it.UseCulling = UseCulling;

      // execute filter loop
      var filter = default(T);

      while (it.Next(&filter)) {
        Update(f, ref filter);
      }
    }

    public abstract void Update(Frame f, ref T filter);
  }
}

// Systems/Base/SystemMainThreadGroup.cs
namespace Quantum {
  public class SystemMainThreadGroup : SystemMainThread {
    public SystemMainThreadGroup(string update, params SystemMainThread[] children)
      : base(update, children) {
    }

    public sealed override void Update(Frame f) {
    }
  }
}

// Systems/Base/SystemSignalsOnly.cs

namespace Quantum {
  public class SystemSignalsOnly : SystemBase {
    protected sealed override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      return taskHandle;
    }
  }
}

// Systems/Core/NavigationSystem.cs
﻿
namespace Quantum.Core {
  public unsafe class NavigationSystem : SystemBase, INavigationCallbacks {
    Frame _f;

    protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      _f = f;
      return f.Navigation.Update(f, f.DeltaTime, this, taskHandle);
    }

    public void OnWaypointReached(EntityRef entity, FPVector2 waypoint, Navigation.WaypointFlag waypointFlags, ref bool resetAgent) {
      _f.Signals.OnNavMeshWaypointReached(entity, waypoint, waypointFlags, ref resetAgent);
    }

    public void OnSearchFailed(EntityRef entity, ref bool resetAgent) {
      _f.Signals.OnNavMeshSearchFailed(entity, ref resetAgent);
    }

    public void OnMoveAgent(EntityRef entity, FPVector2 desiredDirection) {
      _f.Signals.OnNavMeshMoveAgent(entity, desiredDirection);
    }
  }
}

// Systems/Core/EntityPrototypeSystem.cs
﻿namespace Quantum.Core {
  public unsafe sealed partial class EntityPrototypeSystem : SystemSignalsOnly, ISignalOnMapChanged {
    public override void OnInit(Frame f) {
      OnMapChanged(f, default);
    }

    public void OnMapChanged(Frame f, AssetRefMap previousMap) {
      if (previousMap.Id.IsValid) {
        foreach (var (entity, _) in f.GetComponentIterator<MapEntityLink>()) {
          f.Destroy(entity);
        }
      }

      if (f.Map != null) {
        f.Create(f.Map.MapEntities, f.Map);
      }
    }
  }
}

// Systems/Core/CullingSystem.cs

namespace Quantum.Core {
  public unsafe class CullingSystem2D : SystemBase {
    protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      return f.Context.Culling.Schedule2D(f, taskHandle);
    }
  }
  
  public unsafe class CullingSystem3D : SystemBase {
    protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      return f.Context.Culling.Schedule3D(f, taskHandle);
    }
  }
}

// Systems/Core/PhysicsSystem.cs
﻿
namespace Quantum.Core {
  public unsafe partial class PhysicsSystem2D : SystemBase, ICollisionCallbacks2D {
    public override void OnInit(Frame f) {
      f.Physics2D.Init();
    }

    protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      return f.Physics2D.Update(this, f.DeltaTime, taskHandle);
    }
    
    public void OnCollision2D(FrameBase f, CollisionInfo2D info) {
      ((Frame)f).Signals.OnCollision2D(info);
    }

    public void OnCollisionEnter2D(FrameBase f, CollisionInfo2D info) {
      ((Frame)f).Signals.OnCollisionEnter2D(info);
    }

    public void OnCollisionExit2D(FrameBase f, ExitInfo2D info) {
      ((Frame)f).Signals.OnCollisionExit2D(info);
    }

    public void OnTrigger2D(FrameBase f, TriggerInfo2D info) {
      ((Frame)f).Signals.OnTrigger2D(info);
    }

    public void OnTriggerEnter2D(FrameBase f, TriggerInfo2D info) {
      ((Frame)f).Signals.OnTriggerEnter2D(info);
    }

    public void OnTriggerExit2D(FrameBase f, ExitInfo2D info) {
      ((Frame)f).Signals.OnTriggerExit2D(info);
    }
  }
  
  public unsafe partial class PhysicsSystem3D : SystemBase, ICollisionCallbacks3D {
    public override void OnInit(Frame f) {
      f.Physics3D.Init();
    }
    
    protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
      return f.Physics3D.Update(this, f.DeltaTime, taskHandle);
    }

    public void OnCollision3D(FrameBase f, CollisionInfo3D info) {
      ((Frame)f).Signals.OnCollision3D(info);
    }

    public void OnCollisionEnter3D(FrameBase f, CollisionInfo3D info) {
      ((Frame)f).Signals.OnCollisionEnter3D(info);
    }

    public void OnCollisionExit3D(FrameBase f, ExitInfo3D info) {
      ((Frame)f).Signals.OnCollisionExit3D(info);
    }

    public void OnTrigger3D(FrameBase f, TriggerInfo3D info) {
      ((Frame)f).Signals.OnTrigger3D(info);
    }

    public void OnTriggerEnter3D(FrameBase f, TriggerInfo3D info) {
      ((Frame)f).Signals.OnTriggerEnter3D(info);
    }

    public void OnTriggerExit3D(FrameBase f, ExitInfo3D info) {
      ((Frame)f).Signals.OnTriggerExit3D(info);
    }
  }
}
