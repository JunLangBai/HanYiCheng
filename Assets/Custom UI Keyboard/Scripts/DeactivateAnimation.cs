using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateAnimation : MonoBehaviour {
    public void Deactivate() {
        gameObject.SetActive(false);
    }
}
