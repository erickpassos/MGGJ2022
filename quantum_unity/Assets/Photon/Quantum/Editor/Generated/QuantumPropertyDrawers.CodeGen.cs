// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
// </auto-generated>

namespace Quantum.Editor {
  using Quantum;
  using UnityEngine;
  using UnityEditor;

  [CustomPropertyDrawer(typeof(AssetRefBoatConfig))]
  public class AssetRefBoatConfigPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(BoatConfigAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefWaveSampleBase))]
  public class AssetRefWaveSampleBasePropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(WaveSampleBaseAsset));
    }
  }

  [CustomPropertyDrawer(typeof(Quantum.Prototypes.InputButtons_Prototype))]
  partial class PrototypeDrawer {}
}
