using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
  void Awake()
  {
    Application.targetFrameRate = 60;
  }
}