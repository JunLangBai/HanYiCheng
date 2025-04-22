using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public GameObject a;
    // Update is called once per frame
    void Update()
    {
        // 计算从UI图片到目标物体的方向
        Vector3 direction = a.transform.position - transform.position;

        // 使UI图片的正面朝向目标物体
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
