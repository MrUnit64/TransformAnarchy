namespace RotationAnarchy
{
    using Parkitect.UI;
    using System;
    using System.Collections.Generic;

    public class RAWindow : UIWindow
    {
        public static RAWindow Instance { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            Instance = this;
            RA.Controller.NotifyWindowState(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Instance = null;
            RA.Controller.NotifyWindowState(false);
        }
    }


}