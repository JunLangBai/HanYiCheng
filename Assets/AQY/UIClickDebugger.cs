// UIClickDebugger.cs
using UnityEngine;
using UnityEngine.UI;

public class UIClickDebugger : MonoBehaviour
{
    public Color debugColor = new Color(1, 0, 0, 0.3f);
    
    void Start()
    {
        // 为所有可交互UI添加调试层
        Selectable[] allSelectables = FindObjectsOfType<Selectable>();
        foreach (Selectable selectable in allSelectables)
        {
            AddDebugOverlay(selectable.gameObject);
        }
    }

    void AddDebugOverlay(GameObject target)
    {
        GameObject debugObj = new GameObject("ClickDebug");
        debugObj.transform.SetParent(target.transform, false);
        
        Image debugImg = debugObj.AddComponent<Image>();
        debugImg.color = debugColor;
        debugImg.raycastTarget = false; // 防止干扰实际点击
        
        RectTransform rt = debugObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}