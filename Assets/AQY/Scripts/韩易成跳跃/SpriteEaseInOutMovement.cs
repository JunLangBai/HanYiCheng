using UnityEngine;

public class SpriteUpDownMovement : MonoBehaviour
{
    public Transform onlyTextPosition;
    public Transform optionsTextPosition;

    private float timer;

    private void Start()
    {
    }

    private void Update()
    {
        if (GlobalTutorialsManager.instance.canNextText)
        {
            // 更新物体的位置
            transform.position = onlyTextPosition.position;
            
        }
        else
        {
            transform.position = optionsTextPosition.position;
            transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        }
    }
}