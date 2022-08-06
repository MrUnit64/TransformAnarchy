namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class HighlightOverlayControllerUtil
    {
        private static MaterialPropertyBlock materialPropertyBlock;

        public static void ChangeHighlightColor(Renderer renderer, Color color)
        {
            if (materialPropertyBlock == null)
                materialPropertyBlock = new MaterialPropertyBlock();


            renderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetInt("_OutlineColor", (int)ConvertColorTo8Bit(color));
            renderer.SetPropertyBlock(materialPropertyBlock);
        }

        public static byte ConvertColorTo8Bit(Color color)
        {
            int r = (int)(color.r * 255f);
            int g = (int)(color.g * 255f);
            int b = (int)(color.b * 255f) >> 6;
            int w = g >> 5;
            int z = r >> 5;
            return (byte)((b << 6) | (w << 3) | z);
        }
    }


}