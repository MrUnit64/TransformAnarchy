namespace RotationAnarchy
{
    using Parkitect.UI;
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ConstructWindowToggle : ModChange
    {
        private GameObject menuCanvasRoot;
        private GameObject raButtonGo;
        private Sprite gizmoButtonSprite;

        public override void OnChangeApplied()
        {
            menuCanvasRoot = GameObject.Find("MenuCanvas");
            var painterButton = menuCanvasRoot.transform.Find("BottomMenu/Painter").gameObject;
            //Disable the painter button before cloning, so we can initialize it before its Awake is called
            painterButton.SetActive(false);

            raButtonGo = GameObject.Instantiate(painterButton, painterButton.transform.parent);
            raButtonGo.name = RotationAnarchyMod.Instance.getName();

            //Features setup
            GameObject.Destroy(raButtonGo.GetComponent<UIMenuWindowButton>());
            var raWindowButton = raButtonGo.AddComponent<RAWindowButton>();
            //at this point we expect the hotkey to be already registered
            raWindowButton.hotkeyIdentifier = RotationAnarchyMod.ActiveToggle.Identifier;
            var toggle = raButtonGo.GetComponent<Toggle>();
            UIUtil.RemoveListeners(toggle);
            toggle.onValueChanged.AddListener( x => raWindowButton.onChanged() );

            //Graphical adjustments
            gizmoButtonSprite = RotationAnarchyMod.Instance.LoadSpritePNG("img/ui_icon_rotationGizmo");
            var tooltip = raButtonGo.GetComponent<UITooltip>();
            tooltip.text = raButtonGo.name;

            var colors = toggle.colors;
            colors.normalColor = new Color(0.792f, 0.805f, 1f, 1f);
            colors.highlightedColor = Color.white;
            toggle.colors = colors;
            
            var rect = raButtonGo.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(531, -15);
            
            Image image = raButtonGo.transform.Find("Image").GetComponent<Image>();
            image.sprite = gizmoButtonSprite;

            painterButton.SetActive(true);
            raButtonGo.SetActive(true);
        }

        public override void OnChangeReverted()
        {
            GameObject.Destroy(raButtonGo);
        }

    }


}