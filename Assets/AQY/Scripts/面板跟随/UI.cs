using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject a;

    private void Update()
    {
        var nx = a.transform.position.x;
        var ny = a.transform.position.y + 1f;
        transform.position = new Vector3(nx, ny, a.transform.position.z);
    }
}