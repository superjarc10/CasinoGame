using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenSetup : MonoBehaviour {

    public int cylinderCount = 5;
    public int rowsVisible = 3;

    public float spinningSpeed = 5;
    public float spinningDuration = 1;

    public Element.ElementData[] elementSprites;
    public RectTransform cylinder;

    private Cylinder[] cylinders;
    private int spinningCylinderCount;
    private List<int>[] latestSpin;

    [SerializeField]
    private TextMeshProUGUI winText;

    void Start() {
        latestSpin = new List<int>[cylinderCount];
    }

    // method to start the spin
    public void Spin(CylinderStopCondition stopCondition) {
        // making sure that we can spin before all cylinders stop
        if (spinningCylinderCount > 0) return;
        spinningCylinderCount = cylinderCount;
        for (int i = 0; i < cylinders.Length; i++) {
            var cylinder = cylinders[i];
            float delay = i * 0.1f;
            cylinder.Spin(stopCondition, delay, spinningSpeed, 1);
        }
    }

    void CheckResult() {
        // checking first the middle row, then all the others
        var didWin = false;
        if (CheckMiddleRow()) {
            didWin = true;
        } else if (CheckAllRows()) {
            didWin = true;
        } 

        // showing win text
        if (didWin) {
            winText.gameObject.SetActive(true);
            // hack to disable player input while we show win text
            spinningCylinderCount = 1;
            Actions.WaitAndDo(this, 2, () => {
                spinningCylinderCount = 0;
                winText.gameObject.SetActive(false);
            });
        }
    }

    // checking midle row
    bool CheckMiddleRow() {
        var middleRow = new int[latestSpin.Length];
        for (int i = 0; i < middleRow.Length; i++) {
            middleRow[i] = latestSpin[i][1];
        }
        var middleSame = true;
        var middleElementsSame = new bool[cylinderCount];
        middleElementsSame[0] = true;
        for (int i = 1; i < middleRow.Length; i++) {
            // comparing current cylinder with previous one
            if (middleRow[i] != middleRow[i - 1]) {
                middleSame = false;
                break;
            } else middleElementsSame[i] = true;
        }

        // animating elements
        if (middleSame) {
            for (int i = 0; i < cylinders.Length; i++) {
                bool[] animateElements = new bool[] { false, middleElementsSame[i], false };
                cylinders[i].AnimateElements(animateElements, 2);
            }
        }
        return middleSame;
    }

    // helper method
    string ImageNameForId(int id) {
        for (int i = 0; i < elementSprites.Length; i++) {
            if (id == elementSprites[i].id) return elementSprites[i].sprite.name;
        }
        return "";
    }

    // checking all rows
    bool CheckAllRows() {
        var elementsSame = new int[] { 0, 0, 0 };
        var elementCylindersSame = new bool[] { true, true, true };

        // checking how many times each element from 1st row repeats
        for (int i = 1; i < cylinderCount; i++) {
            var firstColumn = latestSpin[0];
            for (int j = 0; j < firstColumn.Count; j++) {
                bool isSame = false;
                for (int k = 0; k < latestSpin[i].Count; k++) {
                    if (firstColumn[j] == latestSpin[i][k]) {
                        isSame = true;
                        continue;
                    }
                }
                if (isSame && elementCylindersSame[j]) {
                    elementsSame[j] += 1;
                } else elementCylindersSame[j] = false;
            }
        }

        // checking if there is any element that repeats 3 times and setting animation for each cylinder
        bool didWin = false;
        for (int i = 0; i < cylinderCount; i++) {
            var animatedElements = new bool[rowsVisible];
            for (int j = 0; j < elementsSame.Length; j++) {
                if (elementsSame[j] < 2) continue;
                didWin = true;
                for (int k = 0; k < latestSpin[i].Count; k++) {
                    if (i > elementsSame[j]) break;
                    if (latestSpin[0][j] == latestSpin[i][k]) animatedElements[k] = true;
                }
            }
            cylinders[i].AnimateElements(animatedElements, 2);
        }
        return didWin;
    }

    // start setup of each cylinder
    public void SetupCylinders(List<int>[] cylinderConfig) {
        var canvasRect = GetComponent<RectTransform>();
        float screenChunk = canvasRect.sizeDelta.x / cylinderCount;
        float cylinderWidth = screenChunk;
        float offset = -canvasRect.sizeDelta.x / 2 + screenChunk / 2;
        cylinders = new Cylinder[cylinderCount];
        for (int i = 0; i < cylinderCount; i++) {
            // instantiating each cylinder and setting cylinder position and size
            RectTransform newCylinder;
            if (i == 0) {
                newCylinder = cylinder;
            } else {
                newCylinder = Instantiate(cylinder);
                newCylinder.SetParent(cylinder.parent);
            }
            newCylinder.sizeDelta = new Vector2(cylinderWidth, cylinder.sizeDelta.y);
            newCylinder.anchoredPosition = new Vector2(offset + i * screenChunk, cylinder.anchoredPosition.y);
            //

            // setting cylinder stop callback
            var cylinderObj = newCylinder.GetComponent<Cylinder>();
            var index = i;
            cylinderObj.onCylinderStoped = ((List<int> currentIDs, int cylinderId) => {
                // decrease cylinder spinning count(to enable input)
                spinningCylinderCount--;
                // caching latest result
                latestSpin[cylinderId] = currentIDs;
                // making sure that the cylinders stop from left to right, meaning that second can stop after first did.
                if (index < cylinderCount - 1) {
                    cylinders[index + 1].CanStop();
                }
                // if all stop, check result
                if (spinningCylinderCount == 0) CheckResult();
            });

            // setting random element configuration(generated in GameSimulation)
            var config = cylinderConfig[i];
            var elementConfigs = new List<Element.ElementData>();
            for (int j = 0; j < config.Count; j++) {
                for (int k = 0; k < elementSprites.Length; k++) {
                    if (config[j] == elementSprites[k].id) {
                        var elementInfo = new Element.ElementData();
                        elementInfo.id = elementSprites[k].id;
                        elementInfo.sprite = elementSprites[k].sprite;
                        elementConfigs.Add(elementInfo);
                        continue;
                    }
                }
            }
            cylinderObj.SetupElements(elementConfigs, i);
            cylinders[i] = cylinderObj;
        }
    }
}
