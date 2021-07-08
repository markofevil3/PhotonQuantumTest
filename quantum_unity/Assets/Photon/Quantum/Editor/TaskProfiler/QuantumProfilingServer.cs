using System;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Quantum.Editor {

  public class QuantumProfilingServer {
    public const int PORT = 30000;
    private static QuantumProfilingServer _server;

    private EventBasedNetListener _listener;

    private NetManager _manager;
    private Dictionary<NetPeer, QuantumProfilingClientInfo> _peers = new Dictionary<NetPeer, QuantumProfilingClientInfo>();

    private QuantumProfilingServer() {
      _listener = new EventBasedNetListener();

      _manager = new NetManager(_listener);
      _manager.BroadcastReceiveEnabled = true;
      _manager.Start(PORT);

      _listener.ConnectionRequestEvent += OnConnectionRequest;
      _listener.PeerConnectedEvent += OnPeerConnected;
      _listener.PeerDisconnectedEvent += OnPeerDisconnected;
      _listener.NetworkReceiveEvent += OnNetworkReceiveEvent;
      _listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnectedEvent;

      Debug.Log($"QuantumProfilingServer: Started @ 0.0.0.0:{PORT}");
    }

    public static event Action<QuantumProfilingClientInfo, Profiling.ProfilerContextData> SampleReceived;

    public static void Update() {
      if (_server == null) {
        _server = new QuantumProfilingServer();
      }

      _server._manager.PollEvents();
    }

    private void OnConnectionRequest(ConnectionRequest request) {
      request.AcceptIfKey(QuantumProfilingClientConstants.CONNECT_TOKEN);
    }

    private void OnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliverymethod) {
      try {

        var msgType = reader.GetByte();
        var text = reader.GetString();

        if (msgType == QuantumProfilingClientConstants.ClientInfoMessage) {
          var data = JsonUtility.FromJson<QuantumProfilingClientInfo>(text);
          _peers[peer] = data;

        } else if (msgType == QuantumProfilingClientConstants.FrameMessage) {
          if (SampleReceived != null) {
            var data = JsonUtility.FromJson<Profiling.ProfilerContextData>(text);
            try {
              if (_peers.TryGetValue(peer, out var info)) {
                SampleReceived(info, data);
              } else {
                Log.Error("Client Info not found for peer {0}", peer.EndPoint);
              }
            } catch (Exception ex) {
              Log.Error($"QuantumProfilingServer: Sample Handler Error: {ex}");
            }
          }
        } else {
          throw new NotSupportedException($"Unknown message type: {msgType}");
        }
      } catch (Exception ex) {
        Log.Error($"QuantumProfilingServer: Receive error: {ex}, disconnecting peer {peer.EndPoint}");
        _manager.DisconnectPeerForce(peer);
      }
    }

    private void OnNetworkReceiveUnconnectedEvent(IPEndPoint remoteendpoint, NetPacketReader reader, UnconnectedMessageType messagetype) {
      if (reader.GetString() == QuantumProfilingClientConstants.DISCOVER_TOKEN) {
        Log.Info($"QuantumProfilingServer: Discovery Request From {remoteendpoint}");
        _manager.SendUnconnectedMessage(NetDataWriter.FromString(QuantumProfilingClientConstants.DISCOVER_RESPONSE_TOKEN), remoteendpoint);
      }
    }

    private void OnPeerConnected(NetPeer peer) {
      Log.Info($"QuantumProfilingServer: Connection From {peer.EndPoint}");
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
      _peers.Remove(peer);
    }

  }
}