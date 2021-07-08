using System;
using Photon.Realtime;
using UnityEngine;

namespace Quantum.Demo {
  [Serializable]
  public class ReconnectInformation {
    public string Room;
    public string Region;
    public string AppVersion;
    public string UserId;
    public string MasterServerAddress;
    public string AuthToken;
    public long TimeoutInTicks;

    public DateTime Timeout {
      get => new DateTime(TimeoutInTicks);
      set => TimeoutInTicks = value.Ticks;
    }

    public bool IsValid => Timeout >= DateTime.Now;

    public static ReconnectInformation Instance {
      get {
        var result = JsonUtility.FromJson<ReconnectInformation>(PlayerPrefs.GetString("Quantum.Demo.ReconnectInformation"));
        return result ?? new ReconnectInformation();
      }
      set => PlayerPrefs.SetString("Quantum.Demo.ReconnectInformation", JsonUtility.ToJson(value));
    }

    public static void Reset() {
      PlayerPrefs.SetString("Quantum.Demo.ReconnectInformation", string.Empty);
    }

    public static void Refresh(LoadBalancingClient client, TimeSpan timeout) {
      Instance = new ReconnectInformation {
        Room                = client.CurrentRoom.Name,
        Region              = client.CloudRegion,
        Timeout             = DateTime.Now + timeout,
        UserId              = client.UserId,
        AuthToken           = client.AuthValues.Token,
        AppVersion          = client.AppVersion,
        MasterServerAddress = client.MasterServerAddress
      };
    }

    public override string ToString() {
      return $"Room '{Room}' Region '{Region}' Timeout {Timeout}' AppVersion '{AppVersion}' UserId '{UserId}' MasterServerAddress '{MasterServerAddress}' AuthToken '{AuthToken}'";
    }
  }
}