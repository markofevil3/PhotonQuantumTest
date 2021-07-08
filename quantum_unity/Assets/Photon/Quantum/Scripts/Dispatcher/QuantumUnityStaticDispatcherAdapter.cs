using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;


public abstract class QuantumUnityStaticDispatcherAdapter {
  protected sealed class Worker : MonoBehaviour {
    public DispatcherBase Dispatcher;
    private void LateUpdate() {
      if (Dispatcher == null) {
        // this may happen when scripts get reloaded in editor
        Destroy(gameObject);
      } else { 
        Dispatcher.RemoveDeadListners();
      }
    }
  }
}

public abstract class QuantumUnityStaticDispatcherAdapter<TDispatcher, TDispatchableBase>  : QuantumUnityStaticDispatcherAdapter
  where TDispatcher : DispatcherBase, IQuantumUnityDispatcher, new()
  where TDispatchableBase : IDispatchable {

  protected static Worker _worker;

  public static TDispatcher Dispatcher { get; } = new TDispatcher();

  public static void Clear() {
    Dispatcher.Clear();
    if (_worker) {
      UnityEngine.Object.Destroy(_worker.gameObject);
      _worker = null;
    }
  }

  public static void RemoveDeadListeners() {
    Dispatcher.RemoveDeadListners();
  }

  public static Quantum.DispatcherSubscription Subscribe<TDispatchable>(UnityEngine.Object listener, Quantum.DispatchableHandler<TDispatchable> handler, bool once = false, bool onlyIfActiveAndEnabled = false, string runnerId = null, QuantumRunner runner = null, DeterministicGameMode? gameMode = null, DeterministicGameMode? excludeGameMode = null)
    where TDispatchable : TDispatchableBase {
    DispatchableFilter filter = null;
    if (!string.IsNullOrEmpty(runnerId)) {
      Debug.Assert(runner == null && gameMode == null && excludeGameMode == null);
      filter = (game) => QuantumRunner.FindRunner(game)?.Id == runnerId;
    } else if (runner != null) {
      Debug.Assert(gameMode == null && excludeGameMode == null);
      // do not capture ref
      var runnerInstanceId = runner.GetInstanceID();
      filter = (game) => QuantumRunner.FindRunner(game)?.GetInstanceID() == runnerInstanceId;
    } else if (gameMode != null) {
      Debug.Assert(excludeGameMode == null);
      filter = (game) => game.Session.GameMode == gameMode.Value;
    } else if (excludeGameMode != null) {
      filter = (game) => game.Session.GameMode != excludeGameMode.Value;
    }

    return Subscribe(listener, handler, filter, once, onlyIfActiveAndEnabled);
  }

  public static Quantum.DispatcherSubscription Subscribe<TDispatchable>(UnityEngine.Object listener, Quantum.DispatchableHandler<TDispatchable> handler, QuantumGame game, bool once = false, bool onlyIfActiveAndEnabled = false)
  where TDispatchable : TDispatchableBase {
    EnsureWorkerExistsAndIsActive();
    return Dispatcher.Subscribe(listener, handler, once, onlyIfActiveAndEnabled, filter: g => g == game);
  }

  public static Quantum.DispatcherSubscription Subscribe<TDispatchable>(UnityEngine.Object listener, Quantum.DispatchableHandler<TDispatchable> handler, DispatchableFilter filter, bool once = false, bool onlyIfActiveAndEnabled = false)
    where TDispatchable : TDispatchableBase {
    EnsureWorkerExistsAndIsActive();
    return Dispatcher.Subscribe(listener, handler, once, onlyIfActiveAndEnabled, filter: filter);
  }

  public static IDisposable SubscribeManual<TDispatchable>(object listener, Quantum.DispatchableHandler<TDispatchable> handler)
  where TDispatchable : TDispatchableBase {
    return Dispatcher.SubscribeManual(listener, handler);
  }

  public static IDisposable SubscribeManual<TDispatchable>(Quantum.DispatchableHandler<TDispatchable> handler)
    where TDispatchable : TDispatchableBase {
    return Dispatcher.SubscribeManual(handler);
  }

  public static bool Unsubscribe(DispatcherSubscription subscription) {
    return Dispatcher.Unsubscribe(subscription);
  }

  public static bool UnsubscribeListener(object listener) {
    return Dispatcher.UnsubscribeListener(listener);
  }
  public static bool UnsubscribeListener<TDispatchable>(object listener) where TDispatchable : TDispatchableBase {
    return Dispatcher.UnsubscribeListener<TDispatchable>(listener);
  }

  private static void EnsureWorkerExistsAndIsActive() {
    if (_worker) {
      if (!_worker.isActiveAndEnabled)
        throw new InvalidOperationException($"{typeof(Worker)} is disabled");

      return;
    }

    var go = new GameObject(typeof(TDispatcher).Name + nameof(Worker), typeof(Worker));
    go.hideFlags = HideFlags.HideAndDontSave;
    GameObject.DontDestroyOnLoad(go);

    _worker = go.GetComponent<Worker>();
    if (!_worker)
      throw new InvalidOperationException($"Unable to create {typeof(Worker)}");

    _worker.Dispatcher = Dispatcher;
  }
}