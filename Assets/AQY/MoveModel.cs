using System;
using System.Collections;
using System.Collections.Generic;
using Rokid.UXR.Interaction;
using UnityEngine;

public class MoveModel : MonoBehaviour
{
    
    [SerializeField] private Vector3 axis = Vector3.forward; // 旋转轴 (默认Y轴)
    [SerializeField] private float degreesPerSecond = 90f; // 每秒旋转角度

    void Update()
    {
        transform.Rotate(axis, degreesPerSecond * Time.deltaTime);
    }
    
    
}
