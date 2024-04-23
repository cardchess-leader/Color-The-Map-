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
    public GameObject countryMapPrefab;
}
