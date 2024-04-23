//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRubyShared;
using TMPro;

public class GestureController : MonoBehaviour
{
    /// <summary>
    /// The targetMap
    /// </summary>
    public GameObject targetMap;
    // public TMP_Text magnification;
    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    private PanGestureRecognizer panGesture;
    private ScaleGestureRecognizer scaleGesture;
    private RotateGestureRecognizer rotateGesture;
    private void DebugText(string text, params object[] format)
    {
        //bottomLabel.text = string.Format(text, format);
        Debug.Log(string.Format(text, format));
    }

    private void TapGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            DebugText("Tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);
            GameManager.instance.HandleMapTouch(gesture.FocusX, gesture.FocusY);
        }
    }

    private void CreateTapGesture()
    {
        tapGesture = new TapGestureRecognizer();
        tapGesture.StateUpdated += TapGestureCallback;
        tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
        FingersScript.Instance.AddGesture(tapGesture);
    }

    private void PanGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            DebugText("Panned, Location: {0}, {1}, Delta: {2}, {3}", gesture.FocusX, gesture.FocusY, gesture.DeltaX, gesture.DeltaY);

            float deltaX = panGesture.DeltaX / 200.0f;
            float deltaY = panGesture.DeltaY / 200.0f;
            Vector3 pos = targetMap.transform.position;
            pos.x += deltaX;
            pos.y += deltaY;
            targetMap.transform.position = pos;
        }
    }

    private void CreatePanGesture()
    {
        panGesture = new PanGestureRecognizer();
        panGesture.MinimumNumberOfTouchesToTrack = 1;
        panGesture.StateUpdated += PanGestureCallback;
        FingersScript.Instance.AddGesture(panGesture);
    }

    private void ScaleGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            DebugText("Scaled: {0}, Focus: {1}, {2}", scaleGesture.ScaleMultiplier, scaleGesture.FocusX, scaleGesture.FocusY);
            targetMap.transform.localScale = Vector3.one * Mathf.Clamp(targetMap.transform.localScale.x * scaleGesture.ScaleMultiplier, 0.5f, 10f);
            // magnification.text = $"{targetMap.transform.localScale * 100}%";
        }
    }

    private void CreateScaleGesture()
    {
        scaleGesture = new ScaleGestureRecognizer();
        scaleGesture.StateUpdated += ScaleGestureCallback;
        FingersScript.Instance.AddGesture(scaleGesture);
    }

    private void RotateGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Executing)
        {
            targetMap.transform.Rotate(0.0f, 0.0f, rotateGesture.RotationRadiansDelta * Mathf.Rad2Deg);
        }
    }

    private void CreateRotateGesture()
    {
        rotateGesture = new RotateGestureRecognizer();
        rotateGesture.StateUpdated += RotateGestureCallback;
        FingersScript.Instance.AddGesture(rotateGesture);
    }

    private void Start()
    {
        // don't reorder the creation of these :)
        CreateTapGesture();
        CreatePanGesture();
        CreateScaleGesture();
        CreateRotateGesture();

        // pan, scale and rotate can all happen simultaneously
        panGesture.AllowSimultaneousExecution(scaleGesture);
        panGesture.AllowSimultaneousExecution(rotateGesture);
        scaleGesture.AllowSimultaneousExecution(rotateGesture);

        // prevent the one special no-pass button from passing through,
        //  even though the parent scroll view allows pass through (see FingerScript.PassThroughObjects)
        FingersScript.Instance.CaptureGestureHandler = (obj) => null;
    }
}