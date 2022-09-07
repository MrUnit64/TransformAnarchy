using UnityEngine;

namespace RotationAnarchy.Internal.Utils
{
    public static class ColorExtensions
    {
        public static Color Scale(this Color color, float factor) => 
            new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }
}