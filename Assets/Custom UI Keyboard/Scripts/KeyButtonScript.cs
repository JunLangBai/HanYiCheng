using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script for key button
/// </summary>
public class KeyButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public KeyboardManager manager; // Reference to keyboard manager

    public KeyClass.KeyType keyType; // Current key type
    public Vector2 keySize; // Key size
    public string keyValue; // Key value 

    public TextMeshProUGUI targetTextKey; // Reference to text on key
    public Image targetImageKey; // Reference to image on key

    public Image shadowImage; // Reference to shadow (glow) on key

    /// <summary>
    /// Initialization of key
    /// </summary>
    /// <param name="keySize">Key size</param>
    /// <param name="keyValue">Key value</param>
    /// <param name="manager">Keyboard manager</param>
    /// <param name="buttonsNormalColor">Buttons normal color </param>
    /// <param name="buttonsHightlightedColor">Buttons hightlighted color </param>
    /// <param name="buttonsPressedColor">Buttons pressed color</param>
    /// <param name="buttonsSelectedColor">Buttons selected color</param>
    /// <param name="keysTextColor">Keys text color</param>
    /// <param name="glowColor">Glow color</param>
    /// <param name="currentFont">Font for text</param>
    /// <param name="currentFontSize">Font size for text</param>
    public void InitKey(Vector2 keySize, string keyValue, KeyboardManager manager,
        Color buttonsNormalColor, Color buttonsHightlightedColor, Color buttonsPressedColor,
        Color buttonsSelectedColor, Color keysTextColor, Color glowColor, TMP_FontAsset currentFont, float currentFontSize) {
        this.keySize = keySize;
        this.manager = manager;
        this.keyValue = keyValue;
        if(keyType == KeyClass.KeyType.Character || keyType == KeyClass.KeyType.EmojiCharacter) {
            targetTextKey.text = keyValue;
        }
        GetComponent<RectTransform>().sizeDelta = keySize;
        var button = GetComponent<Button>();
        var colorsButton = new ColorBlock();
        colorsButton.normalColor = buttonsNormalColor;
        colorsButton.highlightedColor = buttonsHightlightedColor;
        colorsButton.pressedColor = buttonsPressedColor;
        colorsButton.selectedColor = buttonsSelectedColor;
        colorsButton.disabledColor = buttonsPressedColor;
        colorsButton.colorMultiplier = 1;
        colorsButton.fadeDuration = 0.1f;
        button.colors = colorsButton;
        if (targetTextKey != null) {
            targetTextKey.color = keysTextColor;
            targetTextKey.font = currentFont;
            targetTextKey.fontSize = currentFontSize;
        }
        if (targetImageKey != null) {
            targetImageKey.color = keysTextColor;
        }
        shadowImage.color = glowColor;
    }

    bool isShifted;

    /// <summary>
    /// Set shift state
    /// </summary>
    /// <param name="isShifted">Is shift enabled</param>
    public void SetShiftState(bool isShifted) {
        this.isShifted = isShifted;
        targetTextKey.fontStyle = isShifted ? FontStyles.UpperCase : FontStyles.LowerCase;
    }

    /// <summary>
    /// Press key button
    /// </summary>
    public void PressButton() {
        switch (keyType) {
            case KeyClass.KeyType.Character:
                manager.PressKey(isShifted ? keyValue.ToUpper() : keyValue.ToLower());
                break;
            case KeyClass.KeyType.EmojiCharacter:
                manager.PressKey(keyValue);
                break;
            case KeyClass.KeyType.Backspace:
                manager.PressKey("Backspace");
                break;
            case KeyClass.KeyType.Enter:
                manager.PressKey("Enter");
                break;
            case KeyClass.KeyType.Space:
                manager.PressKey("Space");
                break;
            case KeyClass.KeyType.Shift:
                manager.PressShift();
                break;
            case KeyClass.KeyType.Emoji:
                manager.OpenEmojiKeyboard();
                break;
            case KeyClass.KeyType.AdditionalSymbols:
                manager.OpenAdditionalKeyboard();
                break;
            case KeyClass.KeyType.MainCharacters:
                manager.OpenMainKeyboard();
                break;
        }
    }

    public void OnPointerDown (PointerEventData eventData) {
        manager.isPressKeyboard = true;
    }

    public void OnPointerUp (PointerEventData eventData) {
        manager.isPressKeyboard = false;
    }
}
