using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rokid.UXR.Demo
{
    public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image containerImage;
        public Image receivingImage;
        public Color highlightColor = Color.yellow;
        private readonly Color normalColor = Color.white;

        public void OnDrop(PointerEventData data)
        {
            containerImage.color = normalColor;

            if (receivingImage == null)
                return;

            var dropImage = GetDropSprite(data);
            if (dropImage != null)
            {
                receivingImage.overrideSprite = dropImage.sprite;
                receivingImage.color = dropImage.color;
            }
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (containerImage == null)
                return;

            var dropSprite = GetDropSprite(data)?.sprite;
            if (dropSprite != null)
                containerImage.color = highlightColor;
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (containerImage == null)
                return;

            containerImage.color = normalColor;
        }

        private Image GetDropSprite(PointerEventData data)
        {
            var originalObj = data.pointerDrag;
            if (originalObj == null)
                return null;

            var dragMe = originalObj.GetComponent<DragMe>();
            if (dragMe == null)
                return null;

            var srcImage = originalObj.GetComponent<Image>();
            srcImage.color = originalObj.GetComponent<Image>().color;
            if (srcImage == null)
                return null;

            return srcImage;
        }
    }
}