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
    VisualElement statsCountryScreen;
    VisualElement stageClearScreen;
    VisualElement adPopupScreen;
    VisualElement iapPopupScreen;
    VisualElement purchaseSuccessScreen;
    VisualElement purchaseFailureScreen;
    bool isOverlayScreenTransitioning = false;
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        InitializeHandler();
        InitializeThemeList();
        StartCoroutine(InitializeCountryList());
        StartCoroutine(InitializeThemeList());
        StartCoroutine(InitializeStatsCountryList());
        Helper.SetHapticToBtn(root, "ui-btn", false, GameManager.Instance.uiBtnClickSound);
    }
    void InitializeHandler()
    {
        // Cache references to frequently used elements
        mainScreen = root.Q("MainScreen");
        themeScreen = root.Q("ThemeScreen");
        flagListScreen = root.Q("FlagListScreen");
        settingsScreen = root.Q("SettingsScreen");
        aboutScreen = root.Q("AboutScreen");
        dataSettingsScreen = root.Q("DataSettingsScreen");
        statsScreen = root.Q("StatsScreen");
        statsCountryScreen = root.Q("StatsCountryScreen");
        stageClearScreen = root.Q("StageClear");
        adPopupScreen = root.Q("WatchAdsPopup");
        iapPopupScreen = root.Q("IAPPopup");
        purchaseSuccessScreen = root.Q("PurchaseSuccess");
        purchaseFailureScreen = root.Q("PurchaseFailure");

        // Close Button Click Listeners
        flagListScreen.Q<Button>("CloseBtn").clicked += () => flagListScreen.AddToClassList("translate-down");
        themeScreen.Q<Button>("CloseBtn").clicked += () => themeScreen.AddToClassList("translate-down");
        settingsScreen.Q<Button>("CloseBtn").clicked += () => settingsScreen.AddToClassList("translate-down");
        statsCountryScreen.Q<Button>("CloseBtn").clicked += () => statsCountryScreen.AddToClassList("scale-to-zero");
        adPopupScreen.Q<Button>("CloseBtn").clicked += () => adPopupScreen.AddToClassList("scale-to-zero");
        purchaseSuccessScreen.Q<Button>("ContinueBtn").clicked += () => purchaseSuccessScreen.AddToClassList("scale-to-zero");
        purchaseFailureScreen.Q<Button>("ContinueBtn").clicked += () => purchaseFailureScreen.AddToClassList("scale-to-zero");

        // Back Button Click Listeners
        dataSettingsScreen.Q<Button>("BackBtn").clicked += () => dataSettingsScreen.AddToClassList("translate-right");
        aboutScreen.Q<Button>("BackBtn").clicked += () => aboutScreen.AddToClassList("translate-right");
        statsScreen.Q<Button>("BackBtn").clicked += () => statsScreen.AddToClassList("translate-right");

        // Continue Button Click Listener
        stageClearScreen.Q<Button>("ContinueBtn").clicked += () => stageClearScreen.AddToClassList("hidden");

        // Register callback for click event on ScrollContainer
        flagListScreen.Q("ScrollContainer").RegisterCallback<ClickEvent>(OnChooseCtry);
        themeScreen.Q("ScrollContainer").RegisterCallback<ClickEvent>(OnChooseTheme);
        statsScreen.Q<ScrollView>().RegisterCallback<ClickEvent>(OnChooseStatsCtry);

        // Footer Buttons Click Listeners
        mainScreen.Q<Button>("WorldMap").clicked += () => flagListScreen.RemoveFromClassList("translate-down");
        mainScreen.Q<Button>("Palette").clicked += () => themeScreen.RemoveFromClassList("translate-down");
        mainScreen.Q<Button>("Settings").clicked += () => settingsScreen.RemoveFromClassList("translate-down");

        // Settings Item Click Listeners
        settingsScreen.Q<Button>("About").clicked += () => aboutScreen.RemoveFromClassList("translate-right");
        settingsScreen.Q<Button>("DataSettings").clicked += () => dataSettingsScreen.RemoveFromClassList("translate-right");
        settingsScreen.Q<Button>("Stats").clicked += () => statsScreen.RemoveFromClassList("translate-right");

        // Buttons inside Popup Listeners
        adPopupScreen.Q<Button>("YESBtn").clicked += () =>
        {
            adPopupScreen.AddToClassList("scale-to-zero");
            AdManager.Instance.ShowRewardedVideo();
        };
        adPopupScreen.Q<Button>("NOBtn").clicked += () =>
        {
            adPopupScreen.AddToClassList("scale-to-zero");
            iapPopupScreen.RemoveFromClassList("scale-to-zero");
        };
        iapPopupScreen.Q<Button>("YESBtn").clicked += () =>
        {
            iapPopupScreen.AddToClassList("scale-to-zero");
            ProductInfo removeAds = IAPManager.Instance.GetProductInfoById(0);
            IAPManager.Instance.PurchaseProduct(removeAds);
        };
        iapPopupScreen.Q<Button>("NOBtn").clicked += () =>
        {
            iapPopupScreen.AddToClassList("scale-to-zero");
        };
    }
    void OnChooseTheme(ClickEvent evt)
    {
        if (evt.target is VisualElement element && element.userData is int themeIndex)
        {
            GameManager.Instance.SetTheme(themeIndex);
            VisualElement themeContainer = root.Q("ThemeScreen").Q("ScrollContainer").Q("unity-content-container");
            foreach (VisualElement themeItem in themeContainer.Children())
            {
                themeItem.Q("PaletteItem").RemoveFromClassList("palette-item-select");
            }
            themeContainer.Children().ElementAt(themeIndex).Q("PaletteItem").AddToClassList("palette-item-select");
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
            statsCountryScreen.RemoveFromClassList("scale-to-zero");
        }
    }
    void SetStatsCtryInfo(string ctryName)
    {
        CountrySO countrySO = GameManager.Instance.GetCountrySO(ctryName);
        Texture2D flagImage = Resources.Load<Texture2D>($"Images/Flags/{ctryName}");
        statsCountryScreen.Q("TitleFlag").style.backgroundImage = new StyleBackground(flagImage);
        statsCountryScreen.Q<Label>("Title").text = ctryName;
        statsCountryScreen.Q<Label>("NumOfRegions").text = $"# of regions: {countrySO.mapSO.numRegions}";
        string area = countrySO.area.ToString("N0"), population = countrySO.population.ToString("N0");
        statsCountryScreen.Q<Label>("Area").text = $"Area: {area} km^2({GameManager.Instance.GetAreaRank(ctryName)}/{GameManager.Instance.countryList.Count})";
        statsCountryScreen.Q<Label>("Population").text = $"Population: {population}({GameManager.Instance.GetPopulationRank(ctryName)}/{GameManager.Instance.countryList.Count})";
        statsCountryScreen.Q<Label>("FunFact").text = $"<line-height=150%>{countrySO.funFact}";
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
    IEnumerator InitializeStatsCountryList()
    {
        yield return null;
        ScrollView scrollview = root.Q("StatsScreen").Q<ScrollView>();
        foreach (CountrySO.Continent continent in Enum.GetValues(typeof(CountrySO.Continent)))
        {
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
                        flagcell.Q<Button>().userData = countrySO.ctryName;
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
                adPopupScreen.RemoveFromClassList("scale-to-zero");
                break;
            case "iap-popup":
                iapPopupScreen.RemoveFromClassList("scale-to-zero");
                break;
            case "iap-success-popup":
                purchaseSuccessScreen.RemoveFromClassList("scale-to-zero");
                break;
            case "iap-fail-popup":
                purchaseFailureScreen.RemoveFromClassList("scale-to-zero");
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
        VisualElement[] screens = { themeScreen, flagListScreen, settingsScreen, aboutScreen, dataSettingsScreen, statsScreen, statsCountryScreen };

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
        stageClearScreen.RemoveFromClassList("hidden");
        stageClearScreen.Q("TitleContainer").Q<Label>("Title").text = $"Congratulations!\nYou just colored all {ctrySO.mapSO.numRegions} regions!";
        Texture2D flagImage = Resources.Load<Texture2D>($"Images/Flags/{ctrySO.ctryName}");
        stageClearScreen.Q("Flag").style.backgroundImage = new StyleBackground(flagImage);
        stageClearScreen.Q<Label>("FunFact").text = ctrySO.funFact;
    }
}
