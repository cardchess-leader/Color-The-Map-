using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Hyperbyte;
using Hyperbyte.Ads;

public class GameManager : Singleton<GameManager>
{
    public List<CountrySO> countryList = new List<CountrySO>();
    public List<ThemeSO> themeList = new List<ThemeSO>();
    public GestureController gestureController;
    public GameObject mapContainer;
    public GameObject[] paintBrushSet = new GameObject[4];
    public Color? selectedColor;
    public RenderTexture renderTexture;
    public AudioClip uiBtnClickSound;
    public List<AudioClip> sfxClipList = new List<AudioClip>(); // 0: select paint, 1: color region, 2: invalid color, 3: clear pop
    public List<int> showRatingIndexList = new List<int>();
    [NonSerialized] public float initialOrthographicSize;
    [NonSerialized] public float minZoom = 0.5f; // Minimum zoom limit
    [NonSerialized] public CountrySO countrySO;
    Camera mainCamera;
    Color[] regionsColor;
    bool stageCleared = false;
    CountrySO rewardCountrySO;
    void OnEnable()
    {
        InitializePlayerPref();
        InitializeGame();
        AdManager.OnRewardedAdRewardedEvent += OnRewardedAdRewarded;
    }
    void OnDisable()
    {
        AdManager.OnRewardedAdRewardedEvent -= OnRewardedAdRewarded;
    }
    void Start()
    {
        mainCamera = Camera.main;
        initialOrthographicSize = mainCamera.orthographicSize;
        // TestAdjMatrix(); // TEMP: Comment in production! //
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ProcessBackButton();
        }
    }
    void TestAdjMatrix() // This is temp method //
    {
        foreach (CountrySO ctrySO in countryList)
        {
            MapSO mapSO = ctrySO.mapSO;
            for (int i = 1; i < mapSO.adjMatrix.Count; i++)
            {
                foreach (string indexStr in mapSO.adjMatrix[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        int index = int.Parse(indexStr);
                        if (!mapSO.adjMatrix[index].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().Contains(i.ToString()))
                        {
                            Debug.Log($"{ctrySO.ctryName} has error at index {i}, {indexStr}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in {ctrySO.ctryName} at index {i}, {indexStr}: {e.Message}");
                    }
                }
            }
        }
    }
    void ProcessBackButton()
    {
        UITKController.Instance.ShowUISegment("quit-game-popup");
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
                //On Android, on quitting app, App actully won't quit but will be sent to background. So it can be load fast while reopening. 
				AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
				activity.Call<bool>("moveTaskToBack" , true); 
#elif UNITY_IOS
				Application.Quit();
#endif
    }
    void InitializePlayerPref()
    {
        if (PlayerPrefs.GetInt("Initialize") != 1)
        {
            PlayerPrefs.SetInt("ThemeIndex", 0);
            PlayerPrefs.SetInt("Initialize", 1);
            PlayerPrefs.SetInt("VersionCode", 0);
            PlayerPrefs.SetString("CtryMapProgress", string.Empty);
            PlayerPrefs.SetInt("ClearCount", 0);
            PlayerPrefs.Save(); // Ensure preferences are saved
        }
    }
    void OnRewardedAdRewarded()
    {
        if (rewardCountrySO != null)
        {
            SetupCountry(rewardCountrySO);
        }
    }
    public void InitializeGame()
    {
        SetTheme(0, true);
        UITKController.Instance.ShowUISegment("select-map");
    }
    public void SetTheme(int themeIndex = 0, bool initialize = false)
    {
        if (!initialize)
        {
            if (themeIndex == PlayerPrefs.GetInt("ThemeIndex"))
            {
                return;
            }
            else
            {
                PlayerPrefs.SetInt("ThemeIndex", themeIndex);
                ResetColor();
                if (mapContainer.transform.childCount > 0)
                {
                    // Reset the map with default colors (grey)
                    foreach (Transform child in mapContainer.transform.GetChild(0).GetChild(0))
                    {
                        if (child.gameObject.name.StartsWith("Layer"))
                        {
                            child.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1);
                        }
                    }
                    regionsColor = new Color[regionsColor.Length];
                }
            }
        }
        ThemeSO themeSO = themeList[PlayerPrefs.GetInt("ThemeIndex")];
        for (int i = 0; i < paintBrushSet.Length; i++)
        {
            paintBrushSet[i].GetComponent<Image>().color = themeSO.themeColors[i];
        }
        SetParticleSystemGradient(themeSO);
    }
    public void SelectColor(int index)
    {
        selectedColor = paintBrushSet[index].GetComponent<Image>().color;
        foreach (GameObject brush in paintBrushSet)
        {
            brush.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
        paintBrushSet[index].transform.GetChild(0).GetComponent<Image>().enabled = true;
        AudioController.Instance.PlayClip(sfxClipList[0]);
        UIFeedback.Instance.PlayHapticLight();
    }
    public void ResetColor()
    {
        selectedColor = null;
        foreach (GameObject brush in paintBrushSet)
        {
            brush.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
    }
    public void HandleMapTouch(float coordX, float coordY)
    {
        if (regionsColor == null || regionsColor.Length == 0)
        {
            Debug.LogError("regionsColor is not initialized");
            return;
        }
        Vector2 touchPos = mainCamera.ScreenToWorldPoint(new Vector2(coordX, coordY));
        RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);
        if (hit.collider != null && selectedColor.HasValue)
        {
            // Debug.Log("Hit");
            if (int.TryParse(hit.collider.gameObject.name.Substring(6), out int targetIndex) && IsValidColoring(targetIndex))
            {
                SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = selectedColor.Value;
                    regionsColor[targetIndex] = selectedColor.Value;
                    AudioController.Instance.PlayClip(sfxClipList[1]);
                    UIFeedback.Instance.PlayHapticLight();
                    CheckForColoringComplete();
                }
            }
            else
            {
                AudioController.Instance.PlayClip(sfxClipList[2]);
            }
        }
    }
    bool IsValidColoring(int targetIndex)
    {
        var indices = countrySO.mapSO.adjMatrix[targetIndex]
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(index => int.TryParse(index.Trim(), out int parsedIndex) ? parsedIndex : -1)
            .Where(index => index != -1)
            .ToList();

        return !indices.Any(adjIndex => adjIndex >= regionsColor.Length || regionsColor[adjIndex] == selectedColor.Value);
    }
    void CheckForColoringComplete()
    {
        bool complete = !regionsColor.Skip(1).Any(color => color == new Color());

        if (complete && !stageCleared)
        {
            stageCleared = true;
            UITKController.Instance.HandleStageClear(countrySO);
            GameObject.Find("Confetti").GetComponent<ParticleSystem>().Play();

            int clearIndex = countryList.IndexOf(countrySO);
            if (clearIndex != -1)
            {
                var progressList = Helper.ConvertStringToList(PlayerPrefs.GetString("CtryMapProgress"));
                if (!progressList.Contains(clearIndex.ToString()))
                {
                    progressList.Add(clearIndex.ToString());
                    PlayerPrefs.SetString("CtryMapProgress", string.Join(",", progressList));
                }
            }

            AudioController.Instance.PlayClip(sfxClipList[3]);
            StartCoroutine(UITKController.Instance.InitializeCountryList());
            StartCoroutine(UITKController.Instance.InitializeStatsCountryList());

            if (PhotoController.Instance != null)
            {
                PhotoController.Instance.gameObject.SetActive(true);
            }

            LeaderboardController.Instance.UpdateScore();
            PlayerPrefs.SetInt("ClearCount", PlayerPrefs.GetInt("ClearCount") + 1);
        }
    }

    void SetupCountry(CountrySO countrySO)
    {
        SetCountryMap(countrySO);
        UITKController.Instance.HideUISegment("flag-list");
        UITKController.Instance.SetCtryUI(countrySO);
    }
    public void OnChooseCtry(string ctryName)
    {
        CountrySO countrySO = GetCountrySO(ctryName);
        if (countrySO == null)
        {
            return;
        }
        if (ProfileManager.Instance.IsAppAdFree())
        {
            SetupCountry(countrySO);
        }
        else
        {
            // SetupCountry(countrySO); // TEMP: Force Enable All Countries //
            switch (countrySO.terms)
            {
                case CountrySO.Terms.Free:
                    SetupCountry(countrySO);
                    break;
                case CountrySO.Terms.WatchAds:
                    rewardCountrySO = countrySO;
                    UITKController.Instance.ShowUISegment("ad-popup", ctryName);
                    break;
                case CountrySO.Terms.Locked:
                    UITKController.Instance.ShowUISegment("iap-popup");
                    break;
            }
        }
    }
    public void SetCountryMap(CountrySO countrySO)
    {
        if (countrySO == null)
        {
            return;
        }

        this.countrySO = countrySO;

        if (mapContainer.transform.childCount > 0)
        {
            Destroy(mapContainer.transform.GetChild(0).gameObject);
        }

        InitializeMainCamera();
        GameObject countryMap = Instantiate(countrySO.countryMapPrefab, Vector3.zero, Quaternion.identity, mapContainer.transform);
        regionsColor = new Color[countrySO.mapSO.adjMatrix.Count];
        stageCleared = false;

        if (PhotoController.Instance != null)
        {
            PhotoController.Instance.gameObject.SetActive(false);
        }
    }

    void InitializeMainCamera()
    {
        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        mainCamera.orthographicSize = 5;
    }
    public CountrySO GetCountrySO(string ctryName)
    {
        return countryList.FirstOrDefault(ctrySO => ctrySO.ctryName == ctryName);
    }
    public int GetPopulationRank(string ctryName)
    {
        var sortedCountryList = countryList.OrderByDescending(ctry => ctry.population).ToList();
        return sortedCountryList.IndexOf(GetCountrySO(ctryName)) + 1;
    }
    public int GetAreaRank(string ctryName)
    {
        var sortedCountryList = countryList.OrderByDescending(ctry => ctry.area).ToList();
        return sortedCountryList.IndexOf(GetCountrySO(ctryName)) + 1;
    }
    void SetParticleSystemGradient(ThemeSO themeSO)
    {
        var confetti = GameObject.Find("Confetti");
        if (confetti == null)
        {
            Debug.LogError("Confetti GameObject not found");
            return;
        }
        var mainModule = confetti.GetComponent<ParticleSystem>().main;

        // Create a new gradient
        Gradient gradient = new Gradient();

        // Set up color keys
        gradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(themeSO.themeColors[0], 0.25f),
            new GradientColorKey(themeSO.themeColors[1], 0.5f),
            new GradientColorKey(themeSO.themeColors[2], 0.75f),
            new GradientColorKey(themeSO.themeColors[3], 1f)
        };

        // Set up alpha keys
        gradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
        };

        gradient.mode = GradientMode.Fixed;

        mainModule.startColor = new ParticleSystem.MinMaxGradient(gradient)
        {
            mode = ParticleSystemGradientMode.RandomColor
        };
    }
    public bool IsMapCleared(CountrySO countrySO)
    {
        try
        {
            int countryIndex = countryList.IndexOf(countrySO);
            return Helper.ConvertStringToList(PlayerPrefs.GetString("CtryMapProgress")).Contains(countryIndex.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("exception is: " + e);
            return false;
        }
    }
    public List<CountrySO> ClearedCtryList()
    {
        try
        {
            var indicesString = PlayerPrefs.GetString("CtryMapProgress");
            var indexList = Helper.ConvertStringToList(indicesString);

            return indexList.Select(strIndex =>
            {
                if (int.TryParse(strIndex, out int index) && index < countryList.Count)
                {
                    return countryList[index];
                }
                else
                {
                    Debug.LogWarning($"Failed to parse '{strIndex}' to int");
                    return null;
                }
            }).Where(country => country != null).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred: {e.Message}");
            return new List<CountrySO>();
        }
    }

    public void RateApp()
    {
#if UNITY_IOS
        Application.OpenURL("itms-apps://itunes.apple.com/app/id" + ProfileManager.Instance.GetAppSettings().appleID);
#elif UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
#endif
        PlayerPrefs.SetInt("AppRated", 1);
    }

    public void TryShowingRatingPopup()
    {
        if (PlayerPrefs.GetInt("AppRated") == 0)
        {
            int clearCount = PlayerPrefs.GetInt("ClearCount");
            if (showRatingIndexList.Contains(clearCount))
            {
                UITKController.Instance.ShowUISegment("rate-us-popup");
            }
        }
    }
}