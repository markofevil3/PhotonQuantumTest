using System.IO;

namespace Quantum {
  public class UnityFileLoader : IFileLoader {
    private readonly string relativePath;
    public UnityFileLoader(string relativePath) {
      this.relativePath = relativePath;
#if !UNITY_EDITOR
      // Outside the editor we can only load files from resources. 
      // So strip the Assets/Resources part of the path.
      Quantum.PathUtils.MakeRelativeToFolder(this.relativePath, "Resources", out this.relativePath);
#endif
    }

    public byte[] Load(string path) {
      if (path.Length == 0) {
        UnityEngine.Debug.LogError("File path is invalid");
        return new byte[0];
      }

#if UNITY_EDITOR
      var formattedPath = Path.GetFullPath(Path.Combine(relativePath, path));
      return File.ReadAllBytes(formattedPath);
#else
      // Loading files from the resource folder without file extension
      var formattedPath = Path.Combine(relativePath, path.Split('.')[0]);
      var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>(formattedPath);
      if (asset != null) 
        return asset.bytes;
      else
        return new byte[0];
#endif
    }
  }
}
