using UnityEditor;
namespace AcceptSlider {
	public static class ContextMenuUtility {
		[MenuItem("GameObject/UIComponents/Slider Accept")]
		public static void CreateSwitcher(MenuCommand menuCommand) {
			CreateUtility.CreateUIElement("SliderAccept");
		}

		[MenuItem("GameObject/UIComponents/Slider Answer")]
		public static void CreateSwitcherOutlined(MenuCommand menuCommand) {
			CreateUtility.CreateUIElement("SliderAnswer");
		}
	}
}