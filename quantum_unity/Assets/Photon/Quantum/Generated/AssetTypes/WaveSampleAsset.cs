// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial  
// declarations in another file.
// </auto-generated>

using Quantum;
using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/WaveSample", order = Quantum.EditorDefines.AssetMenuPriorityStart + 572)]
public partial class WaveSampleAsset : AssetBase {
  public Quantum.WaveSample Settings;

  public override Quantum.AssetObject AssetObject => Settings;
  
  public override void Reset() {
    if (Settings == null) {
      Settings = new Quantum.WaveSample();
    }
    base.Reset();
  }
}

public static partial class WaveSampleAssetExts {
  public static WaveSampleAsset GetUnityAsset(this WaveSample data) {
    return data == null ? null : UnityDB.FindAsset<WaveSampleAsset>(data);
  }
}
