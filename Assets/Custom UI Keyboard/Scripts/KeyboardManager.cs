using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Class for abstract keys
/// </summary>
[Serializable]
public class KeyClass {
    public enum KeyType { // Key Types
        Character,
        EmojiCharacter,
        Backspace,
        Enter,
        Space,
        Shift,
        Emoji,
        AdditionalSymbols,
        MainCharacters
    }

    public KeyType keyType = KeyType.Character; // Current key type
    public bool isUseDifferentSize; // Is need to use different size or size from KeyLineClass
    public Vector2 keySize; // That differect size
    public string keyValue = ""; // Key value (If selected key type Charancter or EmojiCharacter)
    public KeyButtonScript keyButtonScript; // Script reference to key object
}

/// <summary>
/// Key line class
/// </summary>
[Serializable]
public class KeyLineClass {
    public Vector2 keysSize; // Default keys size
    public List<KeyClass> keys; // List of keys in the line
}

/// <summary>
/// Theme settings
/// </summary>
[Serializable]
public class ThemeClass {
    public string themeName; // Name of the theme
    public Color backgroundColor; // Color for keyboard background
    public Texture2D backgroundTexture; // Texture for keyboard background
    public Color buttonsNormalColor; // Normal color for key buttons
    public Color buttonsHightlightedColor; // Hightlighted color for key buttons
    public Color buttonsPressedColor; // Pressed color for key buttons
    public Color buttonsSelectedColor; // Selected color for key buttons
    public Color keysTextColor; // Color of text on buttons
    public Color glowColor; // Color of glowing on buttons
}

/// <summary>
/// Keyboard Manager
/// </summary>
[ExecuteInEditMode]
public class KeyboardManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [Header("Key list")]
    public List<KeyLineClass> keyList; // Key list
    public List<KeyLineClass> additionalKeyList; // Additional key list
    public List<KeyLineClass> emojiKeyList; // Emoji key list

    [Header("Prefabs and Parent")]
    public GameObject templateCharacterButton; // Character button prefab
    public GameObject templateEmojiCharacterButton; // Emoji Character button prefab
    public GameObject templateBackspaceButton; // Backspace button prefab
    public GameObject templateEnterButton; // Enter button prefab
    public GameObject templateSpaceButton; // Space button prefab
    public GameObject templateShiftButton; // Shift button prefab
    public GameObject templateEmojiButton; // Emoji button prefab
    public GameObject templateAdditionalSymbolsButton; // Additional Symbols button prefab
    public GameObject templateMainCharactersButton; // Main Characters button prefab

    public GameObject templateKeyboardLine; // Keyboard line prefab

    public Transform parentLines; // Lines parent

    public RawImage backgroundImage; // Keyboard background image

    public bool isShifted = true; // Is shift enabled

    bool isValidate;

    public enum States { // All available states
        MainKeyboard,
        AdditionalKeyboard,
        EmojiKeyboard
    }

    [Header("Settings")]
    public States currentState = States.MainKeyboard; // Current keyboard state
    public bool isNotHide = false; // If need to not disable keyboard when deselect input field
    public List<ThemeClass> themeList; // Theme list
    public int currentTheme; // Current theme
    public TMP_FontAsset currentFont; // Current font
    public float currentFontSize; // Current font size

    /// <summary>
    /// Start initializing
    /// </summary>
    private void Start() {
        InitKeyboard();
        if (!isNotHide && Application.isPlaying) {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ReInitializing when changing something in inspector
    /// </summary>
    private void Update () {
        if (Application.isPlaying == false)
            return;

        if (isSelectedInputField) {
            if (EventSystem.current.currentSelectedGameObject == null) {
                DeselectInput();
            } else {
                var currentSelection = EventSystem.current.currentSelectedGameObject;
                if (currentSelection.GetComponent<InputFieldScript>()) {
                    currentSelection.GetComponent<InputFieldScript>().targetKeyboard = this;
                    SelectInput(currentSelection.GetComponent<InputFieldScript>());
                } else {
                    DeselectInput();
                }
            }
        } else {
            if (EventSystem.current.currentSelectedGameObject != null) {
                var currentSelection = EventSystem.current.currentSelectedGameObject;
                if (currentSelection.GetComponent<InputFieldScript>()) {
                    currentSelection.GetComponent<InputFieldScript>().targetKeyboard = this;
                    SelectInput(currentSelection.GetComponent<InputFieldScript>());
                }
            }
        }

        if (isValidate) {
            isValidate = false;
            InitKeyboard();
        }
    }

    /// <summary>
    /// Press shift
    /// </summary>
    public void PressShift() {
        isShifted = !isShifted;
        for(int i = 0; i < keyList.Count; i++) {
            for(int j = 0; j < keyList[i].keys.Count; j++) {
                if (keyList[i].keys[j].keyButtonScript != null) {
                    if (keyList[i].keys[j].keyType == KeyClass.KeyType.Character) {
                        keyList[i].keys[j].keyButtonScript.SetShiftState(isShifted);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Select input field
    /// </summary>
    /// <param name="targetInput">Target input field</param>
    public void SelectInput(InputFieldScript targetInput) {
        if (this.targetInput == targetInput) {
            return;
        }
        StartCoroutine(WaitForFrameSelect(targetInput));
    }

    /// <summary>
    /// Wait for next frame to select input field
    /// </summary>
    /// <param name="targetInput"></param>
    /// <returns></returns>
    IEnumerator WaitForFrameSelect(InputFieldScript targetInput) {
        yield return new WaitForEndOfFrame();
        isSelectedInputField = true;
        this.targetInput = targetInput;
        if (!isNotHide) {
            if (transform.GetChild(0).gameObject.activeSelf) {
                transform.GetChild(0).GetComponent<Animator>().SetTrigger("Stay");
            } else {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        CheckShift();
    }

    /// <summary>
    /// Check is need to set upper or lower case every time when we press any button on keyboard
    /// </summary>
    public void CheckShift() {
        if (!isShifted && targetInput.GetInputField().text.Length == 0) {
            PressShift();
        } else if (isShifted && targetInput.GetInputField().text.Length > 0) {
            PressShift();
        }
    }

    /// <summary>
    /// Deselect input field
    /// </summary>
    public void DeselectInput() {
        if (canDeselect) {
            StartCoroutine(WaitForFrame());
            return;
        }
        if (!isSelectedInputField) {
            return;
        }
        StartCoroutine(WaitForFrame());
    }

    [HideInInspector]
    public bool canDeselect;

    /// <summary>
    /// Deselect keyboard
    /// </summary>
    public void DeselectKeyboard() {
        if (!isNotHide) {
            transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
        }
        isSelectedInputField = false;
        targetInput = null;
    }

    /// <summary>
    /// Whait for next frame to deselect input field
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForFrame() {
        yield return new WaitForEndOfFrame();
        if (canDeselect) {
            if (!isNotHide) {
                transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
            }
            isSelectedInputField = false;
            targetInput.Deselect();
            targetInput = null;
            canDeselect = false;
            yield break;
        }
        if (isPressKeyboard) {
            targetInput.SelectInput();
        } else {
            if (!isNotHide) {
                transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
            }
            isSelectedInputField = false;
            targetInput.Deselect();
            targetInput = null;
        }
    }

    bool isSelectedInputField; // Is any input field is selected
    InputFieldScript targetInput; // Selected input field

    /// <summary>
    /// Check changing values in inspector to reinitialize keyboard
    /// </summary>
    private void OnValidate() {
        isValidate = true;
    }

    /// <summary>
    /// Press key on keyboard
    /// </summary>
    /// <param name="key"></param>
    public void PressKey(string key) {
        if(targetInput == null) {
            return;
        }
        targetInput.PressKey(key);
    }

    /// <summary>
    /// Open Additional type keyboard
    /// </summary>
    public void OpenAdditionalKeyboard() {
        currentState = States.AdditionalKeyboard;
        InitKeyboard();
    }
    
    /// <summary>
    /// Open Emoji type keyboard
    /// </summary>
    public void OpenEmojiKeyboard() {
        currentState = States.EmojiKeyboard;
        InitKeyboard();
    }

    /// <summary>
    /// Open Main type keyboard
    /// </summary>
    public void OpenMainKeyboard() {
        currentState = States.MainKeyboard;
        InitKeyboard();
    }

    /// <summary>
    /// Get current type keyboard's lines count
    /// </summary>
    /// <returns></returns>
    int GetKeysCount() {
        switch (currentState) {
            case States.MainKeyboard:
                return keyList.Count;
            case States.AdditionalKeyboard:
                return additionalKeyList.Count;
            case States.EmojiKeyboard:
                return emojiKeyList.Count;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Get current type keyboard
    /// </summary>
    /// <returns></returns>
    List<KeyLineClass> CurrentKeyList() {
        switch (currentState) {
            case States.MainKeyboard:
                return keyList;
            case States.AdditionalKeyboard:
                return additionalKeyList;
            case States.EmojiKeyboard:
                return emojiKeyList;
            default:
                return null;
        }
    }

    /// <summary>
    /// Fit Background image to parent
    /// </summary>
    public void FitToParent() {
        Vector2 rectParent = new Vector2(backgroundImage.transform.parent.GetComponent<RectTransform>().rect.width,
            backgroundImage.transform.parent.GetComponent<RectTransform>().rect.height);
        var texture = backgroundImage.texture;
        Vector2 rectTexture = new Vector2(texture.width, texture.height);
        if (rectParent.x / rectTexture.x > rectParent.y / rectTexture.y) {
            backgroundImage.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTexture.x * (rectParent.x / rectTexture.x) + 5,
                rectTexture.y * (rectParent.x / rectTexture.x) + 5);
        } else {
            backgroundImage.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTexture.x * (rectParent.y / rectTexture.y) + 5,
                rectTexture.y * (rectParent.y / rectTexture.y) + 5);
        }
    }

    /// <summary>
    /// Initialize Keyboard
    /// </summary>
    public void InitKeyboard() {
        backgroundImage.texture = themeList[currentTheme].backgroundTexture;
        backgroundImage.color = themeList[currentTheme].backgroundColor;
        if (backgroundImage.texture != null) {
            FitToParent();
        }
        int lastIDLines = 0;
        for(int i = 0; i < GetKeysCount(); i++) {
            Transform keyboardLine = null;
            if(parentLines.childCount > i) {
                keyboardLine = parentLines.GetChild(i);
                keyboardLine.gameObject.SetActive(true);
            } else {
                keyboardLine = Instantiate(templateKeyboardLine, parentLines).transform;
            }
            lastIDLines++;
            int lastIDKeys = 0;
            for (int j = 0; j < CurrentKeyList()[i].keys.Count; j++) {
                KeyButtonScript keyScript = null;
                if(keyboardLine.childCount > j) {
                    if(keyboardLine.GetChild(j).GetComponent<KeyButtonScript>().keyType == CurrentKeyList()[i].keys[j].keyType) {
                        keyScript = keyboardLine.GetChild(j).GetComponent<KeyButtonScript>();
                        keyScript.InitKey(CurrentKeyList()[i].keys[j].isUseDifferentSize ?
                                CurrentKeyList()[i].keys[j].keySize : CurrentKeyList()[i].keysSize, CurrentKeyList()[i].keys[j].keyValue, this,
                                themeList[currentTheme].buttonsNormalColor, themeList[currentTheme].buttonsHightlightedColor,
                                themeList[currentTheme].buttonsPressedColor, themeList[currentTheme].buttonsSelectedColor,
                                themeList[currentTheme].keysTextColor, themeList[currentTheme].glowColor, currentFont, currentFontSize);
                        keyScript.gameObject.SetActive(true);
                        CurrentKeyList()[i].keys[j].keyButtonScript = keyScript;
                        if (CurrentKeyList()[i].keys[j].keyType == KeyClass.KeyType.Character) {
                            CurrentKeyList()[i].keys[j].keyButtonScript.SetShiftState(isShifted);
                        }
                    } else {
                        switch (CurrentKeyList()[i].keys[j].keyType) {
                            case KeyClass.KeyType.Character:
                                keyScript = Instantiate(templateCharacterButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.EmojiCharacter:
                                keyScript = Instantiate(templateEmojiCharacterButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.Backspace:
                                keyScript = Instantiate(templateBackspaceButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.Enter:
                                keyScript = Instantiate(templateEnterButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.Space:
                                keyScript = Instantiate(templateSpaceButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.Shift:
                                keyScript = Instantiate(templateShiftButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.Emoji:
                                keyScript = Instantiate(templateEmojiButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.AdditionalSymbols:
                                keyScript = Instantiate(templateAdditionalSymbolsButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                            case KeyClass.KeyType.MainCharacters:
                                keyScript = Instantiate(templateMainCharactersButton, keyboardLine).GetComponent<KeyButtonScript>();
                                keyScript.gameObject.SetActive(false);
                                break;
                        }

                        keyScript.InitKey(CurrentKeyList()[i].keys[j].isUseDifferentSize ?
                            CurrentKeyList()[i].keys[j].keySize : CurrentKeyList()[i].keysSize, CurrentKeyList()[i].keys[j].keyValue, this,
                            themeList[currentTheme].buttonsNormalColor, themeList[currentTheme].buttonsHightlightedColor,
                            themeList[currentTheme].buttonsPressedColor, themeList[currentTheme].buttonsSelectedColor,
                            themeList[currentTheme].keysTextColor, themeList[currentTheme].glowColor, currentFont, currentFontSize);

                        keyScript.gameObject.SetActive(true);

                        int childPos = keyboardLine.GetChild(j).GetSiblingIndex();
                        var objToDestroy = keyboardLine.GetChild(j).gameObject;
                        objToDestroy.transform.SetParent(transform);
                        objToDestroy.SetActive(false);
                        if (Application.isPlaying) {
                            Destroy(objToDestroy);
                        } else {
                            DestroyImmediate(objToDestroy);
                        }
                        keyScript.transform.SetSiblingIndex(keyboardLine.GetChild(j).GetSiblingIndex());
                        CurrentKeyList()[i].keys[j].keyButtonScript = keyScript;
                        if (CurrentKeyList()[i].keys[j].keyType == KeyClass.KeyType.Character) {
                            CurrentKeyList()[i].keys[j].keyButtonScript.SetShiftState(isShifted);
                        }
                    }
                } else {
                    switch (CurrentKeyList()[i].keys[j].keyType) {
                        case KeyClass.KeyType.Character:
                            keyScript = Instantiate(templateCharacterButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.EmojiCharacter:
                            keyScript = Instantiate(templateEmojiCharacterButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.Backspace:
                            keyScript = Instantiate(templateBackspaceButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.Enter:
                            keyScript = Instantiate(templateEnterButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.Space:
                            keyScript = Instantiate(templateSpaceButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.Shift:
                            keyScript = Instantiate(templateShiftButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.Emoji:
                            keyScript = Instantiate(templateEmojiButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.AdditionalSymbols:
                            keyScript = Instantiate(templateAdditionalSymbolsButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                        case KeyClass.KeyType.MainCharacters:
                            keyScript = Instantiate(templateMainCharactersButton, keyboardLine).GetComponent<KeyButtonScript>();
                            keyScript.gameObject.SetActive(false);
                            break;
                    }
                    keyScript.InitKey(CurrentKeyList()[i].keys[j].isUseDifferentSize ?
                            CurrentKeyList()[i].keys[j].keySize : CurrentKeyList()[i].keysSize, CurrentKeyList()[i].keys[j].keyValue, this,
                            themeList[currentTheme].buttonsNormalColor, themeList[currentTheme].buttonsHightlightedColor,
                            themeList[currentTheme].buttonsPressedColor, themeList[currentTheme].buttonsSelectedColor,
                            themeList[currentTheme].keysTextColor, themeList[currentTheme].glowColor, currentFont, currentFontSize);
                    keyScript.gameObject.SetActive(true);

                    CurrentKeyList()[i].keys[j].keyButtonScript = keyScript;
                    if (CurrentKeyList()[i].keys[j].keyType == KeyClass.KeyType.Character) {
                        CurrentKeyList()[i].keys[j].keyButtonScript.SetShiftState(isShifted);
                    }
                }
                lastIDKeys++;
            }
            for(int j = lastIDKeys; j < keyboardLine.childCount; j++) {
                keyboardLine.GetChild(j).gameObject.SetActive(false);
            }
        }
        for (int j = lastIDLines; j < parentLines.childCount; j++) {
            parentLines.GetChild(j).gameObject.SetActive(false);
        }
    }

    [HideInInspector]
    public bool isPressKeyboard;

    public void OnPointerDown (PointerEventData eventData) {
        isPressKeyboard = true;
    }

    public void OnPointerUp (PointerEventData eventData) {
        isPressKeyboard = false;
    }
}
