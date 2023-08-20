using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Element : MonoBehaviour {

    [Serializable]
    public struct ElementData {
        public int id;
        public Sprite sprite;
    }

    private ElementData _data;
    public ElementData Data {
        get => _data;
        set {
            _data = value;
            // setting sprite on the element when we change data
            transform.GetChild(0).GetComponent<Image>().sprite = value.sprite;
        }
    }

    // used for animation
    [HideInInspector]
    public float beforeAnimYPos;
}
