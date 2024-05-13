using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Hyperbyte;
using Hyperbyte.Ads;

public class UITKController : Singleton<UITKController>
{
    VisualElement root;
    VisualElement mainScreen;
    VisualElement themeScreen;
    VisualElement flagListScreen;
    VisualElement settingsScreen;
    VisualElement aboutScreen;
    VisualElement dataSettingsScreen;
    VisualElement statsScreen;
    VisualElement statsCountryPopup;
    VisualElement stageClearOverlay;
    VisualElement adPopup;
    VisualElement iapPopup;
    VisualElement purchaseSuccessPopup;
    VisualElement purchaseFailurePopup;
    VisualElement howToScreen;
    bool isOverlayScreenTransitioning = false;
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        SetScreensReference();
        InitializeHandler();
        InitializeThemeList();
        StartCoroutine(InitAfterFirstFrame());
        StartCoroutine(InitializeCountryList());
        StartCoroutine(InitializeThemeList());
        StartCoroutine(InitializeStatsCountryList());
        Helper.SetHapticToBtn(root, "ui-btn", false, GameManager.Instance.uiBtnClickSound);
    }
    void SetScreensReference()
    {
        // Cache references to frequently used elements
        mainScreen = root.Q("MainScreen");
        themeScreen = root.Q("ThemeScreen");
        flagListScreen = root.Q("FlagListScreen");
        howToScreen = root.Q("HowToScreen");
        settingsScreen = root.Q("SettingsScreen");
        dataSettingsScreen = root.Q("DataSettingsScreen");
        aboutScreen = root.Q("AboutScreen");
        statsScreen = root.Q("StatsScreen");
        stageClearOverlay = root.Q("StageClearOverlay");
        statsCountryPopup = root.Q("StatsCountryPopup");
        adPopup = root.Q("WatchAdsPopup");
        iapPopup = root.Q("IAPPopup");
        purchaseSuccessPopup = root.Q("PurchaseSuccessPopup");
        purchaseFailurePopup = root.Q("PurchaseFailurePopup");
    }
    void InitializeMainPageHandler()
    {
        // Open Subpage through Footer
        mainScreen.Q<Button>("WorldMap").clicked += () => flagListScreen.RemoveFromClassList("translate-down");
        mainScreen.Q<Button>("Palette").clicked += () => themeScreen.RemoveFromClassList("translate-down");
        mainScreen.Q<Button>("Help").clicked += () => howToScreen.RemoveFromClassList("translate-down");
        mainScreen.Q<Button>("Settings").clicked += () => settingsScreen.RemoveFromClassList("translate-down");
        mainScreen.Q<Button>("IAP").clicked += () => iapPopup.RemoveFromClassList("scale-to-zero");
        // Close Subpage
        flagListScreen.Q<Button>("CloseBtn").clicked += () => flagListScreen.AddToClassList("translate-down");
        themeScreen.Q<Button>("CloseBtn").clicked += () => themeScreen.AddToClassList("translate-down");
        howToScreen.Q<Button>("ContinueBtn").clicked += () => howToScreen.AddToClassList("translate-down");
        stageClearOverlay.Q<Button>("ContinueBtn").clicked += () => stageClearOverlay.AddToClassList("hidden");
        // Register callback for click event on ScrollContainer
        flagListScreen.Q("ScrollContainer").RegisterCallback<ClickEvent>(OnChooseCtry);
        themeScreen.Q("ScrollContainer").RegisterCallback<ClickEvent>(OnChooseTheme);
    }
    void InitializeAdIapPageHandler()
    {
        // Yes/No Btn Handler
        adPopup.Q<Button>("YESBtn").clicked += () =>
        {
            adPopup.AddToClassList("scale-to-zero");
            AdManager.Instance.ShowRewardedVideo();
        };
        adPopup.Q<Button>("NOBtn").clicked += () =>
        {
            adPopup.AddToClassList("scale-to-zero");
            iapPopup.RemoveFromClassList("scale-to-zero");
        };
        iapPopup.Q<Button>("YESBtn").clicked += () =>
        {
            iapPopup.AddToClassList("scale-to-zero");
            ProductInfo removeAds = IAPManager.Instance.GetProductInfoById(0);
            IAPManager.Instance.PurchaseProduct(removeAds);
        };
        iapPopup.Q<Button>("NOBtn").clicked += () =>
        {
            iapPopup.AddToClassList("scale-to-zero");
        };
        // Close Ad & IAP Pages
        adPopup.Q<Button>("CloseBtn").clicked += () => adPopup.AddToClassList("scale-to-zero");
        purchaseSuccessPopup.Q<Button>("ContinueBtn").clicked += () => purchaseSuccessPopup.AddToClassList("scale-to-zero");
        purchaseFailurePopup.Q<Button>("ContinueBtn").clicked += () => purchaseFailurePopup.AddToClassList("scale-to-zero");
    }
    void InitializeSettingsPageHandler()
    {
        // Settings Item Click Listeners
        settingsScreen.Q<Button>("About").clicked += () => aboutScreen.RemoveFromClassList("translate-right");
        settingsScreen.Q<Button>("Sound").clicked += () =>
        {
            ProfileManager.Instance.ToggleSoundStatus();
            UpdateSoundIcon();
        };
        settingsScreen.Q<Button>("Vibration").clicked += () =>
        {
            ProfileManager.Instance.ToggleVibrationStatus();
            UpdateVibrationIcon();
        };
        settingsScreen.Q<Button>("Music").clicked += () =>
        {
            ProfileManager.Instance.ToggleMusicStatus();
            UpdateMusicIcon();
        };
        settingsScreen.Q<Button>("DataSettings").clicked += () => dataSettingsScreen.RemoveFromClassList("translate-right");
        settingsScreen.Q<Button>("Stats").clicked += () => statsScreen.RemoveFromClassList("translate-right");
        // Close Settings & Settings subpages
        settingsScreen.Q<Button>("CloseBtn").clicked += () => settingsScreen.AddToClassList("translate-down");
        dataSettingsScreen.Q<Button>("BackBtn").clicked += () => dataSettingsScreen.AddToClassList("translate-right");
        aboutScreen.Q<Button>("BackBtn").clicked += () => aboutScreen.AddToClassList("translate-right");
        statsScreen.Q<Button>("BackBtn").clicked += () => statsScreen.AddToClassList("translate-right");
        statsCountryPopup.Q<Button>("CloseBtn").clicked += () => statsCountryPopup.AddToClassList("scale-to-zero");
        // Open Stats > Country Info Popup
        statsScreen.Q<ScrollView>().RegisterCallback<ClickEvent>(OnChooseStatsCtry);
    }
    void InitializeHandler()
    {
        SetScreensReference();
        InitializeMainPageHandler();
        InitializeAdIapPageHandler();
        InitializeSettingsPageHandler();
    }
    IEnumerator InitAfterFirstFrame()
    {
        yield return null;
        UpdateSoundIcon();
        UpdateVibrationIcon();
        UpdateMusicIcon();
        UpdateIAPButton();
    }
    void UpdateSoundIcon()
    {
        string iconPath = ProfileManager.Instance.IsSoundEnabled ? "Icons/volume" : "Icons/volume-slash";
        Texture2D soundImage = Resources.Load<Texture2D>(iconPath);
        if (soundImage == null)
        {
            Debug.LogError($"Failed to load texture from path: {iconPath}");
            return;
        }
        settingsScreen.Q<Button>("Sound").Q("Icon").style.backgroundImage = soundImage;
    }
    void UpdateVibrationIcon()
    {
        string iconPath = ProfileManager.Instance.IsVibrationEnabled ? "Icons/vibration-on" : "Icons/vibration-off";
        Texture2D vibrationImage = Resources.Load<Texture2D>(iconPath);
        if (vibrationImage == null)
        {
            Debug.LogError($"Failed to load texture from path: {iconPath}");
            return;
        }
        settingsScreen.Q<Button>("Vibration").Q("Icon").style.backgroundImage = vibrationImage;
    }
    void UpdateMusicIcon()
    {
        string iconPath = ProfileManager.Instance.IsMusicEnabled ? "Icons/music" : "Icons/music-slash";
        Texture2D musicImage = Resources.Load<Texture2D>(iconPath);
        if (musicImage == null)
        {
            Debug.LogError($"Failed to load texture from path: {iconPath}");
            return;
        }
        settingsScreen.Q<Button>("Music").Q("Icon").style.backgroundImage = musicImage;
    }
    public void UpdateIAPButton()
    {
        if (ProfileManager.Instance.IsAppAdFree())
        {
            // Attempt to find the IAP button
            VisualElement iapButton = mainScreen.Q<Button>("IAP");

            // Check if the element is correctly cast as a Button
            if (iapButton is Button button)
            {
                button.SetEnabled(false);  // Safely disable the button

                // Find the checkmark element within the button and make it visible
                var checkMark = iapButton.Q("Check");
                if (checkMark != null)
                {
                    checkMark.style.display = DisplayStyle.Flex;
                }
                else
                {
                    Debug.LogError("Checkmark element not found within IAP button.");
                }
            }
            else
            {
                Debug.LogError("IAP button not found or is not a button.");
            }
        }
    }
    void OnChooseTheme(ClickEvent evt)
    {
        if (evt.target is VisualElement element && element.userData is int themeIndex)
        {
            if (themeIndex == 0 || ProfileManager.Instance.IsAppAdFree())
            {
                GameManager.Instance.SetTheme(themeIndex);
                VisualElement themeContainer = root.Q("ThemeScreen").Q("ScrollContainer").Q("unity-content-container");
                foreach (VisualElement themeItem in themeContainer.Children())
                {
                    themeItem.Q("PaletteItem").RemoveFromClassList("palette-item-select");
                }
                themeContainer.Children().ElementAt(themeIndex).Q("PaletteItem").AddToClassList("palette-item-select");
            }
            else
            {
                iapPopup.RemoveFromClassList("scale-to-zero"); // Show IAP Page
            }
        }
    }
    void OnChooseCtry(ClickEvent evt)
    {
        if (evt.target is VisualElement element && element.userData is string ctryName)
        {
            GameManager.Instance.OnChooseCtry(ctryName);
        }
    }
    public void SetCtryUI(CountrySO countrySO)
    {
        mainScreen.Q<Label>("Title").text = countrySO.ctryName;
        Texture2D flagImage = Resources.Load<Texture2D>($"Images/Flags/{countrySO.ctryName}");
        mainScreen.Q("TitleFlag").style.backgroundImage = new StyleBackground(flagImage);
    }
    IEnumerator OverlayScreenTransitionCoroutine()
    {
        isOverlayScreenTransitioning = true;
        yield return new WaitForSeconds(0.5f);
        isOverlayScreenTransitioning = false;
    }
    void OnChooseStatsCtry(ClickEvent evt)
    {
        if (evt.target is VisualElement element && element.userData is string ctryName)
        {
            SetStatsCtryInfo(ctryName);
            statsCountryPopup.RemoveFromClassList("scale-to-zero");
        }
    }
    void SetStatsCtryInfo(string ctryName)
    {
        CountrySO countrySO = GameManager.Instance.GetCountrySO(ctryName);
        Texture2D flagImage = Resources.Load<Texture2D>($"Images/Flags/{ctryName}");
        statsCountryPopup.Q("TitleFlag").style.backgroundImage = new StyleBackground(flagImage);
        statsCountryPopup.Q<Label>("Title").text = ctryName;
        statsCountryPopup.Q<Label>("NumOfRegions").text = $"# of regions: {countrySO.mapSO.numRegions}";
        string area = countrySO.area.ToString("N0"), population = countrySO.population.ToString("N0");
        statsCountryPopup.Q<Label>("Area").text = $"Area: {area} km^2({GameManager.Instance.GetAreaRank(ctryName)}/{GameManager.Instance.countryList.Count})";
        statsCountryPopup.Q<Label>("Population").text = $"Population: {population}({GameManager.Instance.GetPopulationRank(ctryName)}/{GameManager.Instance.countryList.Count})";
        statsCountryPopup.Q<Label>("FunFact").text = $"<line-height=150%>{countrySO.funFact}";
    }
    IEnumerator InitializeThemeList()
    {
        yield return null;
        ScrollView scrollview = root.Q("ThemeScreen").Q<ScrollView>();
        bool isOdd = true;
        for (int i = 0; i < GameManager.Instance.themeList.Count; i++)
        {
            ThemeSO themeSO = GameManager.Instance.themeList[i];
            VisualElement paletteItem = Resources.Load<VisualTreeAsset>($"UXML/PaletteItem").CloneTree();
            paletteItem.AddToClassList(isOdd ? "palette-item-odd" : "palette-item-even");
            paletteItem.Q<Label>().text = themeSO.themeName;
            paletteItem.Q("Color1").style.unityBackgroundImageTintColor = themeSO.themeColors[0];
            paletteItem.Q("Color2").style.unityBackgroundImageTintColor = themeSO.themeColors[1];
            paletteItem.Q("Color3").style.unityBackgroundImageTintColor = themeSO.themeColors[2];
            paletteItem.Q("Color4").style.unityBackgroundImageTintColor = themeSO.themeColors[3];
            paletteItem.Q("PaletteItem").style.backgroundColor = Helper.GetAverageColor(themeSO.themeColors, 0.5f);
            paletteItem.Q<Button>().userData = i;
            if (PlayerPrefs.GetInt("ThemeIndex") == i)
            {
                paletteItem.Q<Button>().AddToClassList("palette-item-select");
            }
            scrollview.Add(paletteItem);
            isOdd = !isOdd;
        }
    }
    public IEnumerator InitializeCountryList()
    {
        yield return null;
        ScrollView scrollview = root.Q("FlagListScreen").Q<ScrollView>();
        scrollview.Clear();
        foreach (CountrySO.Continent continent in Enum.GetValues(typeof(CountrySO.Continent)))
        {
            // Adding the delimiter //
            VisualElement delimiter = Resources.Load<VisualTreeAsset>($"UXML/Delimiter").CloneTree();
            delimiter.Q<Label>().text = continent.ToString().Replace("_", " ");
            scrollview.Add(delimiter);
            List<CountrySO> countryList = GameManager.Instance.countryList.Where(countrySO => countrySO.continent == continent).ToList();
            for (int i = 0; i < Mathf.Ceil(countryList.Count / 5f); i++)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("flag-row");
                for (int j = 0; j < 5; j++)
                {
                    int index = 5 * i + j;
                    VisualElement flagcell = Resources.Load<VisualTreeAsset>($"UXML/FlagCell").CloneTree();
                    if (index < countryList.Count)
                    {
                        CountrySO countrySO = countryList[index];
                        Texture2D image = Resources.Load<Texture2D>($"Images/Flags/{countrySO.ctryName}");
                        flagcell.Q<Button>().style.backgroundImage = new StyleBackground(image);
                        VisualElement marker = flagcell.Q("Marker");
                        if (GameManager.Instance.IsMapCleared(countrySO))
                        {
                            marker.AddToClassList("flag-marker-clear");
                        }
                        else if (!ProfileManager.Instance.IsAppAdFree() && countrySO.terms == CountrySO.Terms.WatchAds)
                        {
                            marker.AddToClassList("flag-marker-camera");
                        }
                        else if (!ProfileManager.Instance.IsAppAdFree() && countrySO.terms == CountrySO.Terms.Locked)
                        {
                            marker.AddToClassList("flag-marker-lock");
                        }
                        flagcell.Q<Button>().userData = countrySO.ctryName;
                        flagcell.Q<Button>().AddToClassList("ui-btn");
                    }
                    else // Insert empty flag items as placeholder //
                    {
                        flagcell.style.visibility = Visibility.Hidden;
                        flagcell.Q<Button>().pickingMode = PickingMode.Ignore;
                    }
                    row.Add(flagcell);
                }
                scrollview.Add(row);
            }
        }
    }
    public IEnumerator InitializeStatsCountryList() // also update how many countries have been cleared!
    {
        yield return null;
        ScrollView scrollview = root.Q("StatsScreen").Q<ScrollView>();
        scrollview.Clear();
        List<CountrySO> countryList = GameManager.Instance.ClearedCtryList();
        statsScreen.Q<Label>("Label2").text = $"You have colored {countryList.Count} country maps!";
        for (int i = 0; i < Mathf.Ceil(countryList.Count / 5f); i++)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("flag-row");
            for (int j = 0; j < 5; j++)
            {
                int index = 5 * i + j;
                VisualElement flagcell = Resources.Load<VisualTreeAsset>($"UXML/FlagCell").CloneTree();
                if (index < countryList.Count)
                {
                    CountrySO countrySO = countryList[index];
                    Texture2D image = Resources.Load<Texture2D>($"Images/Flags/{countrySO.ctryName}");
                    flagcell.Q<Button>().style.backgroundImage = new StyleBackground(image);
                    flagcell.Q<Button>().userData = countrySO.ctryName;
                    flagcell.Q<Button>().AddToClassList("ui-btn");
                }
                else // Insert empty flag items as placeholder //
                {
                    flagcell.style.visibility = Visibility.Hidden;
                    flagcell.Q<Button>().pickingMode = PickingMode.Ignore;
                }
                row.Add(flagcell);
            }
            scrollview.Add(row);
        }
    }
    public void ShowUISegment(string segmentName)
    {
        switch (segmentName)
        {
            case "footer":
                mainScreen.Q("Footer").RemoveFromClassList("translate-down");
                break;
            case "select-map":
                flagListScreen.RemoveFromClassList("translate-down");
                break;
            case "ad-popup":
                adPopup.RemoveFromClassList("scale-to-zero");
                break;
            case "iap-popup":
                iapPopup.RemoveFromClassList("scale-to-zero");
                break;
            case "iap-success-popup":
                purchaseSuccessPopup.RemoveFromClassList("scale-to-zero");
                break;
            case "iap-fail-popup":
                purchaseFailurePopup.RemoveFromClassList("scale-to-zero");
                break;
        }
    }
    public void HideUISegment(string segmentName)
    {
        switch (segmentName)
        {
            case "flag-list":
                flagListScreen.AddToClassList("translate-down");
                StartCoroutine(OverlayScreenTransitionCoroutine());
                break;
            case "ad-popup":
                StartCoroutine(OverlayScreenTransitionCoroutine());
                break;
        }
    }
    public bool IsScreenOverlay()
    {
        if (isOverlayScreenTransitioning)
        {
            return true;
        }
        // Create an array of all screens to check
        VisualElement[] screens = { themeScreen, flagListScreen, settingsScreen, aboutScreen, dataSettingsScreen, statsScreen, statsCountryPopup };

        // Loop through each screen and check the classes
        foreach (VisualElement screen in screens)
        {
            if (!(screen.ClassListContains("translate-down") || screen.ClassListContains("scale-to-zero") || screen.ClassListContains("translate-right")))
            {
                return true;
            }
        }
        return false;
    }
    public void HandleStageClear(CountrySO ctrySO)
    {
        stageClearOverlay.RemoveFromClassList("hidden");
        stageClearOverlay.Q("TitleContainer").Q<Label>("Title").text = $"Congratulations!\nYou just colored all {ctrySO.mapSO.numRegions} regions!";
        Texture2D flagImage = Resources.Load<Texture2D>($"Images/Flags/{ctrySO.ctryName}");
        stageClearOverlay.Q("Flag").style.backgroundImage = new StyleBackground(flagImage);
        stageClearOverlay.Q<Label>("FunFact").text = ctrySO.funFact;
    }
}
