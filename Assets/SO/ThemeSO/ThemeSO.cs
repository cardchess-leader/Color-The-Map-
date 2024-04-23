using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeSO", menuName = "Color-The-Map/ThemeSO", order = 2)]
public class ThemeSO : ScriptableObject
{
    public string themeName;
    public Color[] themeColors = new Color[4];
}
