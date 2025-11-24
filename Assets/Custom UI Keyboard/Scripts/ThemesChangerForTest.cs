using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Just test script for change themes by pressing Buttons
/// </summary>
public class ThemesChangerForTest : MonoBehaviour
{
    public KeyboardManager keyboardManager;

    public void SelectTheme(int target) {
        keyboardManager.currentTheme = target;
        keyboardManager.InitKeyboard();
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).GetChild(4).gameObject.SetActive(false);
        }
        transform.GetChild(target).GetChild(4).gameObject.SetActive(true);
    }
}
