namespace RotationAnarchy.Internal
{
    using System;
    using System.Collections.Generic;

    public static class MathUtil
    {
        public static float Remap(this float v, float a0, float a1, float b0, float b1)
        {
            return (v - a0) / (a1 - a0) * (b1 - b0) + b0;
        }
    }
}