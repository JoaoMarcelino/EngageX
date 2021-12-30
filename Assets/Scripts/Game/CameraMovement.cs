using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  [SerializeField] private Transform _target;

  public void SetTarget(Transform target)
  {
      _target = target;
  }
  
  [SerializeField] private Vector3 _offset;

  void LateUpdate () 
  {
      transform.position = new Vector3 (_target.position.x + _offset.x, _target.position.y + _offset.y, -10.0f); // Camera follows the player with specified offset position
  }
}
