using Rokid.UXR.Interaction;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Demo
{
    public class ImageTrackingSample : MonoBehaviour
    {
        [SerializeField]
        private GameObject leftUpAnchor;
        [SerializeField]
        private GameObject rightUpAnchor;
        [SerializeField]
        private GameObject leftBottomAnchor;
        [SerializeField]
        private GameObject rightBottomAnchor;
        [SerializeField]
        private GameObject quad;

        private bool isInitialize = false;
        void Start()
        {
            Assert.IsNotNull(leftUpAnchor);
            Assert.IsNotNull(rightUpAnchor);
            Assert.IsNotNull(leftBottomAnchor);
            Assert.IsNotNull(rightBottomAnchor);
            Assert.IsNotNull(quad);

            if (isInitialize == false)
            {
                isInitialize = true;
                leftUpAnchor.gameObject.SetActive(false);
                rightUpAnchor.gameObject.SetActive(false);
                leftBottomAnchor.gameObject.SetActive(false);
                rightBottomAnchor.gameObject.SetActive(false);
                quad.gameObject.SetActive(false);
            }
        }


        public void OnTrackedImageAdded(ARTrackedImageObj trackedImage)
        {
            isInitialize = true;
            leftUpAnchor.gameObject.SetActive(true);
            rightUpAnchor.gameObject.SetActive(true);
            leftBottomAnchor.gameObject.SetActive(true);
            rightBottomAnchor.gameObject.SetActive(true);
            quad.gameObject.SetActive(true);
            OnTrackedImageUpdated(trackedImage);
        }
        public void OnTrackedImageUpdated(ARTrackedImageObj trackedImageObj)
        {
            leftUpAnchor.transform.position = trackedImageObj.transform.position + trackedImageObj.transform.rotation * new Vector3(-trackedImageObj.trackedImage.bounds.extents.x, trackedImageObj.trackedImage.bounds.extents.y, 0);
            rightUpAnchor.transform.position = trackedImageObj.transform.position + trackedImageObj.transform.rotation * new Vector3(trackedImageObj.trackedImage.bounds.extents.x, trackedImageObj.trackedImage.bounds.extents.y, 0);
            leftBottomAnchor.transform.position = trackedImageObj.transform.position + trackedImageObj.transform.rotation * new Vector3(-trackedImageObj.trackedImage.bounds.extents.x, -trackedImageObj.trackedImage.bounds.extents.y, 0);
            rightBottomAnchor.transform.position = trackedImageObj.transform.position + trackedImageObj.transform.rotation * new Vector3(trackedImageObj.trackedImage.bounds.extents.x, -trackedImageObj.trackedImage.bounds.extents.y, 0);
            quad.transform.localScale = new Vector3(trackedImageObj.trackedImage.size.x / transform.parent.localScale.x, trackedImageObj.trackedImage.size.y / transform.parent.localScale.y, 1);
            quad.transform.position = trackedImageObj.transform.position;
            quad.transform.rotation = trackedImageObj.transform.rotation;
        }
        public void OnTrackedImageRemoved(ARTrackedImageObj trackedImage)
        {
            leftUpAnchor.gameObject.SetActive(false);
            rightUpAnchor.gameObject.SetActive(false);
            leftBottomAnchor.gameObject.SetActive(false);
            rightBottomAnchor.gameObject.SetActive(false);
            quad.gameObject.SetActive(false);
        }
    }

}
