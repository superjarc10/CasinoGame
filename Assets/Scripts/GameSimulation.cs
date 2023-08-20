using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameSimulation : MonoBehaviour {

    public CylinderStopCondition stopCondition;
    public LayerMask buttonLayerMask;

    private ScreenSetup screenSetup => GetComponent<ScreenSetup>();
    private Camera mainCamera;

    void Start() {
        var randomizedElements = RandomizeElements();
        screenSetup.SetupCylinders(randomizedElements);
        mainCamera = Camera.main;
    }

    // inputs
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            screenSetup.Spin(stopCondition);
        }

        if (Input.GetMouseButtonDown(0)) {
            Vector3 touchPos = Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(touchPos);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100, buttonLayerMask)) {
                hit.collider.GetComponent<Animator>().Play("ButtonAnim", 0, 0);
                screenSetup.Spin(stopCondition);
            } 
        }
    }

    // creating random configuration for elements in each cylinder at start
    List<int>[] RandomizeElements() {
        List<int>[] allCylindersConfig = new List<int>[screenSetup.cylinderCount];
        System.Random rng = new System.Random();
        for (int i = 0; i < screenSetup.cylinderCount; i++) {
            var cylinderConfig = new List<int>();
            for (int j = 0; j < screenSetup.elementSprites.Length; j++) {
                cylinderConfig.Add(j);
            }
            cylinderConfig = cylinderConfig.OrderBy(_ => rng.Next()).ToList();
            allCylindersConfig[i] = cylinderConfig;
        }
        return allCylindersConfig;
    }
}
