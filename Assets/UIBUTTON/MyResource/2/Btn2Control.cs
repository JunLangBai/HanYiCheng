using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn2Control : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float speed = .3f;
    private bool flag;
    private Material mat;
    private float offsetValue;

    private void Start()
    {
        GetComponent<Image>().material = new Material(GetComponent<Image>().material);
        mat = GetComponent<Image>().material;
        offsetValue = 0;
        flag = false;
    }

    private void Update()
    {
        if (flag)
            offsetValue += Time.deltaTime * speed;
        mat.SetFloat("_OffsetX", offsetValue);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        flag = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        flag = false;
    }
}