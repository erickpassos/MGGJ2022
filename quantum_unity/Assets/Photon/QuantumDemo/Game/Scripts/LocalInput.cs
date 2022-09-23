using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class LocalInput : MonoBehaviour {
    
  private void OnEnable() {
    QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
  }

  public void PollInput(CallbackPollInput callback) {
    Quantum.Input i = new Quantum.Input();

    var forward = UnityEngine.Input.GetAxis("Vertical");
    if (forward > 0) i.Forward = true;
    else if (forward < 0) i.Backward = true;
    
    var x = UnityEngine.Input.GetAxis("Horizontal");
    if (x > 0) i.Right = true;
    else if (x < 0) i.Left = true;

    i.Use = UnityEngine.Input.GetButton("Jump");
    
    callback.SetInput(i, DeterministicInputFlags.Repeatable);
  }
}
