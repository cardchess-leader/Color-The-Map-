using System;
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
    public GestureController gestureController;
    public GameObject mapContainer;
    public GameObject[] paintBrushSet = new GameObject[4];
    public Color? selectedColor;
    public GameObject tutorial;
    Camera mainCamera;
    Color[] regionsColor; // Subject Map's colors for each region
    MapSO mapSO; // Subject Map's MapSO
    void Awake()
    {
        instance = this;
    }
    void OnEnable()
    {
        InitializePlayerPref();
        SetTheme(0, true);
        RunTutorial();
    }
    void Start()
    {
        mainCamera = Camera.main;
    }
    void InitializePlayerPref()
    {
        if (PlayerPrefs.GetInt("Initialize") != 1)
        {
            PlayerPrefs.SetInt("ThemeIndex", 0);
            PlayerPrefs.SetInt("Initialize", 1);
            PlayerPrefs.SetInt("VersionCode", 0);
        }
    }
    void RunTutorial()
    {
        if (PlayerPrefs.GetInt("TutorialViewed", 0) == 0)
        {
            tutorial.SetActive(true);
        }
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
                Debug.Log("Invalid target index or collider name.");
                return;
            }

            var indices = mapSO.adjMatrix[targetIndex].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(index => int.TryParse(index.Trim(), out int parsedIndex) ? parsedIndex : -1)
                                .Where(index => index != -1).ToList();

            if (indices.Any(adjIndex => adjIndex >= regionsColor.Length || regionsColor[adjIndex] == selectedColor.Value))
            {
                Debug.Log("Invalid Color or index out of range!");
                return;
            }

            SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.Log("No SpriteRenderer found!");
                return;
            }

            renderer.color = selectedColor.Value;
            regionsColor[targetIndex] = selectedColor.Value;
        }
    }

    public void SetCountryMap(string ctryName)
    {
        // validation // 
        CountrySO countrySO = GameManager.instance.countryList.FirstOrDefault(ctrySO => ctrySO.ctryName == ctryName);
        if (countrySO == null)
        {
            return;
        }
        if (transform.childCount > 0)
        {
            Destroy(mapContainer.transform.GetChild(0).gameObject);
        }
        GameObject countryMap = Instantiate(countrySO.countryMapPrefab, Vector3.zero, Quaternion.identity, mapContainer.transform);
        gestureController.targetMap = countryMap;
        mapSO = countryMap.GetComponent<CountryPrefab>().mapSO;
        regionsColor = new Color[mapSO.adjMatrix.Count];
    }
}
