namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using RotationAnarchy.Internal.Utils.Meshes;
    using UnityEngine;

    public class PlacementModeGizmo : AbstractRenderedMeshGizmo
    {
        TorusGizmoComponent torusComponent;
        ArrowGizmoComponent ArrowComponent1;
        ArrowGizmoComponent ArrowComponent2;

        float width = 0;

        GizmoOffsets gizmoOffsets = new GizmoOffsets()
        {
            X = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = new Quaternion() },
            Y = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.up) },
            Z = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.forward) },
        };

        public PlacementModeGizmo() : base("Placement Mode Gizmo") { }

        protected override void OnConstruct()
        {
            base.OnConstruct();
            torusComponent = new TorusGizmoComponent("Rotation Circle");
            ArrowComponent1 = new ArrowGizmoComponent("Rotation Arrow1");
            ArrowComponent2 = new ArrowGizmoComponent("Rotation Arrow2");
            AddGizmoComponent(torusComponent);
            AddGizmoComponent(ArrowComponent1);
            AddGizmoComponent(ArrowComponent2);
        }

        protected override void OnAxisChanged()
        {
            base.OnAxisChanged();

            var offsets = gizmoOffsets.GetForAxis(Axis);

            var colors = RA.GizmoColors.GetForAxis(Axis);

            torusComponent.colorInterpolator.TargetColor = colors.color;
            torusComponent.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            ArrowComponent1.colorInterpolator.TargetColor = colors.color;
            ArrowComponent1.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            ArrowComponent2.colorInterpolator.TargetColor = colors.color;
            ArrowComponent2.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            torusComponent.transformInterpolator.TargetPositionOffset = offsets.positionOffset;
            torusComponent.transformInterpolator.TargetRotationOffset = offsets.rotationOffset;

            // Set up arrow offsets
            Vector3 forwardDir = offsets.rotationOffset * (Vector3.left * (width / 2.0f));

            // Arrow rotation
            var arrowRotationOffset1 = offsets.rotationOffset * Quaternion.Euler(new Vector3(180, 0, 0));
            var arrowRotationOffset2 = offsets.rotationOffset * Quaternion.Euler(new Vector3(0, 0, 0));

            // This one faces towards rotation only if shift held down
            ArrowComponent1.transformInterpolator.TargetPositionOffset = offsets.positionOffset + forwardDir;
            ArrowComponent1.transformInterpolator.TargetRotationOffset = arrowRotationOffset1;

            // This one faces towards rotation normally
            ArrowComponent2.transformInterpolator.TargetPositionOffset = offsets.positionOffset - forwardDir;
            ArrowComponent2.transformInterpolator.TargetRotationOffset = arrowRotationOffset2;

        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (RA.Controller.Active && RA.Controller.GameState == ParkitectState.Placement && RA.Controller.ActiveGhost)
            {
                Active = true;
                SnapToActiveGhost();

                if (RA.Controller.IsDirectionHorizontal)
                    Axis = GizmoAxis.Z;
                else
                    Axis = GizmoAxis.Y;

                // first we need total bounds of the object
                if (RA.Controller.ActiveGhost)
                {
                    var ghost = RA.Controller.ActiveGhost;

                    float tubeRadius = ComputeGizmoWidth(ghost.transform.position);

                    // now we need to update gizmo dimensions to the size of the object
                    float xWidth = GhostBounds.max.x - GhostBounds.min.x;
                    float zWidth = GhostBounds.max.z - GhostBounds.min.z;

                    // Needs to be a class variable to properly offset the arrows in OnAxisChanged
                    width = Mathf.Max(xWidth, zWidth) + (tubeRadius * 2 + 0.5f);

                    torusComponent.Torus.TubeRadius = tubeRadius;
                    torusComponent.Torus.Radius = width / 2f;

                    float arrowRadius = width * 0.1f;

                    // Set arrow radius/length depending on tube size and radius
                    ArrowComponent1.Arrow.BottomRadius = arrowRadius;
                    ArrowComponent1.Arrow.Length = arrowRadius * 2;

                    // Set arrow radius/length depending on tube size and radius
                    ArrowComponent2.Arrow.BottomRadius = arrowRadius;
                    ArrowComponent2.Arrow.Length = arrowRadius * 2;


                }
            }
            else
            {
                Active = false;
            }
        }
    }
}