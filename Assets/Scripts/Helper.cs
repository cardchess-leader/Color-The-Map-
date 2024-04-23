using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    // Method to convert hex to RGB
    public static Color HexToColor(string hex)
    {
        // Remove the '#' character if it's present
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // Assume full opacity if not specified
        if (hex.Length == 6)
        {
            hex += "FF"; // Adds alpha value of 255 (fully opaque) if not specified
        }

        // Convert hex to integer and extract each color component
        int argb = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        float red = (argb >> 24) & 255;
        float green = (argb >> 16) & 255;
        float blue = (argb >> 8) & 255;
        float alpha = argb & 255;

        // Normalize the components by dividing by 255
        return new Color(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f);
    }

    public static Color GetAverageColor(Color[] colors, float alpha = 1)
    {
        float totalR = 0, totalG = 0, totalB = 0;
        int count = colors.Length;

        foreach (Color color in colors)
        {
            totalR += color.r;
            totalG += color.g;
            totalB += color.b;
        }

        return new Color(totalR / count, totalG / count, totalB / count, alpha);
    }

    public static Color GetOppositeColor(Color color, float alpha = 1)
    {
        return new Color(1 - color.r, 1 - color.g, 1 - color.b, alpha);
    }
}
