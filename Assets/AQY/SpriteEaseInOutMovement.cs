using UnityEngine;

public class SpriteUpDownMovement : MonoBehaviour
{
    // 基础移动速率
    public float speed = 1.0f;

    // 移动范围（上下移动的最大距离）
    public float moveRange = 2.0f;

    // 初始位置
    private Vector3 startPosition;

    void Start()
    {
        // 记录初始位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 更新时间变量
        float t = Mathf.PingPong(Time.time * speed, 1f);

        // 缓进急出的核心逻辑：使用 Mathf.SmoothStep
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // 根据 smoothT 计算新的 Y 坐标
        float newY = Mathf.Lerp(startPosition.y - moveRange / 2, startPosition.y + moveRange / 2, smoothT);

        // 更新位置
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}