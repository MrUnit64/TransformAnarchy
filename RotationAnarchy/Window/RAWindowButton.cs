namespace RotationAnarchy
{
    using Parkitect.UI;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class RAWindowButton : UIMenuButton, IPointerClickHandler
    {
        private Image icon;
        private Toggle toggle;

        protected override void Awake()
        {
            base.Awake();

            //Graphical adjustments
            var tooltip = gameObject.GetComponent<UITooltip>();
            tooltip.text = gameObject.name;

            toggle = gameObject.GetComponent<Toggle>();
            var colors = toggle.colors;
            colors.normalColor = new Color(0.792f, 0.805f, 1f, 1f);
            colors.highlightedColor = Color.white;
            toggle.colors = colors;

            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(531, -15);

            icon = gameObject.transform.Find("Image").GetComponent<Image>();

            RotationAnarchyMod.Controller.OnActiveChanged += OnRAActiveChanged;
            OnRAActiveChanged(RotationAnarchyMod.Controller.Active);
        }

        protected override void onSelected()
        {
            if (!RotationAnarchyMod.Controller.Active)
                return;

            var prefab = RotationAnarchyMod.Controller.ConstructWindowPrefab();
            if (prefab == null)
                RotationAnarchyMod.Instance.ERROR("Window prefab is null");

            if(ScriptableSingleton<UIAssetManager>.Instance.uiWindowFrameGO == null)
                RotationAnarchyMod.Instance.ERROR("uiWindowFrameGO is null, what?");

            this.windowInstance = UIWindowsController.Instance.spawnWindow(prefab, null);
            this.windowInstance.OnClose += this.onWindowClose;
        }

        protected override void onDeselected()
        {
            if (this.windowInstance != null)
            {
                this.windowInstance.close();
                this.windowInstance = null;
            }
        }

        private void onWindowClose(UIWindowFrame window)
        {
            base.setSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                RotationAnarchyMod.Controller.ToggleRAActive();
            }
        }

        private void OnRAActiveChanged(bool state)
        {
            if(state)
            {
                toggle.enabled = true;
                toggle.isOn = false;
                icon.sprite = RotationAnarchyMod.Instance.LoadSpritePNG("img/ui_icon_rotationGizmo");
            }
            else
            {
                toggle.isOn = state;
                toggle.enabled = false;
                icon.sprite = RotationAnarchyMod.Instance.LoadSpritePNG("img/ui_icon_rotationGizmoDisabled");
            }
        }

        private UIWindowFrame windowInstance;
    }
}