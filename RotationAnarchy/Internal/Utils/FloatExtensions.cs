using UnityEngine;

namespace RotationAnarchy.Internal.Utils
{
    public static class FloatExtensions
    {
        public static float RoundToMultipleOf(this float number, float value) =>
            Mathf.Round(number / value) * value;
    }
}