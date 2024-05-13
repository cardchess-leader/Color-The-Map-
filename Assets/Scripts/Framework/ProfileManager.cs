using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hyperbyte;
using Hyperbyte.Ads;
using Hyperbyte.HapticFeedback;
public class ProfileManager : Singleton<ProfileManager>
{
    [SerializeField] AppSettings appSettings;

    // Sound status change event action.
    public static event Action<bool> OnSoundStatusChangedEvent;

    // Music status change event action.
    public static event Action<bool> OnMusicStatusChangedEvent;

    // Vibration/Haptic feedback status change event action.
    public static event Action<bool> OnVibrationStatusChangedEvent;

    // Notification status change event action.
    public static event Action<bool> OnNotificationStatusChangedEvent;

    // Returns current status of sound.
    private bool isSoundEnabled = true;
    public bool IsSoundEnabled
    {
        get
        {
            return isSoundEnabled;
        }
    }

    // Returns current status of music.
    private bool isMusicEnabled = true;
    public bool IsMusicEnabled
    {
        get
        {
            return isMusicEnabled;
        }
    }

    // Returns current status of Vibrations/Haptic Feedback.
    private bool isVibrationEnabled = true;
    public bool IsVibrationEnabled
    {
        get
        {
            return isVibrationEnabled;
        }
    }

    // Returns current status of Notifications.
    private bool isNotificationEnabled = true;
    public bool IsNotificationEnabled
    {
        get
        {
            return isNotificationEnabled;
        }
    }

    // Whether user will be served ads or not.
    [System.NonSerialized] bool isUserAdFree = false;

    // Profile manager has initialized or not.
    bool hasInitialised = false;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        Initialise();
    }

    /// <summary>
    /// Initializes the profilemanager.
    /// </summary>
    void Initialise()
    {
        if (appSettings == null)
        {
            appSettings = (AppSettings)Resources.Load("AppSettings");
        }
        hasInitialised = true;
        initProfileStatus();
    }

    /// <summary>
    /// This function is called when the behaviour becomes enabled or active.
    /// </summary>
    private void OnEnable()
    {
        /// Initiate haptic feedback generator.
        HapticFeedbackGenerator.InitHapticFeedbackGenerator();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        /// Releases haptic feedback generator.
        HapticFeedbackGenerator.ReleaseHapticFeedbackGenerator();
    }

    /// <summary>
    /// Inits the audio status.
    /// </summary>
    public void initProfileStatus()
    {
        // Fetches the status of all user setting and invokes event callbacks for each settings.

        isMusicEnabled = (PlayerPrefs.GetInt("isMusicEnabled", 0) == 0) ? true : false;
        isVibrationEnabled = (PlayerPrefs.GetInt("isVibrationEnabled", 0) == 0) ? true : false;
        isNotificationEnabled = (PlayerPrefs.GetInt("isNotificationEnabled", 0) == 0) ? true : false;
        isSoundEnabled = (PlayerPrefs.GetInt("isSoundEnabled", 0) == 0) ? true : false;

        if ((!isSoundEnabled) && (OnSoundStatusChangedEvent != null))
        {
            OnSoundStatusChangedEvent.Invoke(isSoundEnabled);
        }
        if ((!isMusicEnabled) && (OnMusicStatusChangedEvent != null))
        {
            OnMusicStatusChangedEvent.Invoke(isMusicEnabled);
        }
        if ((!isVibrationEnabled) && (OnVibrationStatusChangedEvent != null))
        {
            OnVibrationStatusChangedEvent.Invoke(isVibrationEnabled);
        }
        if (!appSettings.enableVibrations) { isVibrationEnabled = false; }

        if ((!isNotificationEnabled) && (OnNotificationStatusChangedEvent != null))
        {
            OnNotificationStatusChangedEvent.Invoke(isNotificationEnabled);
        }

        isUserAdFree = (PlayerPrefs.GetInt("isUserAdFree", 0) == 1) ? true : false;
    }

    /// <summary>
    /// Toggles the sound status.
    /// </summary>
    public void ToggleSoundStatus()
    {
        isSoundEnabled = (isSoundEnabled) ? false : true;
        PlayerPrefs.SetInt("isSoundEnabled", (isSoundEnabled) ? 0 : 1);

        if (OnSoundStatusChangedEvent != null)
        {
            OnSoundStatusChangedEvent.Invoke(isSoundEnabled);
        }
    }

    /// <summary>
    /// Toggles the music status.
    /// </summary>
    public void ToggleMusicStatus()
    {
        isMusicEnabled = (isMusicEnabled) ? false : true;
        PlayerPrefs.SetInt("isMusicEnabled", (isMusicEnabled) ? 0 : 1);

        if (OnMusicStatusChangedEvent != null)
        {
            OnMusicStatusChangedEvent.Invoke(isMusicEnabled);
        }
    }

    /// <summary>
    /// Toggles the vibration status.
    /// </summary>
    public void ToggleVibrationStatus()
    {
        isVibrationEnabled = (isVibrationEnabled) ? false : true;
        PlayerPrefs.SetInt("isVibrationEnabled", (isVibrationEnabled) ? 0 : 1);

        if (OnVibrationStatusChangedEvent != null)
        {
            OnVibrationStatusChangedEvent.Invoke(isVibrationEnabled);
        }
    }

    // Returns the app setting scriptable instance.
    public AppSettings GetAppSettings()
    {
        if (!hasInitialised)
        {
            Initialise();
        }
        return appSettings;
    }

    /// <summary>
    /// Sets app as ad free. Will be called when user purchase inapp to remove ads.
    /// </summary>
    public void SetAppAsAdFree()
    {
        Debug.Log("Ad Free!");
        PlayerPrefs.SetInt("isUserAdFree", 1);
        isUserAdFree = true;
        AdManager.Instance.HideBanner();
    }

    public bool IsAppAdFree()
    {
        return isUserAdFree;
    }
}
