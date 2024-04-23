using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSO", menuName = "Color-The-Map/MapSO", order = 1)]
public class MapSO : ScriptableObject
{
    public string ctryName;
    public int numRegions;
    public List<string> adjMatrix = new List<string>(); // each element should be string of comma separated //
}
