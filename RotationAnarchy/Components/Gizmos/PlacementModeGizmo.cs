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

            if (RA.Controller.Active && RA.Controller.GameState == ParkitectState.Placement && RA.Controller.ActiveGhost)
            {
                SetActive(true);
                SnapToActiveGhost();

                if (RA.Controller.IsDirectionHorizontal)
                    Axis = GizmoAxis.Z;
                else
                    Axis = GizmoAxis.Y;
            }
            else
            {
                SetActive(false);
            }
        }
    }
}