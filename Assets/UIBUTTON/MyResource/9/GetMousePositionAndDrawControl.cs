using UnityEngine;

[ExecuteInEditMode]
public class GetMousePositionAndDrawControl : MonoBehaviour
{
    private const int MAX_VERT_NUM = 1000;
    [SerializeField] private Material material;
    private bool canDraw;
    private Vector4[] drawPos;
    private int nowDrawPos;

    private void Start()
    {
        canDraw = false;
        drawPos = new Vector4[MAX_VERT_NUM];
        nowDrawPos = 0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) canDraw = true;
        if (Input.GetMouseButtonUp(0)) canDraw = false;
        if (canDraw)
        {
            material.SetVector("_MousePos", Input.mousePosition);
            var size = material.GetFloat("_Size");
            if (nowDrawPos == 0)
            {
                drawPos[nowDrawPos] = Input.mousePosition;
                material.SetInt("_VertNum", nowDrawPos);
                nowDrawPos += 1;
                material.SetVectorArray("_Verts", drawPos);
            }
            else if (nowDrawPos > 0 && nowDrawPos < MAX_VERT_NUM &&
                     Vector2.Distance(Input.mousePosition, drawPos[nowDrawPos - 1]) > size * 0.5f)
            {
                drawPos[nowDrawPos] = Input.mousePosition;
                material.SetInt("_VertNum", nowDrawPos);
                nowDrawPos += 1;
                material.SetVectorArray("_Verts", drawPos);
            }
            else
            {
                Debug.Log("nowDrawPos:" + nowDrawPos);
            }
            //material.set
        }
    }

    private void OnDestroy()
    {
        canDraw = false;
    }
}