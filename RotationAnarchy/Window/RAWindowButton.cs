namespace RotationAnarchy
{
    using Parkitect.UI;
    using System;
    using System.Collections.Generic;

    public class RAWindowButton : UIMenuButton
    {
        protected override void onSelected()
        {
            var prefab = RotationAnarchyMod.WindowController.ConstructWindowPrefab();
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

        private UIWindowFrame windowInstance;
    }
}