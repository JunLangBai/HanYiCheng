using UnityEngine;

public class SpriteUpDownMovement : MonoBehaviour
{
   public float jumpHeight = 2f; // 跳跃高度
       public float jumpDuration = 1f; // 跳跃持续时间
       public float rotationSpeed = 180f; // 左右旋转速度
       public float horizontalOffset = 1f; // 水平偏移量
   
       private Vector3 startPosition;
       private float timer = 0f;
   
       void Start()
       {
           startPosition = transform.position; // 记录初始位置
       }
   
       void Update()
       {
           // 更新计时器
           timer += Time.deltaTime;
   
           // 计算跳跃的垂直高度（使用正弦函数模拟抛物线）
           float normalizedTime = Mathf.PingPong(timer, jumpDuration) / jumpDuration; // [0, 1] 循环
           float height = jumpHeight * Mathf.Sin(normalizedTime * Mathf.PI); // 抛物线高度
   
           // 计算水平偏移（使用正弦函数模拟左右摆动）
           float horizontalMovement = Mathf.Sin(normalizedTime * Mathf.PI * 2) * horizontalOffset;
   
           // 更新物体的位置
           transform.position = startPosition + new Vector3(horizontalMovement, height, 0);
   
           // 左右旋转
           float rotationAmount = rotationSpeed * Time.deltaTime;
           transform.Rotate(Vector3.up, rotationAmount);
       }
}