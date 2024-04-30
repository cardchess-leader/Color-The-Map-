using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public List<CountrySO> countryList = new List<CountrySO>();
    public List<ThemeSO> themeList = new List<ThemeSO>();
    public GameObject canvas;
    public GestureController gestureController;
    public GameObject mapContainer;
    public GameObject[] paintBrushSet = new GameObject[4];
    public Color? selectedColor;
    public GameObject tutorial;
    public Camera cameraToCapture;
    public RenderTexture renderTexture;
    public float initialOrthographicSize;
    [System.NonSerialized] public float minZoom = 0.5f; // Minimum zoom limit
    [System.NonSerialized] public CountrySO countrySO;
    Camera mainCamera;
    Color[] regionsColor; // Subject Map's colors for each region
    void Awake()
    {
        instance = this;
    }
    void OnEnable()
    {
        InitializePlayerPref();
        Initialize();
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
            PlayerPrefs.SetInt("TutorialViewed", 0);
        }
    }
    void Initialize()
    {
        if (PlayerPrefs.GetInt("TutorialViewed", 0) == 0)
        {
            tutorial.SetActive(true);
        }
        else
        {
            InitializeGame();
        }
    }
    public void InitializeGame(bool tutorialEnd = false)
    {
        canvas.SetActive(true);
        SetTheme(0, true);
        UITKController.instance.ShowUISegment("footer");
        if (tutorialEnd)
        {
            UITKController.instance.ShowUISegment("select-map");
        }
    }
    public void EndTutorial()
    {
        PlayerPrefs.SetInt("TutorialViewed", 1);
    }
    public void SetTheme(int themeIndex = 0, bool update = false) // -1 means just use playerpref value, no update //
    {
        if (!update)
        {
            if (themeIndex == PlayerPrefs.GetInt("ThemeIndex"))
            {
                return;
            }
            else
            {
                PlayerPrefs.SetInt("ThemeIndex", themeIndex);
            }
        }
        ThemeSO themeSO = themeList[PlayerPrefs.GetInt("ThemeIndex")];
        for (int i = 0; i < 4; i++)
        {
            paintBrushSet[i].GetComponent<Image>().color = themeSO.themeColors[i];
        }
    }
    public void SelectColor(int index)
    {
        selectedColor = paintBrushSet[index].GetComponent<Image>().color;
        foreach (GameObject brush in paintBrushSet)
        {
            brush.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
        paintBrushSet[index].transform.GetChild(0).GetComponent<Image>().enabled = true;
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

            var indices = countrySO.mapSO.adjMatrix[targetIndex].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(index => int.TryParse(index.Trim(), out int parsedIndex) ? parsedIndex : -1)
                                .Where(index => index != -1).ToList();

            if (indices.Any(adjIndex => adjIndex >= regionsColor.Length || regionsColor[adjIndex] == selectedColor.Value))
            {
                return;
            }

            SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.color = selectedColor.Value;
            regionsColor[targetIndex] = selectedColor.Value;
        }
    }

    public void SetCountryMap(string ctryName)
    {
        // validation // 
        countrySO = GetCountrySO(ctryName);
        if (countrySO == null)
        {
            return;
        }
        if (mapContainer.transform.childCount > 0)
        {
            Destroy(mapContainer.transform.GetChild(0).gameObject);
        }
        InitializeMainCamera();
        GameObject countryMap = Instantiate(countrySO.countryMapPrefab, Vector3.zero, Quaternion.identity, mapContainer.transform);
        regionsColor = new Color[countrySO.mapSO.adjMatrix.Count];
    }

    void InitializeMainCamera()
    {
        mainCamera.transform.position = new Vector3(0, 0, -10); // Example position
        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // No rotation necessary in 2D, but just to be clear
        mainCamera.orthographicSize = 5;
    }

    public CountrySO GetCountrySO(string ctryName)
    {
        return GameManager.instance.countryList.FirstOrDefault(ctrySO => ctrySO.ctryName == ctryName);
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
            canvas.transform.Find("Camera Save Message").GetComponent<Animator>().Play("Text Fade", 0, 0);
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

}
