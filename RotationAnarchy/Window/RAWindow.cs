namespace RotationAnarchy
{
    using Parkitect.UI;
    using System;
    using System.Collections.Generic;

    public class RAWindow : UIWindow
    {
        private void Update()
        {
            if (!RotationAnarchyMod.Controller.Active)
                this.windowFrame.close();
        }
    }


}