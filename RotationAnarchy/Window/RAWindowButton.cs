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
        public static RAWindowButton Instance { get; private set; }

        private Image icon;
        private Toggle toggle;

        protected override void Awake()
        {
            base.Awake();

            Instance = this;

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
        }

        public void SetButtonEnabled(bool state)
        {
            if (state)
            {
                toggle.enabled = true;
                icon.sprite = RA.Instance.LoadSpritePNG("img/ui_icon_rotationGizmo");
            }
            else
            {
                toggle.isOn = state;
                toggle.enabled = false;
                icon.sprite = RA.Instance.LoadSpritePNG("img/ui_icon_rotationGizmoDisabled");
            }
        }

        public void SetWindowOpened(bool state)
        {
            toggle.isOn = state;
        }

        protected override void onSelected()
        {
            if (!RA.Controller.Active)
                return;

            var prefab = ConstructWindowPrefab();
            if (prefab == null)
                RA.Instance.ERROR("Window prefab is null");

            if(ScriptableSingleton<UIAssetManager>.Instance.uiWindowFrameGO == null)
                RA.Instance.ERROR("uiWindowFrameGO is null, what?");

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

        public RAWindow ConstructWindowPrefab()
        {
            var WindowPrefab = new GameObject(RA.Instance.getName());
            WindowPrefab.SetActive(false);

            var rect = WindowPrefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(98, 43);
            WindowPrefab.AddComponent<CanvasRenderer>();
            var window = WindowPrefab.AddComponent<RAWindow>();

            var windowSettings = WindowPrefab.AddComponent<UIWindowSettings>();
            windowSettings.closable = true;
            windowSettings.defaultWindowPosition = new Vector2(Screen.width / 2f, 200);
            windowSettings.title = RA.Instance.getName();
            windowSettings.uniqueTag = RA.Instance.getName();
            windowSettings.uniqueTagString = RA.Instance.getName();

            WindowPrefab.SetActive(true);
            return window;
        }

        private void onWindowClose(UIWindowFrame window)
        {
            base.setSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                RA.Controller.ToggleRAActive();
            }
        }

        private UIWindowFrame windowInstance;
    }
}