using System;
using Quantum;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuantumCallbackHandler_UnityCallbacks : IDisposable {
  private Coroutine _coroutine;
  private Map _currentMap;

  private readonly CallbackUnitySceneLoadBegin    _callbackUnitySceneLoadBegin;
  private readonly CallbackUnitySceneLoadDone     _callbackUnitySceneLoadDone;
  private readonly CallbackUnitySceneUnloadBegin  _callbackUnitySceneUnloadBegin;
  private readonly CallbackUnitySceneUnloadDone   _callbackUnitySceneUnloadDone;

  public QuantumCallbackHandler_UnityCallbacks(QuantumGame game) {
    _callbackUnitySceneLoadBegin   = new CallbackUnitySceneLoadBegin(game);
    _callbackUnitySceneLoadDone    = new CallbackUnitySceneLoadDone(game);
    _callbackUnitySceneUnloadBegin = new CallbackUnitySceneUnloadBegin(game);
    _callbackUnitySceneUnloadDone  = new CallbackUnitySceneUnloadDone(game);
  }

  public static IDisposable Initialize() {
    return QuantumCallback.SubscribeManual((CallbackGameStarted c) => {

      var runner = QuantumRunner.FindRunner(c.Game);
      if (runner != QuantumRunner.Default) {
        // only work for the default runner
        return;
      }

      var callbacksHost = new QuantumCallbackHandler_UnityCallbacks(c.Game);

      // TODO: this has a bug: disposing parent sub doesn't cancel following subscriptions
      QuantumCallback.Subscribe(runner, (CallbackGameDestroyed cc) => callbacksHost.Dispose(), runner: runner);
      QuantumCallback.Subscribe(runner, (CallbackUpdateView cc) => callbacksHost.UpdateLoading(cc.Game), runner: runner);
    });
  }

  public void Dispose() {
    QuantumCallback.UnsubscribeListener(this);

    if (_coroutine != null) {
      Log.Warn("Map loading or unloading was still in progress when destroying the game");
    }

    if (_currentMap != null) {
      _coroutine = QuantumMapLoader.Instance?.StartCoroutine(UnloadMap(_currentMap.Scene));
      _currentMap = null;
    }
  }

  private System.Collections.IEnumerator LoadMap(string sceneName) {
    try {
      _callbackUnitySceneLoadBegin.SceneName = sceneName;
      QuantumCallback.Dispatcher.Publish(_callbackUnitySceneLoadBegin);

      yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
      SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

      _callbackUnitySceneLoadDone.SceneName = sceneName;
      QuantumCallback.Dispatcher.Publish(_callbackUnitySceneLoadDone);
    } finally {
      _coroutine = null;
    }
  }

  private System.Collections.IEnumerator UnloadMap(string sceneName) {
    try {
      _callbackUnitySceneUnloadBegin.SceneName = sceneName;
      QuantumCallback.Dispatcher.Publish(_callbackUnitySceneUnloadBegin);

      yield return SceneManager.UnloadSceneAsync(sceneName);

      _callbackUnitySceneUnloadDone.SceneName = sceneName;
      QuantumCallback.Dispatcher.Publish(_callbackUnitySceneUnloadDone);
    } finally {
      _coroutine = null;
    }
  }

  private void UpdateLoading(QuantumGame game) {
    if (game.Configurations.Simulation.AutoLoadSceneFromMap && _coroutine == null) {
      var map = game.Frames.Verified.Map;
      if (map != null && map != _currentMap) {
        if (_currentMap == null) {
          // Check if the scene has already been loaded, then treat this as externally controlled and also don't unload it.
          if (SceneManager.GetSceneByName(game.Frames.Verified.Map.Scene).IsValid() == false) {
            _currentMap = game.Frames.Verified.Map;
            _coroutine = QuantumMapLoader.Instance.StartCoroutine(LoadMap(_currentMap.Scene));
          }
        } else {
          _coroutine = QuantumMapLoader.Instance.StartCoroutine(UnloadMap(_currentMap.Scene));
          _currentMap = null;
        }
      }
    }
  }
}