namespace RotationAnarchy
{
    using Parkitect.UI;
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class HUDGizmoButtonCreator : ModChange
    {
        private GameObject menuCanvasRoot;
        private GameObject gizmoButtonGo;
        private Sprite gizmoButtonSprite;

        private RAWindow window;

        public override void OnChangeApplied()
        {
            menuCanvasRoot = GameObject.Find("MenuCanvas");
            var painterButton = menuCanvasRoot.transform.Find("BottomMenu/Painter").gameObject;
            //Disable the painter button before cloning, so we can initialize it before its Awake is called
            painterButton.SetActive(false);

            gizmoButtonGo = GameObject.Instantiate(painterButton, painterButton.transform.parent);
            gizmoButtonGo.name = RotationAnarchyMod.Instance.getName();

            //Features setup
            var windowButton = gizmoButtonGo.GetComponent<UIMenuWindowButton>();
            //at this point we expect the hotkey to be already registered
            windowButton.hotkeyIdentifier = RotationAnarchyMod.ActiveToggle.Identifier;
            //windowButton.windowContentGO = new RAWindow().ConstructGameObject();

            //Graphical adjustments
            gizmoButtonSprite = RotationAnarchyMod.Instance.LoadSpritePNG("img/ui_icon_rotationGizmo");
            var tooltip = gizmoButtonGo.GetComponent<UITooltip>();
            tooltip.text = gizmoButtonGo.name;
            var toggle = gizmoButtonGo.GetComponent<Toggle>();
            var colors = toggle.colors;
            colors.normalColor = new Color(0.792f, 0.805f, 1f, 1f);
            colors.highlightedColor = Color.white;
            toggle.colors = colors;
            var rect = gizmoButtonGo.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(531, -15);
            Image image = gizmoButtonGo.transform.Find("Image").GetComponent<Image>();
            image.sprite = gizmoButtonSprite;


            painterButton.SetActive(true);
            gizmoButtonGo.SetActive(true);
        }

        public override void OnChangeReverted()
        {
            GameObject.Destroy(gizmoButtonGo);
        }

    }


}