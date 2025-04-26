#if UNITY_EDITOR
using UnityEngine;

namespace LeTai.Asset.TranslucentImage
{
    [CreateAssetMenu(menuName = "Translucent Image/Default Resources")]
    public class DefaultResources : ScriptableObject
    {
        private static DefaultResources instance;

        public Material material;

        public static DefaultResources Instance
        {
            get
            {
                if (!instance)
                    instance = Resources.Load<DefaultResources>("Translucent Image Default Resources");
                return instance;
            }
        }
    }
}
#endif