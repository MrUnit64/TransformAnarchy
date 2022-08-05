namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ChangeUIBackgroundGraphics : ModChange
    {
        private GameObject menuCanvasRoot;

        // Cached values
        private Vector2 cached_Clockbar_BG_sizeDelta;

        public override void OnChangeApplied()
        {
            // root object for game HUD
            menuCanvasRoot = GameObject.Find("MenuCanvas");
            // the background graphic for bottom buttons
            var clockbarBgRect = menuCanvasRoot.transform.Find("Radial_BG/Clockbar_BG").GetComponent<RectTransform>();
            cached_Clockbar_BG_sizeDelta = clockbarBgRect.sizeDelta;
            var sizeDelta = clockbarBgRect.sizeDelta;
            // extend it horizontally so it covers our button
            clockbarBgRect.sizeDelta = new Vector2(562, sizeDelta.y);
        }

        public override void OnChangeReverted()
        {
            // the background graphic for bottom buttons
            var radialBgRect = menuCanvasRoot.transform.Find("Radial_BG/Clockbar_BG").GetComponent<RectTransform>();
            radialBgRect.sizeDelta = cached_Clockbar_BG_sizeDelta;
        }
    }
}