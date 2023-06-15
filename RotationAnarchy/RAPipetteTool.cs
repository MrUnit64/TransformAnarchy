namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;

    public class RAPipetteTool : ObjectPipetteTool
    {
        public override bool canEscapeMouseTool()
        {
            return true;
        }

        protected override bool isPickableObject(BuildableObject buildableObject)
        {
            return RAController.AllowedBuilderTypes.Contains(typeof(BuildableObject)) && buildableObject.isAvailable();
        }
    } 
}