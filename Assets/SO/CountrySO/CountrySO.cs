using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CountrySO", menuName = "Color-The-Map/CountrySO", order = 0)]
public class CountrySO : ScriptableObject
{
    public enum Continent
    {
        Europe,
        Asia,
        North_America,
        South_America,
        Oceania,
        Africa
    }
    public enum Terms
    {
        Free,
        WatchAds,
        Locked
    }
    public string ctryName;
    public Continent continent;
    public Terms terms;
    public float maxZoom = 2;
    public GameObject countryMapPrefab;
    public MapSO mapSO;
    public int population;
    public float area;
    [TextArea(1, 8)]
    public string funFact;
}
