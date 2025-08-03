using UnityEngine;
using System.Collections;

namespace Hessburg
{
    public class DemoRotations : MonoBehaviour
    {
        public Transform SunLight;
        public Transform CamParent;
        public Transform Triangle1;
        public Transform Triangle2;
        public Transform Triangle3;

        void Update()
        {
            SunLight.Rotate(new Vector3(20.0f * Time.deltaTime, 15.0f * Time.deltaTime, 0.0f), Space.World);
            Triangle1.Rotate(new Vector3(53.0f * Time.deltaTime, 50.0f * Time.deltaTime, 0.0f), Space.World);
            Triangle2.Rotate(new Vector3(50.0f * Time.deltaTime, 50.0f * Time.deltaTime, 0.0f), Space.World);
            Triangle3.Rotate(new Vector3(47.0f * Time.deltaTime, 50.0f * Time.deltaTime, 0.0f), Space.World);
            CamParent.Rotate(new Vector3(0.0f, -45.0f * Time.deltaTime, 0.0f), Space.World);
        }
    }
}
