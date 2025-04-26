using UnityEngine;

[ExecuteAlways]
public class ScreenSpacePlacement : MonoBehaviour
{
    [SerializeField] private Camera m_Cam;

    [SerializeField] private Transform m_FlareObject;

    private bool m_MouseDown;

    private void OnGUI()
    {
        var currentEvent = Event.current;
        var mousePos = new Vector2();

        if (currentEvent.type == EventType.MouseDown) m_MouseDown = true;
        if (currentEvent.type == EventType.MouseUp) m_MouseDown = false;

        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = m_Cam.pixelHeight - currentEvent.mousePosition.y;

        if (m_FlareObject != null && mousePos.x > 0 && mousePos.y > 0 && mousePos.x < m_Cam.pixelWidth &&
            mousePos.y < m_Cam.pixelHeight)
        {
            var point = m_Cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, m_Cam.nearClipPlane));

            if (m_MouseDown) m_FlareObject.position = point;
        }
    }
}