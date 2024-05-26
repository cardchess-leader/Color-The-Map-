using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using TMPro;
using Hyperbyte;

public class PhotoController : Singleton<PhotoController>
{
    public Camera cameraToCapture;
    private const float PermissionRequestTimeout = 10f; // Timeout duration in seconds

    void Start()
    {
        gameObject.SetActive(false);
    }

    void RequestPermissions()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            NativeGallery.Permission writePermission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);

            if (writePermission == NativeGallery.Permission.ShouldAsk)
            {
                NativeGallery.RequestPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);
            }
            else if (writePermission == NativeGallery.Permission.Denied)
            {
                ShowPermissionSettingsPrompt();
            }
        }
    }

    void ShowPermissionSettingsPrompt()
    {
        // Show a message to the user directing them to the app settings
        Debug.LogError("Permission has been denied permanently. Please enable it in the app settings.");
        // Optionally, you can show a dialog to the user here
        ShowSaveMessage("Please enable file write permission\nin the app settings.");
    }

    public void Capture()
    {
        if (!ProfileManager.Instance.IsAppAdFree())
        {
            UITKController.Instance.ShowUISegment("iap-popup");
            return;
        }
        if (cameraToCapture == null)
        {
            Debug.LogError("Camera to capture is not assigned.");
            return;
        }

        RequestPermissions();
        StartCoroutine(CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine()
    {
        float startTime = Time.time;

        // Wait for permissions to be granted
        while (!HasNecessaryPermissions())
        {
            if (Time.time - startTime > PermissionRequestTimeout)
            {
                Debug.LogError("Permission request timed out.");
                yield break; // Exit the coroutine if permissions are not granted within the timeout period
            }

            yield return null;
        }

        try
        {
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            cameraToCapture.targetTexture = rt;
            cameraToCapture.Render();

            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();

#if UNITY_EDITOR
            byte[] bytes = texture.EncodeToPNG();
            string imageName = GameManager.Instance.countrySO == null ? "capturedImage.png" : GameManager.Instance.countrySO.ctryName;
            string path = Path.Combine(Application.persistentDataPath, imageName);
            File.WriteAllBytes(path, bytes);
            Debug.Log("Saved Image to " + path);
#elif UNITY_ANDROID
            SaveImageToGallery(texture);
#elif UNITY_IOS
            // Implement iOS-specific image saving if needed
#endif
            Destroy(texture);
            Destroy(rt);

            ShowSaveMessage("Image successfully saved\nto your gallery!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to capture and save image: " + ex.Message);
        }
        finally
        {
            RenderTexture.active = null;
            cameraToCapture.targetTexture = null;
            cameraToCapture.enabled = false;
        }
    }

    private bool HasNecessaryPermissions()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            NativeGallery.Permission writePermission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);

            return writePermission == NativeGallery.Permission.Granted;
        }

        return true; // Assuming permissions are granted on other platforms
    }

    private void SaveImageToGallery(Texture2D texture)
    {
        string albumName = "ColorTheMap";
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(texture, albumName, "capturedImage.png", (success, path) =>
        {
            if (success)
            {
                Debug.Log("Image saved to gallery: " + path);
            }
            else
            {
                Debug.LogError("Failed to save image to gallery.");
            }
        });
    }

    private void ShowSaveMessage(string msgStr)
    {
        GameObject message = GameObject.Find("Camera Save Message");
        if (message != null)
        {
            message.GetComponent<TMP_Text>().text = msgStr;
            Animator animator = message.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Text Fade", 0, 0);
            }
            else
            {
                Debug.LogWarning("Animator component not found on Camera Save Message.");
            }
        }
        else
        {
            Debug.LogWarning("Camera Save Message GameObject not found.");
        }
    }
}
