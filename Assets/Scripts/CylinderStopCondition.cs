using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// template for condition for how we want to stop cylinder spins
[System.Serializable]
public struct CylinderStopCondition {
    public enum ConditionType {
        Duration, MiddleElement, AllRowsPermutation
    }

    // 2 options for stopping cylinders, 
    // Duration will spin for certain time + randomized from 0 -> randomDuration time
    // MiddleElement will stop cylinders when there is an image with middleElementId in the middle row
    // AllRowsPermutation will stop when a middleElementId element stops at random row visible
    public ConditionType type;
    public float duration;
    public float randomDuration;
    public int middleElementId;
}
