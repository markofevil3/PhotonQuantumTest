using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  public class ReplayMenu {
    private static string ReplayLocation {
      get => EditorPrefs.GetString("Quantum_Export_LastReplayLocation");
      set => EditorPrefs.SetString("Quantum_Export_LastReplayLocation", value);
    }

    private static string SavegameLocation {
      get => EditorPrefs.GetString("Quantum_Export_LastSavegameLocation");
      set => EditorPrefs.SetString("Quantum_Export_LastSavegameLocation", value);
    }

    private static string DBLocation {
      get => EditorPrefs.GetString("Quantum_Export_LastDBLocation");
      set => EditorPrefs.SetString("Quantum_Export_LastDBLocation", value);
    }

    [MenuItem("Quantum/Export/Replay (JSON) %#r", true, 3)]
    public static bool ExportReplayAndDBToJSONCheck() {
      return Application.isPlaying;
    }

    [MenuItem("Quantum/Export/Replay (JSON) %#r", false, 3)]
    public static void ExportReplayAndDBToJSON() {
      ExportDialogReplayAndDB(QuantumRunner.Default.Game, new QuantumUnityJsonSerializer(), ".json");
    }

    [MenuItem("Quantum/Export/Savegame (JSON)", true, 3)]
    public static bool SaveGameCheck() {
      return Application.isPlaying;
    }

    [MenuItem("Quantum/Export/Savegame (JSON)", false, 3)]
    public static void SaveGame() {
      ExportDialogSavegame(QuantumRunner.Default.Game, new QuantumUnityJsonSerializer(), ".json");
    }

    [MenuItem("Quantum/Export/DB (JSON) %#t", false, 3)]
    public static void ExportDB() {
      var lastLocation = DBLocation;
      var directory = string.IsNullOrEmpty(lastLocation) ? Application.dataPath : Path.GetDirectoryName(lastLocation);
      var defaultName = string.IsNullOrEmpty(lastLocation) ? "db" : Path.GetFileName(lastLocation);

      var folderPath = EditorUtility.SaveFolderPanel("Save db to..", directory, defaultName);
      if (!string.IsNullOrEmpty(folderPath)) {
        var serializer = new QuantumUnityJsonSerializer();
        QuantumGame.ExportDatabase(UnityDB.ResourceManager.LoadAllAssets(true), serializer, folderPath, MapDataBaker.NavMeshSerializationBufferSize, dbExtension: ".json");
        DBLocation = folderPath;
      }
    }

    // TODO: add interface for replay serializer
    public static void ExportDialogReplayAndDB(QuantumGame game, QuantumUnityJsonSerializer serializer, string ext) {
      Assert.Check(serializer != null, "Serializer is invalid");

      var defaultName = "replay";
      var directory = Application.dataPath;
      if (!string.IsNullOrEmpty(ReplayLocation)) {
        defaultName = Path.GetFileName(ReplayLocation);
        directory   = ReplayLocation.Remove(ReplayLocation.IndexOf(defaultName, StringComparison.Ordinal));
      }

      var folderPath = EditorUtility.SaveFolderPanel("Save replay and db to..", directory, defaultName);
      if (!string.IsNullOrEmpty(folderPath)) {

        var replay = game.GetRecordedReplay();
        Debug.Assert(replay != null);

        File.WriteAllBytes(Path.Combine(folderPath, "replay" + ext), serializer.SerializeReplay(replay));

        QuantumGame.ExportDatabase(UnityDB.ResourceManager.LoadAllAssets(true), serializer, folderPath, MapDataBaker.NavMeshSerializationBufferSize, dbExtension: ext);

        if (game.RecordedChecksums != null) {
          File.WriteAllBytes(Path.Combine(folderPath, "checksum" + ext), serializer.SerializeChecksum(game.RecordedChecksums));
        }

        Debug.Log("Saved replay to " + folderPath);
        AssetDatabase.Refresh();

        ReplayLocation = folderPath;
      }
    }

    public static void ExportDialogSavegame(QuantumGame game, QuantumUnityJsonSerializer serializer, string ext) {
      Assert.Check(serializer != null, "Serializer is invalid");

      var defaultName = "savegame";
      var directory = Application.dataPath;
      if (!string.IsNullOrEmpty(SavegameLocation)) {
        defaultName = Path.GetFileName(SavegameLocation);
        directory = SavegameLocation.Remove(SavegameLocation.IndexOf(defaultName, StringComparison.Ordinal));
      }

      var folderPath = EditorUtility.SaveFolderPanel("Save game to..", directory, defaultName);
      if (!string.IsNullOrEmpty(folderPath)) {
        var replay = game.CreateSavegame();
        File.WriteAllBytes(Path.Combine(folderPath, "savegame" + ext), serializer.SerializeReplay(replay));

        QuantumGame.ExportDatabase(UnityDB.ResourceManager.LoadAllAssets(true), serializer, folderPath, MapDataBaker.NavMeshSerializationBufferSize, dbExtension: ext);

        Debug.Log("Saved game to " + folderPath);
        AssetDatabase.Refresh();
      }

      SavegameLocation = folderPath;
    }
  }
}