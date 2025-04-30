using UnityEditor;

namespace AcceptSlider
{
	[CustomEditor(typeof(UIAcceptSlider))]
	public class UIAcceptSliderEditor : UnityEditor.UI.SelectableEditor
	{
		private SerializedProperty _onAccept, _onReject, _fillRect, _handleRect, _onValueChanged;
		private UIAcceptSlider _uiAcceptSlider;

		protected override void OnEnable() {
			base.OnEnable();
			_onValueChanged = serializedObject.FindProperty("m_OnValueChanged");
			_fillRect = serializedObject.FindProperty("m_FillRect");
			_handleRect = serializedObject.FindProperty("m_HandleRect");
			_onAccept = serializedObject.FindProperty("onAccept");
			_onReject = serializedObject.FindProperty("onReject");
			_uiAcceptSlider = serializedObject.targetObject as UIAcceptSlider;
		}

		public override void OnInspectorGUI() {
			EditorGUI.BeginChangeCheck();

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(_uiAcceptSlider);

			base.OnInspectorGUI();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_fillRect);
			EditorGUILayout.PropertyField(_handleRect);
			EditorGUILayout.PropertyField(_onValueChanged);
			EditorGUILayout.PropertyField(_onAccept, true);
			EditorGUILayout.PropertyField(_onReject, true);
		}
	}
}