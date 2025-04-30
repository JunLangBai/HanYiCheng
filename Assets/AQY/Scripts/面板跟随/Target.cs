using UnityEngine;

public class Target : MonoBehaviour
{
    public GameObject a; // 目标物体
    public Vector3 offset; // 当前物体相对于目标物体的偏移量

    // Update is called once per frame
    private void Update()
    {
        // 计算从UI图片到目标物体的方向
        var direction = a.transform.position - transform.position;

        // 使UI图片的正面朝向目标物体
        transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);

        // 设置当前物体的位置为a物体的位置加上偏移量
        transform.position = a.transform.position + offset;
    }
}