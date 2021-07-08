using Newtonsoft.Json;
using Photon.Deterministic;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Quantum {
  public class ReplayRunnerSample {

    public static bool Run(string pathToLUT,string pathToDatabaseFile, string pathToReplayFile, string pathToChecksumFile) {

      FPLut.Init(pathToLUT);

      FileLoader.Init(new DotNetFileLoader(Path.GetDirectoryName(pathToDatabaseFile)));

      Console.WriteLine($"Loading replay from file: '{Path.GetFileName(pathToReplayFile)}' from folder '{Path.GetDirectoryName(pathToReplayFile)}'");

      if (!File.Exists(pathToDatabaseFile)) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"File not found: '{pathToReplayFile}'");
        Console.ForegroundColor = ConsoleColor.Gray;
        return false;
      }

      var serializer = new QuantumJsonSerializer();

      SessionContainer container = new SessionContainer();

      var callbackDispatcher = new CallbackDispatcher();

      container.assetSerializer = serializer;
      container.resourceManager = new ResourceManagerStatic(serializer.DeserializeAssets(File.ReadAllBytes(pathToDatabaseFile)), container.allocator);
      container.replayFile = serializer.DeserializeReplay(File.ReadAllBytes(pathToReplayFile));
      container.callbackDispatcher = callbackDispatcher;
      container.Start();
      (container.provider as InputProvider).ImportFromList(container.replayFile.InputHistory);

      var numberOfFrames = container.replayFile.Length;

      var checksumVerification = String.IsNullOrEmpty(pathToChecksumFile) ? null : new ChecksumVerification(pathToChecksumFile, callbackDispatcher);

      while (container.session.FramePredicted == null || container.session.FramePredicted.Number < numberOfFrames) {
        Thread.Sleep(1);
        container.Service(dt: 1.0f);

        if (Console.KeyAvailable) {
          if (Console.ReadKey().Key == ConsoleKey.Escape) {
            Console.WriteLine("Stopping replay");
            return false;
          }
        }
      }

      Console.WriteLine($"Ending replay at frame {container.session.FramePredicted.Number}");

      checksumVerification?.Dispose();
      container.Destroy();

      return true;
    }
  }
}
