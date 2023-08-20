using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cylinder : MonoBehaviour {

    public AnimationCurve startAnimCurve;
    public AnimationCurve endAnimCurve;

    public System.Action<List<int>, int> onCylinderStoped;

    // cylinder has 4 states
    enum State {
        Start, Spinning, End, Idle
    }

    private State currentState = State.Idle;
    private CylinderStopCondition stopCondition;
    private float speed;
    private float startEndAnimMagnitude;
    private int cylinderID;
    private ScreenSetup screenSetup;
    private ActionInfo<float> startAnim;
    private ActionInfo<float> endAnim;
    private List<Element> elements = new List<Element>();
    private List<Element.ElementData> config;
    private int currentElementIndex;
    private float spinningDuration;
    private bool didPreviousStop = false;

    // updating movement/logic for each state
    void Update() {
        switch (currentState) {
            case State.Start:
                UpdateStart();
                break;
            case State.Spinning:
                UpdateSpinning();
                break;
            case State.End:
                UpdateEnd();
                break;
            default: break;
        }
    }

    // spin method for every cylinder, called from ScreenSetup
    public void Spin(CylinderStopCondition stopCondition, float delay, float speed, float startEndAnimMagnitude) {
        if (currentState != State.Idle) return;
        this.stopCondition = stopCondition;
        this.speed = speed;
        this.startEndAnimMagnitude = startEndAnimMagnitude;
        spinningDuration = stopCondition.duration + Random.Range(0, stopCondition.randomDuration);
        // the first cylinder doesnt have to wait for any other to stop
        didPreviousStop = cylinderID == 0;

        // call start animation with an increasingly larger delay(set in ScreenSetup)
        Actions.WaitAndDo(this, delay, (() => {
            currentState = State.Start;
            // setting and caching position for animation
            SetElementStartPos(0);
            startAnim = new ActionInfo<float>(0, startEndAnimMagnitude, 0.5f);
            // changing state after the start animation is finished
            startAnim.AddOnAnimationEnd(() => {
                currentState = State.Spinning;
            });
        }));
    }

    // updating start animation
    void UpdateStart() {
        startAnim.UpdateTimeCurve(startAnimCurve);
        if (startAnim.isAnimating) {
            foreach (var element in elements) {
                float y = element.beforeAnimYPos + startAnim.AnimationValue;
                var rect = element.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            }
        }
    }

    // setting and caching position for animation
    void SetElementStartPos(float offset) {
        foreach (var element in elements) {
            var rect = element.GetComponent<RectTransform>();
            rect.anchoredPosition += Vector2.up * -offset;
            element.beforeAnimYPos = rect.anchoredPosition.y;
        }
    }

    // updating during spinning state
    void UpdateSpinning() {
        var canvasRect = screenSetup.GetComponent<RectTransform>();
        float screenChunk = canvasRect.sizeDelta.y / screenSetup.rowsVisible;
        float offset = canvasRect.sizeDelta.y / 2 + screenChunk / 2;

        for (int i = 0; i < elements.Count; i++) {
            // decreasing elements y position 
            var element = elements[i].GetComponent<RectTransform>();
            element.anchoredPosition += Vector2.down * speed * Time.deltaTime;

            // if element is past some value out of the screen in the bottom, we put it back at the top
            float posDiff = element.anchoredPosition.y + offset;
            if (posDiff <= 0.2f) {
                PutLastElementTop();
            }
        }

        // checking for condition to stop the cylinder
        var conditionTrue = false;
        spinningDuration -= Time.deltaTime;
        if (spinningDuration <= 0 && didPreviousStop) {
            if (stopCondition.type == CylinderStopCondition.ConditionType.Duration) {
                conditionTrue = true;
            } else if (stopCondition.type == CylinderStopCondition.ConditionType.MiddleElement) {
                var middleElement = elements[2];
                if (middleElement.Data.id == stopCondition.middleElementId) conditionTrue = true;
            } else {
                int index = Random.Range(1,4);
                var randomElement = elements[index];
                if (randomElement.Data.id == stopCondition.middleElementId) conditionTrue = true;
            }
        }

        // if condition true and previous cylinder did stop
        if (conditionTrue) {
            // transition to the end animation state
            currentState = State.End;
            var middleElement = elements[2];
            SetElementStartPos(middleElement.GetComponent<RectTransform>().anchoredPosition.y);
            endAnim = new ActionInfo<float>(0, -startEndAnimMagnitude, 0.5f);
            endAnim.AddOnAnimationEnd((System.Action)(() => {
                // when end animation ends, transition to idle state
                currentState = State.Idle;
                var currentElementIDs = new List<int>();
                currentElementIDs.Add((int)elements[(int)1].Data.id);
                currentElementIDs.Add((int)elements[(int)2].Data.id);
                currentElementIDs.Add((int)elements[(int)3].Data.id);
                // calling cylinder stoped spinning with cylinder result and id/index
                onCylinderStoped.Invoke(currentElementIDs, cylinderID);
            }));
        }
    }
    
    void PutLastElementTop() {
        // calculating position for the element we put at the top
        var canvasRect = screenSetup.GetComponent<RectTransform>();
        float screenChunk = canvasRect.sizeDelta.y / screenSetup.rowsVisible;
        var lastElement = elements[elements.Count - 1].GetComponent<RectTransform>();
        var firstElement = elements[0].GetComponent<RectTransform>();
        float y = firstElement.anchoredPosition.y + screenChunk;
        lastElement.anchoredPosition = new Vector2(lastElement.anchoredPosition.x, y);

        // also we need to set configuration for the element we put at the top. we really only have 4 elements 
        // in each cylinder but there is 8 different images. we must switch element info(image)
        var lastElementObj = lastElement.GetComponent<Element>();
        lastElementObj.Data = config[currentElementIndex];
        currentElementIndex = currentElementIndex + 1 >= config.Count ? 0 : currentElementIndex + 1;

        // removing last element from the last place in the list and putting it at the first palce
        elements.RemoveAt(elements.Count - 1);
        elements.Insert(0, lastElement.GetComponent<Element>());
    }

    // end animation update
    void UpdateEnd() {
        endAnim.UpdateTimeCurve(endAnimCurve);
        if (endAnim.isAnimating) {
            foreach (var element in elements) {
                float y = element.beforeAnimYPos + endAnim.AnimationValue;
                var rect = element.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            }
        }
    }

    // calling this will allow cylinder to stop
    public void CanStop() {
        didPreviousStop = true;
    }

    public void SetupElements(List<Element.ElementData> config, int cylinderID) {
        this.config = config;
        this.cylinderID = cylinderID;
        currentElementIndex = 0;
        screenSetup = GetComponentInParent<ScreenSetup>();
        var canvasRect = screenSetup.GetComponent<RectTransform>();
        float screenChunk = canvasRect.sizeDelta.y / screenSetup.rowsVisible;
        var elementSize = screenChunk - 0.3f;
        float offset = canvasRect.sizeDelta.y / 2 - screenChunk / 2;
        for (int i = 0; i < transform.childCount; i++) {
            // setting size, position
            RectTransform element = transform.GetChild(i).GetComponent<RectTransform>();
            element.sizeDelta = new Vector2(element.sizeDelta.x, elementSize);
            element.anchoredPosition = new Vector2(0, offset - i * screenChunk);
            var elementObj = element.GetComponent<Element>();
            // setting element config
            elementObj.Data = config[i];
            elements.Add(elementObj);
            // tracking which element info/image is next since we are switching element placegholder images.
            currentElementIndex += 1;
        }
    }

    // scaling animation for elements that are the winning ones
    public void AnimateElements(bool[] animateElements, float duration) {
        for (int i = 0; i < animateElements.Length; i++) {
            if (!animateElements[i]) continue;
            var elementAnim = elements[i + 1].GetComponent<Animator>();
            elementAnim.Play("ElementAnim", 0, 0);
            Actions.WaitAndDo(this, duration, () => {
                elementAnim.CrossFade("Empty", 0.1f);
            });
        }
    }
}
