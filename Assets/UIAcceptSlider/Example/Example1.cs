using UnityEngine;
namespace AcceptSlider {
	public class Example1 : MonoBehaviour {
		[SerializeField] private UIAcceptSlider answerSlider, acceptSlider;
		[SerializeField] private GameObject panelCall, panelPopup;

		private void Awake() {
			answerSlider.onAccept.AddListener(OnAnswer);
			acceptSlider.onAccept.AddListener(OnAccept);
		}
		private void OnAnswer() {
			panelCall.gameObject.SetActive(false);
			panelPopup.gameObject.SetActive(true);
		}
		private void OnAccept() {
			panelCall.gameObject.SetActive(true);
			panelPopup.gameObject.SetActive(false);
		}
	}
}