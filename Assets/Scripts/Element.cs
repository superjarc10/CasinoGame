using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Element : MonoBehaviour {

    [Serializable]
    public struct ElementInfo {
        public int id;
        public Sprite sprite;
    }

    private ElementInfo _elementInfo;
    public ElementInfo elementInfo {
        get => _elementInfo;
        set {
            _elementInfo = value;
            // setting sprite on the element when we change data
            transform.GetChild(0).GetComponent<Image>().sprite = value.sprite;
        }
    }

    // used for animation
    [HideInInspector]
    public float beforeAnimYPos;
}
