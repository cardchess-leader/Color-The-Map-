using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UITKController : MonoBehaviour
{
    public static UITKController instance;
    VisualElement root;
    VisualElement mainScreen;
    VisualElement themeScreen;
    VisualElement flagListScreen;
    VisualElement settingsScreen;
    VisualElement aboutScreen;
    VisualElement dataSettingsScreen;
    VisualElement statsScreen;
    VisualElement statsCountryScreen;
    void Awake()
    {
        instance = this;
    }
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        InitializeHandler();
        InitializeThemeList();
        StartCoroutine(InitializeCountryList());
        StartCoroutine(InitializeThemeList());
        StartCoroutine(InitializeStatsCountryList());
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

        // Close Button Click Listeners
        flagListScreen.Q<Button>("CloseBtn").clicked += () => flagListScreen.AddToClassList("translate-down");
        themeScreen.Q<Button>("CloseBtn").clicked += () => themeScreen.AddToClassList("translate-down");
        settingsScreen.Q<Button>("CloseBtn").clicked += () => settingsScreen.AddToClassList("translate-down");
        statsCountryScreen.Q<Button>("CloseBtn").clicked += () => statsCountryScreen.AddToClassList("scale-to-zero");

        // Back Button Click Listeners
        dataSettingsScreen.Q<Button>("BackBtn").clicked += () => dataSettingsScreen.AddToClassList("translate-right");
        aboutScreen.Q<Button>("BackBtn").clicked += () => aboutScreen.AddToClassList("translate-right");
        statsScreen.Q<Button>("BackBtn").clicked += () => statsScreen.AddToClassList("translate-right");

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
    }
    void OnChooseTheme(ClickEvent evt)
    {
        if (evt.target is VisualElement element && element.userData is int themeIndex)
        {
            GameManager.instance.SetTheme(themeIndex);
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
            GameManager.instance.SetCountryMap(ctryName);
            root.Q("FlagListScreen").AddToClassList("translate-down");
        }
    }
    void OnChooseStatsCtry(ClickEvent evt)
    {
        if (evt.target is VisualElement element && element.userData is string ctryName)
        {
            // GameManager.instance.SetCountryMap(ctryName);
            Debug.Log("ctryName is: " + ctryName);
            root.Q("StatsCountryScreen").RemoveFromClassList("scale-to-zero");
        }
    }
    IEnumerator InitializeThemeList()
    {
        yield return null;
        ScrollView scrollview = root.Q("ThemeScreen").Q<ScrollView>();
        bool isOdd = true;
        for (int i = 0; i < GameManager.instance.themeList.Count; i++)
        {
            ThemeSO themeSO = GameManager.instance.themeList[i];
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
    IEnumerator InitializeCountryList()
    {
        yield return null;
        ScrollView scrollview = root.Q("FlagListScreen").Q<ScrollView>();
        foreach (CountrySO.Continent continent in Enum.GetValues(typeof(CountrySO.Continent)))
        {
            // Adding the delimiter //
            VisualElement delimiter = Resources.Load<VisualTreeAsset>($"UXML/Delimiter").CloneTree();
            delimiter.Q<Label>().text = continent.ToString().Replace("_", " ");
            scrollview.Add(delimiter);
            List<CountrySO> countryList = GameManager.instance.countryList.Where(countrySO => countrySO.continent == continent).ToList();
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
                        if (countrySO.terms == CountrySO.Terms.WatchAds)
                        {
                            marker.AddToClassList("flag-marker-camera");
                        }
                        else if (countrySO.terms == CountrySO.Terms.Locked)
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
            List<CountrySO> countryList = GameManager.instance.countryList.Where(countrySO => countrySO.continent == continent).ToList();
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
        // StartCoroutine(ShowUISegmentCoroutine(segmentName));
        switch (segmentName)
        {
            case "footer":
                mainScreen.Q("Footer").RemoveFromClassList("translate-down");
                break;
            case "select-map":
                flagListScreen.RemoveFromClassList("translate-down");
                break;
        }
    }
    // IEnumerator ShowUISegmentCoroutine(string segmentName)
    // {
    //     yield return null;
    //     switch (segmentName)
    //     {
    //         case "footer":
    //             mainScreen.Q("Footer").RemoveFromClassList("translate-down");
    //             break;
    //         case "select-map":
    //             flagListScreen.RemoveFromClassList("translate-down");
    //             break;
    //     }
    // }
}
