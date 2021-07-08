using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;
using Quantum.Profiling;
using UnityEngine;
using static UnityEditor.Graphs.Styles;
using Color = UnityEngine.Color;

namespace Quantum.Editor {
  [Serializable]
  public class QuantumTaskProfilerModel : ISerializationCallbackReceiver {

    public static readonly Color DefaultSampleColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

    public byte FormatVersion = BinaryFormat.Invalid;
    public List<Frame> Frames = new List<Frame>();
    public List<SampleMeta> SamplesMeta = new List<SampleMeta>();
    public List<QuantumProfilingClientInfo> Clients = new List<QuantumProfilingClientInfo>();

    /// <summary>
    /// 'QPRF'
    /// </summary>
    private const int BinaryHeader = 0x46525051;
    private const int InitialStackCapacity = 10;

    private Dictionary<string, int> _clientIdToIndex = new Dictionary<string, int>();
    private Dictionary<string, int> _samplePathToId = new Dictionary<string, int>();
    private Sample[] _samplesStack = new Sample[InitialStackCapacity];
    private int samplesStackCount = 0;

    public static QuantumTaskProfilerModel LoadFromFile(string path) {
      // first, try to read as a binary
      try {
        using (var serializer = new BinarySerializer(File.OpenRead(path), false)) {
          var result = new QuantumTaskProfilerModel();
          result.Serialize(serializer);
          return result;
        }
      } catch (System.InvalidOperationException) {
        // well, try to load as json now
        var text = File.ReadAllText(path);
        var result = JsonUtility.FromJson<QuantumTaskProfilerModel>(text);
        if (result.FormatVersion == 0) {
          var legacySession = JsonUtility.FromJson<LegacySerializableFrames>(text);
          if (legacySession.Frames != null) {
            result = new QuantumTaskProfilerModel();
            foreach (var frame in legacySession.Frames) {
              result.AddFrame(null, frame);
            }
          }
        }

        return result;
      }
    }

    public void AccumulateDurations(BitArray mask, int startFrame, List<float> target) {
      if (Frames.Count == 0)
        return;

      // we need to keep track of the match depth; if we have a match for a parent, 
      // we want to skip all the descendants
      int matchDepthPlusOne = 0;

      for (int frame = startFrame; frame < Frames.Count; ++frame) {
        long totalTicks = 0;
        var f = Frames[frame];
        foreach (var thread in f.Threads) {
          foreach (var sample in thread.Samples) {
            if (matchDepthPlusOne > 0) {
              if (sample.Depth + 1 > matchDepthPlusOne)
                continue;
              else
                matchDepthPlusOne = 0;
            }

            int mod = mask.Get(sample.Id) ? 1 : 0;

            totalTicks += sample.Duration * mod;
            matchDepthPlusOne = sample.Depth * mod;
          }
        }

        target.Add(Mathf.Min((float)(totalTicks * f.TicksToMS), f.DurationMS));
      }
    }

    public void AddFrame(QuantumProfilingClientInfo clientInfo, ProfilerContextData data) {
      var frame = new Frame();

      GetStartEndRange(data, out frame.Start, out frame.Duration);
      frame.TickFrequency = data.Frequency;
      frame.Number = data.Frame;
      frame.IsVerified = data.IsVerified;
      frame.SimulationId = data.SimulationId;
      if (clientInfo != null) {
        frame.ClientId = GetOrAddClientInfo(clientInfo);
      }

      foreach (var sourceThread in data.Profilers) {
        var thread = new Thread() {
          Name = sourceThread.Name
        };

        foreach (var sourceSample in sourceThread.Samples) {
          switch (sourceSample.Type) {
            case SampleType.Begin: {
                var sample = new Sample() {
                  Id = GetOrAddMetaId(sourceSample.Name),
                  Start = sourceSample.Time,
                };

                PushSample(sample);
              }
              break;

            case SampleType.End: {
                var sample = PopSample();
                var duration = sourceSample.Time - sample.Start;
                sample.Duration = duration;
                sample.Start -= frame.Start;
                sample.Depth = samplesStackCount;
                thread.Samples.Add(sample);
              }
              break;

            case SampleType.Event: {
                // events have duration of 0 and depth is always 0
                var sample = new Sample() {
                  Id = GetOrAddMetaId(sourceSample.Name),
                  Start = sourceSample.Time - frame.Start,
                  Duration = 0,
                  Depth = 0
                };

                thread.Samples.Add(sample);
              }
              break;

            default:
              break;
          }
        }

        frame.Threads.Add(thread);
      }

      Frames.Add(frame);
    }

    public void CreateSearchMask(string pattern, BitArray bitArray) {
      if (bitArray.Length < SamplesMeta.Count) {
        bitArray.Length = SamplesMeta.Count;
      }
      for (int i = 0; i < SamplesMeta.Count; ++i) {
        var name = SamplesMeta[i].Name;
        bitArray.Set(i, name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0);
      }
    }

    public Frame FindPrevSafe(int index, bool verified = true) {
      if (index > Frames.Count || index <= 0)
        return null;

      for (int i = index - 1; i >= 0; --i) {
        if (Frames[i].IsVerified == verified)
          return Frames[i];
      }

      return null;
    }

    public int FrameIndexToSimulationIndex(int index) {
      if (Frames.Count == 0)
        return 0;

      int currentSimulation = Frames[0].SimulationId;
      int simulationIndex = 0;

      for (int i = 0; i < Frames.Count; ++i) {
        var frame = Frames[i];
        if (frame.SimulationId != currentSimulation) {
          ++simulationIndex;
          currentSimulation = frame.SimulationId;
        }

        if (i == index) {
          return simulationIndex;
        }
      }

      throw new InvalidOperationException();
    }

    public QuantumProfilingClientInfo GetClientInfo(Frame frame) {
      if (frame.ClientId < 0)
        return null;
      return Clients[frame.ClientId];
    }

    public void GetFrameDurations(List<float> values) {
      foreach (var f in Frames) {
        values.Add(f.DurationMS);
      }
    }

    public void GetSampleMeta(Sample s, out Color color, out string text) {
      var meta = SamplesMeta[s.Id];
      color = meta.Color;
      text = meta.Name;
    }

    public void GroupBySimulationId(List<float> values, List<float> grouped, List<float> counts = null) {
      Debug.Assert(values == null || values.Count == Frames.Count);
      if (Frames.Count == 0)
        return;

      int currentSimulation = Frames[0].SimulationId;
      float total = 0.0f;
      int count = 0;

      for (int i = 0; i < Frames.Count; ++i) {
        var frame = Frames[i];
        if (frame.SimulationId != currentSimulation) {
          grouped.Add(total);
          counts?.Add((float)count);
          count = 0;
          total = 0.0f;
          currentSimulation = frame.SimulationId;
        }
        ++count;
        total += values == null ? Frames[i].DurationMS : values[i];
      }

      counts?.Add((float)count);
      grouped.Add(total);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {

      _samplePathToId.Clear();
      for (int i = 0; i < SamplesMeta.Count; ++i) {
        _samplePathToId.Add(SamplesMeta[i].FullName, i);
      }

      _clientIdToIndex.Clear();
      for (int i = 0; i < Clients.Count; ++i) {
        _clientIdToIndex.Add(Clients[i].ProfilerId, i);
      }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() {
      FormatVersion = BinaryFormat.Latest;
    }

    public void Serialize(BinarySerializer serializer) {

      if (!serializer.IsReading) {
        ((ISerializationCallbackReceiver)this).OnBeforeSerialize();
      }

      int header = BinaryHeader;
      serializer.Serialize(ref header);
      if (header != BinaryHeader) {
        throw new InvalidOperationException("Invalid header");
      }

      serializer.Serialize(ref FormatVersion);
      if (FormatVersion > BinaryFormat.Latest || FormatVersion == 0) {
        throw new InvalidOperationException($"Version not supported: {FormatVersion}");
      }

      serializer.SerializeList(ref SamplesMeta, Serialize);
      serializer.SerializeList(ref Frames, Serialize);

      if (FormatVersion >= BinaryFormat.WithClientInfo) {
        serializer.SerializeList(ref Clients, Serialize);
      }

      if (serializer.IsReading) {
        ((ISerializationCallbackReceiver)this).OnAfterDeserialize();
      }
    }

    public int SimulationIndexToFrameIndex(int index, out int frameCount) {
      frameCount = 0;
      if (Frames.Count == 0)
        return 0;

      if (index == 0)
        return 0;

      int currentSimulation = Frames[0].SimulationId;
      int simulationIndex = 0;

      int i;
      for (i = 0; i < Frames.Count; ++i) {
        var frame = Frames[i];
        if (frame.SimulationId != currentSimulation) {
          ++simulationIndex;
          currentSimulation = frame.SimulationId;
          if (index == simulationIndex) {
            break;
          }
        }
      }

      var frameIndex = i;

      for (; i < Frames.Count; ++i) {
        var frame = Frames[i];
        if (frame.SimulationId == currentSimulation) {
          ++frameCount;
        } else {
          break;
        }
      }

      return frameIndex;
    }

    private static void GetStartEndRange(ProfilerContextData sourceFrame, out long min, out long range) {
      min = long.MaxValue;
      var max = long.MinValue;

      for (int i = 0; i < sourceFrame.Profilers.Length; ++i) {
        var p = sourceFrame.Profilers[i];
        if (p.Samples.Length > 0) {
          min = Math.Min(min, p.Samples[0].Time);
          max = Math.Max(max, p.Samples[p.Samples.Length - 1].Time);
        }
      }

      range = max - min;
    }

    private static string ProcessName(string name, out Color color) {
      if (name.Length >= 7 && name[name.Length - 7] == '#') {
        // possibly hex encoded color
        var hex = name.Substring(name.Length - 7);
        if (ColorUtility.TryParseHtmlString(hex, out color)) {
          return name.Substring(0, name.Length - 7).Trim();
        }
      }

      color = DefaultSampleColor;
      return name;
    }

    private int GetOrAddClientInfo(QuantumProfilingClientInfo info) {
      if (_clientIdToIndex.TryGetValue(info.ProfilerId, out int id)) {
        return id;
      }

      _clientIdToIndex.Add(info.ProfilerId, Clients.Count);
      Clients.Add(info);

      return Clients.Count - 1;
    }

    private int GetOrAddMetaId(string name) {
      if (_samplePathToId.TryGetValue(name, out int id)) {
        return id;
      }

      var shortName = ProcessName(name, out var color);

      _samplePathToId.Add(name, SamplesMeta.Count);
      SamplesMeta.Add(new SampleMeta() {
        Name = shortName,
        Color = color,
        FullName = name,
      });

      return SamplesMeta.Count - 1;
    }

    private Sample PopSample() {
      Debug.Assert(samplesStackCount > 0);
      return _samplesStack[--samplesStackCount];
    }

    private void PushSample(Sample sample) {
      Debug.Assert(samplesStackCount <= _samplesStack.Length);
      if (samplesStackCount + 1 >= _samplesStack.Length) {
        Array.Resize(ref _samplesStack, samplesStackCount + 10);
      }
      _samplesStack[samplesStackCount++] = sample;
    }

    private void Serialize(BinarySerializer serializer, ref SampleMeta meta) {
      serializer.Serialize(ref meta.FullName);
      serializer.Serialize(ref meta.Name);
      serializer.Serialize(ref meta.Color);
    }

    private void Serialize(BinarySerializer serializer, ref QuantumProfilingClientInfo info) {
      serializer.Serialize(ref info.ProfilerId);
      serializer.Serialize(ref info.Config, DeterministicSessionConfig.ToByteArray, DeterministicSessionConfig.FromByteArray);

      serializer.SerializeList(ref info.Properties, Serialize);
    }

    private void Serialize(BinarySerializer serializer, ref QuantumProfilingClientInfo.CustomProperty info) {
      serializer.Serialize(ref info.Name);
      serializer.Serialize(ref info.Value);
    }

    private void Serialize(BinarySerializer serializer, ref Frame frame) {
      if (FormatVersion < BinaryFormat.WithClientInfo) {
        string oldDeviceId = "";
        serializer.Serialize(ref oldDeviceId);
      } else {
        serializer.Serialize7BitEncoded(ref frame.ClientId);
      }

      serializer.Serialize7BitEncoded(ref frame.Duration);
      serializer.Serialize7BitEncoded(ref frame.TickFrequency);
      serializer.Serialize(ref frame.IsVerified);
      serializer.Serialize(ref frame.Start);
      serializer.Serialize(ref frame.Number);
      serializer.Serialize(ref frame.SimulationId);
      serializer.SerializeList(ref frame.Threads, Serialize);
    }

    private void Serialize(BinarySerializer serializer, ref Thread thread) {
      serializer.Serialize(ref thread.Name);
      serializer.SerializeList(ref thread.Samples, Serialize);
    }

    private void Serialize(BinarySerializer serializer, ref Sample sample) {
      serializer.Serialize7BitEncoded(ref sample.Id);
      serializer.Serialize7BitEncoded(ref sample.Start);
      serializer.Serialize7BitEncoded(ref sample.Duration);
      serializer.Serialize7BitEncoded(ref sample.Depth);
    }

    [Serializable]
    public struct Sample {
      public int Depth;
      public long Duration;
      public int Id;
      public long Start;
    }

    public static class BinaryFormat {
      public const byte Initial = 1;
      public const byte Invalid = 0;
      public const byte Latest = WithClientInfo;
      public const byte WithClientInfo = 2;
    }

    [Serializable]
    public class Frame {
      public int ClientId = -1;
      public long Duration;
      public bool IsVerified;
      public int Number;
      public int SimulationId;
      public long Start;
      public List<Thread> Threads = new List<Thread>();
      public long TickFrequency;
      public float DurationMS => (float)(Duration * TicksToMS);
      public double TicksToMS => 1000.0 / TickFrequency;
    }

    [Serializable]
    public class SampleMeta {
      public Color Color;
      public string FullName;
      public string Name;
    }

    [Serializable]
    public class Thread {
      public string Name;
      public List<Sample> Samples = new List<Sample>();
    }

    [Serializable]
    private class LegacySerializableFrames {
      public ProfilerContextData[] Frames = Array.Empty<ProfilerContextData>();
    }
  }
}
