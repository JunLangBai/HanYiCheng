using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rokid.UXR.Demo
{
    public class SetChildSibilingIndex : MonoBehaviour
    {
        [SerializeField]
        private Text text;
        void Start()
        {
            if (text == null)
            {
                text = GetComponentInChildren<Text>();
            }
            if (text != null)
                text.text = transform.GetSiblingIndex() + "";
        }
    }
}
