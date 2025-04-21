using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR;
using Rokid.UXR.Native;
using UnityEngine.SceneManagement;
using Rokid.UXR.Module;

namespace Rokid.UXR.Demo
{
    public class VirtualControllerSample : MonoBehaviour
    {
        public GameObject cube;

        public Text startInfo;
        public Text leftRockerInfo;
        public Text rightRockerInfo;

        private float rotateSpeed = 1f;
        private int moveSpeed = 2;
        private int colorIndex = 0;
        private List<Color> colors = new List<Color>() { Color.red, Color.blue, Color.yellow, Color.green, Color.gray };


        void Start()
        {
            // Configures the app to not shut down the screen
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            RKVirtualController.Instance.Change(ControllerType.GAMEPAD);

            //Use RKInput instead of UnityEngine.Input for ABXY KeyEvent and Axis event, true as default.
            RKVirtualController.Instance.UseCustomGamePadEvent(true);
        }

        void Update()
        {

            if (RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_OK))
            {
                cube.transform.localPosition = new Vector3(0, 1, 0);
            }

            if (RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_X))
            {
                cube.transform.Rotate(new Vector3(0, rotateSpeed, 0));
            }

            if (RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_B))
            {
                cube.transform.Rotate(new Vector3(0, -rotateSpeed, 0));
            }

            if (RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_Y))
            {
                colorIndex++;
                cube.GetComponent<MeshRenderer>().material.color = colors[Mathf.Abs(colorIndex) % colors.Count];
            }

            if (RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_A))
            {
                colorIndex--;
                cube.GetComponent<MeshRenderer>().material.color = colors[Mathf.Abs(colorIndex) % colors.Count];
            }

            if (RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_LEFT) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                cube.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);
            }

            if (RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_RIGHT) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                cube.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
            }

            if (RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_UP) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                cube.transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
            }

            if (RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_DOWN) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                cube.transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
            }


            //RKLog.Info("UXR-PLUGIN::lr axis = " + RKInput.GetAxis(AxisEvent.Horizontal_Left) + ", " + RKInput.GetAxis(AxisEvent.Vertical_Left));
            leftRockerInfo.text = "Left Horizontal Axis: " + RKNativeInput.Instance.GetAxis(AxisEvent.Horizontal_Left) + ", Vertical Axis: " + RKNativeInput.Instance.GetAxis(AxisEvent.Vertical_Left);
            rightRockerInfo.text = "Right Horizontal Axis: " + RKNativeInput.Instance.GetAxis(AxisEvent.Horizontal_Right) + ", Vertical Axis: " + RKNativeInput.Instance.GetAxis(AxisEvent.Vertical_Right);
        }
    }
}
