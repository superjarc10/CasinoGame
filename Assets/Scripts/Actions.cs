using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Helper things for animation

interface IActionInfo {
	 
}

public struct ActionInfo<T>: IActionInfo {

	public T startValue;
	public T endValue;
	public float duration;
	public float currentTime;
	public float percent;
	bool unscaledTime;
	float timeScale;
	bool didStart;
	public Action onAnimationEnd;

	public bool isAnimating {
		get {
            return currentTime < duration;
		}
	}

	public ActionInfo(T startValue, T endValue, float duration) {
		this.startValue = startValue;
		this.endValue = endValue;
		this.duration = duration;
		currentTime = 0;
		percent = 0;
		unscaledTime = false;
		timeScale = 1;
		didStart = true;
		onAnimationEnd = null;
	}
	
	public void UpdateTimeCurve(AnimationCurve curve) {
		if (currentTime > duration) return;
		currentTime += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime * timeScale;
		float timePercent = Mathf.Clamp01(currentTime / duration);
		percent = curve.Evaluate(timePercent);

		if (didStart && timePercent >= 1) {
			didStart = false;
			if (onAnimationEnd != null) onAnimationEnd();
		}
	}

	public void AddOnAnimationEnd(Action action) {
		onAnimationEnd = action;
	}

	public T AnimationValue {
		get {
			if (typeof(T) == typeof(float))
				return (T)(object)Mathf.LerpUnclamped((float)(object)startValue, (float)(object)endValue, percent);
			if (typeof(T) == typeof(Vector2))
				return (T)(object)Vector2.LerpUnclamped((Vector2)(object)startValue, (Vector2)(object)endValue, percent);
			if (typeof(T) == typeof(Vector3))
				return (T)(object)Vector3.LerpUnclamped((Vector3)(object)startValue, (Vector3)(object)endValue, percent);
			else return (T)(object)null;
		}
	}
}

public class Actions : MonoBehaviour {

	public static void WaitAndDo(MonoBehaviour obj, float wait, Action doSomething) {
		obj.StartCoroutine(WaitAndDoCor(wait, doSomething));
	}

	public static IEnumerator WaitAndDoCor(float wait, Action doSomething) {
		yield return new WaitForSeconds(wait);
		doSomething();
	}
}
