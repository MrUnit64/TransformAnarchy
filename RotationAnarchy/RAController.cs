namespace RotationAnarchy
{
    using Parkitect.UI;
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public enum ParkitectState
    {
        None,
        Placement
    }

    public class RAController : ModChange
    {
        public event Action<bool> OnActiveChanged;
        public bool Active { get; private set; }

        public override void OnChangeApplied()
        {
            RotationAnarchyMod.RAActiveHotkey.onKeyDown += ToggleRAActive;
        }

        public override void OnChangeReverted() { }

        public RAWindow ConstructWindowPrefab()
        {
            var WindowPrefab = new GameObject(RotationAnarchyMod.Instance.getName());
            WindowPrefab.SetActive(false);

            var rect = WindowPrefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(98, 43);
            WindowPrefab.AddComponent<CanvasRenderer>();
            var window = WindowPrefab.AddComponent<RAWindow>();

            var windowSettings = WindowPrefab.AddComponent<UIWindowSettings>();
            windowSettings.closable = true;
            windowSettings.defaultWindowPosition = new Vector2(Screen.width / 2f, 200);
            windowSettings.title = RotationAnarchyMod.Instance.getName();
            windowSettings.uniqueTag = RotationAnarchyMod.Instance.getName();
            windowSettings.uniqueTagString = RotationAnarchyMod.Instance.getName();

            WindowPrefab.SetActive(true);
            return window;
        }

        public void ToggleRAActive()
        {
            SetRAActive(!Active);
        }

        public void SetRAActive(bool state)
        {
            if(state != Active)
            {
                Active = state;
                OnActiveChanged?.Invoke(Active);
            }
        }
    }

}