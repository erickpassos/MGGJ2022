using Quantum;
using UnityEngine;

public unsafe class BoatView : MonoBehaviour
{
  public EntityView View;
  public bool InitCameraOnStart;
  public Transform RudderView;
  
  public Vector3 CameraOffset;
  public float CameraFollowSpeed;
  private PlayerRef _player;
  private Transform Camera;
  

  public void OnInstantiated(QuantumGame game)
  {
    var frame = QuantumRunner.Default?.Game.Frames.Verified;
    if (frame != null)
    {
      var controls = frame.Unsafe.GetPointer<BoatControl>(View.EntityRef);
      if (InitCameraOnStart)
      {
        InitCamera(controls->Player, View.EntityRef);
      }
    }
  }
  void InitCamera(PlayerRef player, EntityRef entity)
  {
    if (QuantumRunner.Default.Session.IsLocalPlayer(player) && entity == View.EntityRef)
    {
      Camera = FindObjectOfType<Camera>().transform;
      FindObjectOfType<WaterSampler>().Anchor = transform;
    }
  }

  void LateUpdate()
  {
    var frame = QuantumRunner.Default?.Game.Frames.Predicted;
    if (frame != null)
    {
      var boat = frame.Unsafe.GetPointer<Boat>(View.EntityRef);
      RudderView.localRotation = Quaternion.Euler(0, boat->CurrentRudderAngle.AsFloat, 0);
    }
    
    
    if (!Camera) return;
    var desired = transform.TransformPoint(CameraOffset);
    Camera.position = Vector3.Lerp(Camera.position, desired, Time.deltaTime * CameraFollowSpeed);
    Camera.LookAt(transform);
    
    
  }

}