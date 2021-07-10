using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour {

  [SerializeField] private CinemachineVirtualCamera _virtualCamera;

  public static VirtualCameraManager Instance;
  
  // Start is called before the first frame update
  void Awake() {
    Instance = this;
  }

  public void SetFollowTarget(Transform target) {
    _virtualCamera.Follow = target;
  }
}