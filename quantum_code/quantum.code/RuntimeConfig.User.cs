using Photon.Deterministic;
using System;

namespace Quantum {
  partial class RuntimeConfig
  {
    public AssetRefWaveSample WaveSample;

    partial void SerializeUserData(BitStream stream)
    {
      stream.Serialize(ref WaveSample);
    }
  }
}