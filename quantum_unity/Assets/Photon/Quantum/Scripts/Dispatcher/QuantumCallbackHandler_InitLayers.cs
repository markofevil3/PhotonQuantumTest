using Quantum;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuantumCallbackHandler_InitLayers {
    public static IDisposable Initialize() {
        var disposables = new CompositeDisposable();

        try {
            disposables.Add(QuantumCallback.SubscribeManual((CallbackGameStarted c) => {
                var config = c.Game.Configurations.Simulation.Physics;
                Layers.Init(config.Layers, config.LayerMatrix, forceInitUnsafe: true);
            }));
            
            disposables.Add(QuantumCallback.SubscribeManual((CallbackGameResynced c) => {
                var config = c.Game.Configurations.Simulation.Physics;
                Layers.Init(config.Layers, config.LayerMatrix, forceInitUnsafe: true);
            }));
        } catch {
            // if something goes wrong clean up subscriptions
            disposables.Dispose();
            throw;
        }

        return disposables;
    }

    private class CompositeDisposable : IDisposable {
        private List<IDisposable> _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable) {
            _disposables.Add(disposable);
        }

        public void Dispose() {
            foreach (var disposable in _disposables) {
                try {
                    disposable.Dispose();
                } catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }
        }
    }
}