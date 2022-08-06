namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;

    public class PlacementModeGizmo : RotationAxisGizmo
    {
        public PlacementModeGizmo(string cmdBufferID, GizmoAxis axis) : base(cmdBufferID, axis)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            SetActive(false);

            if (RA.Controller.Active)
            {
                if (RA.Controller.GameState == ParkitectState.Placement)
                {
                    if (RA.Controller.ActiveBuilder)
                    {
                        if (RA.Controller.ActiveGhost)
                        {
                            SetActive(true);
                            SnapToActiveBuilder();
                        }
                    }
                }
            }
        }
    }
}