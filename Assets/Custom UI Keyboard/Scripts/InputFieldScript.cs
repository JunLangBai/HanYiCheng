using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Script you need to add to your TMP_InputField if you want to use the UI keyboard
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class InputFieldScript : MonoBehaviour
{
    TMP_InputField inputField; // Current InputField

    [HideInInspector]
    public KeyboardManager targetKeyboard; // Reference to keyboard manager

    /// <summary>
    /// Get current input field
    /// </summary>
    /// <returns></returns>
    public TMP_InputField GetInputField() {
        return inputField;
    }

    /// <summary>
    /// Pressed key button
    /// </summary>
    /// <param name="key">Key pressed</param>
    public void PressKey(string key) {
        switch (key) {
            case "Space":
                if (isSelectingText) {
                    inputField.ProcessEvent(Event.KeyboardEvent("Backspace"));
                }
                inputField.text = inputField.text.Insert(inputField.caretPosition, " ");
                inputField.caretPosition++;
                break;
            case "Backspace":
                inputField.ProcessEvent(Event.KeyboardEvent("Backspace"));
                break;
            case "Enter":
                if (inputField.lineType == TMP_InputField.LineType.SingleLine) {
                    targetKeyboard.canDeselect = true;
                    EventSystem.current.SetSelectedGameObject(null);
                } else {
                    inputField.text = inputField.text.Insert(inputField.caretPosition, "\n");
                    inputField.caretPosition++;
                }
                break;
            default:
                if (isSelectingText) {
                    inputField.ProcessEvent(Event.KeyboardEvent("Backspace"));
                }
                inputField.text = inputField.text.Insert(inputField.caretPosition, key);
                inputField.caretPosition += key.Length;
                break;
        }
        targetKeyboard.CheckShift();
    }

    bool isSelectingText;

    /// <summary>
    /// Start initialization
    /// </summary>
    private void Start() {
        inputField = GetComponent<TMP_InputField>();
        inputField.shouldHideMobileInput = true;
        inputField.shouldHideSoftKeyboard = true;
        inputField.onFocusSelectAll = false;
        inputField.resetOnDeActivation = false;
        inputField.onSelect.AddListener(OnSelected);
        inputField.onDeselect.AddListener(OnDeselected);
        inputField.onTextSelection.AddListener((string text, int pos, int pos2) => {
            isSelectingText = true;
        });
        inputField.onEndTextSelection.AddListener((string text, int pos, int pos2) => {
            isSelectingText = false;
        });
    }

    /// <summary>
    /// On selected input field event
    /// </summary>
    /// <param name="value"></param>
    public void OnSelected(string value) {
    }

    /// <summary>
    /// On deselected input field event
    /// </summary>
    /// <param name="value"></param>
    public void OnDeselected(string value) {
    }

    /// <summary>
    /// Deselect input field manually
    /// </summary>
    public void Deselect() {
        inputField.resetOnDeActivation = true;
        inputField.ReleaseSelection();
        inputField.resetOnDeActivation = false;
    }

    /// <summary>
    /// Select input field
    /// </summary>
    public void SelectInput() {
        inputField.Select();
    }
}
