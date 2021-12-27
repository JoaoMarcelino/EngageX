using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  [SerializeField]
  private Transform _player;
  public Vector3 offset;
  
  public void SetTarget(Transform target)
  {
      _player = target;
  }
  void LateUpdate () 
  {
      transform.position = new Vector3 (_player.position.x + offset.x, _player.position.y + offset.y, offset.z); // Camera follows the player with specified offset position
  }
}
