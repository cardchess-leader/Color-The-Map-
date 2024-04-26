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
    // public GameObject targetMap;
    // public TMP_Text magnification;
    private Camera targetCamera;  // Assign your main camera here in the inspector
    private TapGestureRecognizer tapGesture;
    private TapGestureRecognizer doubleTapGesture;
    private PanGestureRecognizer panGesture;
    private ScaleGestureRecognizer scaleGesture;
    private RotateGestureRecognizer rotateGesture;
    public float minZoom = 0.5f; // Minimum zoom limit
    public float maxZoom = 10f;  // Maximum zoom limit
    public float zoomSensitivity = 1; // Sensitivity of the zoom
    private Vector2 previousPinchPosition; // Zoom Gesture Related
    private Vector3 previousPinchWorldPosition;
    private Vector2 previousRotatePosition; // Rotation Gesture Related
    private Vector3 rotationCenter; // Rotation Gesture Related
    private Vector2 lastPanPosition;
    private bool isPanning = false;
    private void DebugText(string text, params object[] format)
    {
        //bottomLabel.text = string.Format(text, format);
        Debug.Log(string.Format(text, format));
    }

    private void TapGestureCallback(DigitalRubyShared.GestureRecognizer gesture)
    {
        if (gesture.State == GestureRecognizerState.Ended)
        {
            DebugText("Test - Tapped at {0}, {1}", gesture.FocusX, gesture.FocusY);
            if (Tutorial.instance != null && Tutorial.instance.enabled)
            {
                Tutorial.instance.HandleMapTouch(gesture.FocusX, gesture.FocusY);
            }
            else
            {
                GameManager.instance.HandleMapTouch(gesture.FocusX, gesture.FocusY);
            }
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
        if (gesture.State == GestureRecognizerState.Began)
        {
            lastPanPosition = new Vector2(gesture.FocusX, gesture.FocusY);
            isPanning = true;
        }
        else if (gesture.State == GestureRecognizerState.Executing && isPanning)
        {
            Vector2 currentPanPosition = new Vector2(gesture.FocusX, gesture.FocusY);
            Vector2 delta = currentPanPosition - lastPanPosition;

            // Convert the delta from screen coordinates to world coordinates
            Vector3 worldDelta = targetCamera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, targetCamera.nearClipPlane))
                              - targetCamera.ScreenToWorldPoint(new Vector3(0, 0, targetCamera.nearClipPlane));

            // Apply the delta to the camera's position
            targetCamera.transform.position -= new Vector3(worldDelta.x, worldDelta.y, 0);

            // Update the last pan position
            lastPanPosition = currentPanPosition;
        }
        else if (gesture.State == GestureRecognizerState.Ended)
        {
            isPanning = false;
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
        if (gesture.State == GestureRecognizerState.Began)
        {
            previousPinchPosition = new Vector2(gesture.FocusX, gesture.FocusY); // Store the initial pinch position in screen coordinates
        }
        else if (gesture.State == GestureRecognizerState.Executing)
        {
            // Calculate the factor of zoom based on the scale multiplier
            float scaleFactor = 1 + (scaleGesture.ScaleMultiplier - 1) * zoomSensitivity;
            float newOrthographicSize = Mathf.Clamp(targetCamera.orthographicSize / scaleFactor, minZoom, maxZoom);

            // Calculate the new pinch center in screen coordinates
            Vector2 pinchCenter = new Vector2(gesture.FocusX, gesture.FocusY);
            Vector3 pinchWorldBeforeZoom = targetCamera.ScreenToWorldPoint(new Vector3(pinchCenter.x, pinchCenter.y, targetCamera.nearClipPlane));

            // Apply the new orthographic size to the camera
            targetCamera.orthographicSize = newOrthographicSize;

            // Recalculate the pinch position in the world after applying the new orthographic size
            Vector3 pinchWorldAfterZoom = targetCamera.ScreenToWorldPoint(new Vector3(pinchCenter.x, pinchCenter.y, targetCamera.nearClipPlane));

            // Adjust the camera position to compensate for the shift in pinch position
            Vector3 adjustment = pinchWorldBeforeZoom - pinchWorldAfterZoom;
            targetCamera.transform.position += adjustment;

            // Update previous pinch position
            previousPinchPosition = pinchCenter;

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
        if (gesture.State == GestureRecognizerState.Began)
        {
            previousRotatePosition = new Vector2(gesture.FocusX, gesture.FocusY); // Store the initial rotate position in screen coordinates
                                                                                  // Convert screen position of the gesture focus to a world point considering the camera's near clip plane
            rotationCenter = targetCamera.ScreenToWorldPoint(new Vector3(previousRotatePosition.x, previousRotatePosition.y, targetCamera.nearClipPlane));
            rotationCenter.z = 0; // Ensure the rotation center is in the correct plane
        }
        else if (gesture.State == GestureRecognizerState.Executing)
        {
            // Calculate rotation amount based on the change in rotation from the gesture
            float rotationAmount = -(rotateGesture.RotationRadiansDelta * Mathf.Rad2Deg);

            // Rotate the camera around the z-axis at the rotation center
            targetCamera.transform.RotateAround(rotationCenter, Vector3.forward, rotationAmount);

            // Optionally: Keep the camera's z rotation aligned to avoid unwanted tilt
            targetCamera.transform.eulerAngles = new Vector3(0, 0, targetCamera.transform.eulerAngles.z);

            // Update the previous rotation position for consistency, though it's not used further here
            previousRotatePosition = new Vector2(gesture.FocusX, gesture.FocusY);
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
        targetCamera = Camera.main;
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