using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public Camera cameraToCapture;
    public RenderTexture renderTexture;
    public float initialOrthographicSize;
    public AudioClip uiBtnClickSound;
    public List<AudioClip> sfxClipList = new List<AudioClip>(); // 0: select paint, 1: color region, 2: invalid color, 3: clear pop
    [System.NonSerialized] public float minZoom = 0.5f; // Minimum zoom limit
    [System.NonSerialized] public CountrySO countrySO;
    Camera mainCamera;
    Color[] regionsColor; // Subject Map's colors for each region
    bool stageCleared = false;
    CountrySO rewardCountrySO;
    void OnEnable()
    {
        InitializePlayerPref();
        Initialize();
        AdManager.OnRewardedAdRewardedEvent += OnRewardedAdRewarded;
    }
    void Start()
    {
        mainCamera = Camera.main;
        initialOrthographicSize = mainCamera.orthographicSize;
    }
    void InitializePlayerPref()
    {
        if (PlayerPrefs.GetInt("Initialize") != 1)
        {
            PlayerPrefs.SetInt("ThemeIndex", 0);
            PlayerPrefs.SetInt("Initialize", 1);
            PlayerPrefs.SetInt("VersionCode", 0);
            PlayerPrefs.SetString("CtryMapProgress", "");
        }
    }
    void Initialize()
    {
        InitializeGame();
    }
    void OnRewardedAdRewarded()
    {
        if (rewardCountrySO != null)
        {
            SetupCountry(rewardCountrySO);
            UITKController.Instance.HideUISegment("flag-list");
        }
    }
    public void InitializeGame()
    {
        SetTheme(0, true);
        UITKController.Instance.ShowUISegment("select-map");
    }
    public void SetTheme(int themeIndex = 0, bool initialize = false) // -1 means just use playerpref value, no update //
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
        for (int i = 0; i < 4; i++)
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
    public void HandleMapTouch(float coordX, float coordY)
    {
        Vector2 touchPos = mainCamera.ScreenToWorldPoint(new Vector2(coordX, coordY));
        RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);
        if (hit.collider != null && selectedColor.HasValue)
        {
            string colliderName = hit.collider.gameObject.name;
            if (colliderName.Length <= 6 || !int.TryParse(colliderName.Substring(6), out int targetIndex))
            {
                return;
            }

            Debug.Log($"[Log] adjMatrix length is: {countrySO.mapSO.adjMatrix.Count}");
            Debug.Log($"[Log] target index is: {targetIndex}");

            var indices = countrySO.mapSO.adjMatrix[targetIndex].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(index => int.TryParse(index.Trim(), out int parsedIndex) ? parsedIndex : -1)
                                .Where(index => index != -1).ToList();

            if (indices.Any(adjIndex => adjIndex >= regionsColor.Length || regionsColor[adjIndex] == selectedColor.Value))
            {
                AudioController.Instance.PlayClip(sfxClipList[2]); // Error sound for trying to use same color with adjacent region!
                return;
            }

            SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.color = selectedColor.Value;
            regionsColor[targetIndex] = selectedColor.Value;
            AudioController.Instance.PlayClip(sfxClipList[1]);
            UIFeedback.Instance.PlayHapticLight();
            CheckForColoringComplete();
        }
    }
    void CheckForColoringComplete()
    {
        try
        {
            bool complete = true;
            for (int i = 1; i < regionsColor.Length; i++)
            {
                if (regionsColor[i] == new Color())
                {
                    complete = false;
                    break;
                }
            }
            if (complete && !stageCleared)
            {
                stageCleared = true;
                UITKController.Instance.HandleStageClear(countrySO);
                GameObject.Find("Confetti").GetComponent<ParticleSystem>().Play();
                int clearIndex = countryList.IndexOf(countrySO); // index 0 is the first country
                if (clearIndex != -1)
                {
                    List<string> progressList = Helper.ConvertStringToList(PlayerPrefs.GetString("CtryMapProgress"));
                    if (!progressList.Contains(clearIndex.ToString()))
                    {
                        progressList.Add(clearIndex.ToString());
                        PlayerPrefs.SetString("CtryMapProgress", String.Join(",", progressList));
                    }
                    Debug.Log(PlayerPrefs.GetString("CtryMapProgress"));
                }
                AudioController.Instance.PlayClip(sfxClipList[3]);
                StartCoroutine(UITKController.Instance.InitializeCountryList());
                StartCoroutine(UITKController.Instance.InitializeStatsCountryList());
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
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
            switch (countrySO.terms)
            {
                case CountrySO.Terms.Free:
                    SetupCountry(countrySO);
                    break;
                case CountrySO.Terms.WatchAds:
                    rewardCountrySO = countrySO;
                    UITKController.Instance.ShowUISegment("ad-popup");
                    break;
                case CountrySO.Terms.Locked:
                    UITKController.Instance.ShowUISegment("iap-popup");
                    break;
            }
        }
    }
    public void SetCountryMap(CountrySO countrySO)
    {
        // validation // 
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
    }
    void InitializeMainCamera()
    {
        mainCamera.transform.position = new Vector3(0, 0, -10); // Example position
        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // No rotation necessary in 2D, but just to be clear
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
    public void Capture()
    {
        try
        {
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            rt.antiAliasing = 2; // Set anti-aliasing as needed
            rt.filterMode = FilterMode.Bilinear; // Set the filter mode
            rt.wrapMode = TextureWrapMode.Clamp; // Set the wrap mode

            cameraToCapture.enabled = true;
            cameraToCapture.targetTexture = rt;
            cameraToCapture.Render(); // Render the camera's view to its RenderTexture

            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();

#if UNITY_EDITOR
            byte[] bytes = texture.EncodeToPNG();

            File.WriteAllBytes(Application.persistentDataPath + "/capturedImage.png", bytes);
            Debug.Log("Saved Image to " + Application.persistentDataPath + "/capturedImage.png");
#elif UNITY_ANDROID
	        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(texture, "ColorTheMap", $"{countrySO.ctryName}.png", ( success, path ) => Debug.Log( "Media save result: " + success + " " + path ) );
#elif UNITY_IOS
#endif
            Destroy(texture);
            Destroy(rt);
            // Show message prompt (store successful) //
            GameObject.Find("Camera Save Message").GetComponent<Animator>().Play("Text Fade", 0, 0);
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
    void SetParticleSystemGradient(ThemeSO themeSO)
    {
        var mainModule = GameObject.Find("Confetti").GetComponent<ParticleSystem>().main;

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
            // Get the string from PlayerPrefs and convert it to a list of strings
            var indicesString = PlayerPrefs.GetString("CtryMapProgress");
            var indexList = Helper.ConvertStringToList(indicesString);

            // Convert each string index to int and then map to CountrySO
            return indexList.Select(strIndex =>
            {
                if (int.TryParse(strIndex, out int index))
                {
                    return countryList[index]; // Ensure that this index is within the bounds of countryList
                }
                else
                {
                    Debug.LogWarning($"Failed to parse '{strIndex}' to int");
                    return null; // or handle differently
                }
            }).Where(country => country != null).ToList(); // Exclude null values if any parse failed
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred: {e.Message}");
            return new List<CountrySO>();
        }
    }
}