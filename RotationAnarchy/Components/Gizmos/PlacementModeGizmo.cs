namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using RotationAnarchy.Internal.Utils.Meshes;
    using UnityEngine;

    public class PlacementModeGizmo : AbstractRenderedMeshGizmo
    {
        TorusGizmoComponent torusComponent;
        TorusGizmoComponent torusComponent1;
        TorusGizmoComponent torusComponent2;

        GizmoOffsets gizmoOffsets = new GizmoOffsets()
        {
            X = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = new Quaternion() },
            Y = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0.1f, 0), rotationOffset = Quaternion.LookRotation(Vector3.up) },
            Z = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.forward) },
        };

        public PlacementModeGizmo() : base("Placement Mode Gizmo") { }

        protected override void OnConstruct()
        {
            base.OnConstruct();
            torusComponent = new TorusGizmoComponent("Rotation Circle");
            torusComponent1 = new TorusGizmoComponent("Rotation Circle1");
            torusComponent2 = new TorusGizmoComponent("Rotation Circle2");
            AddGizmoComponent(torusComponent);
            AddGizmoComponent(torusComponent1);
            AddGizmoComponent(torusComponent2);
        }

        protected override void OnAxisChanged()
        {
            base.OnAxisChanged();

            var offsets = gizmoOffsets.GetForAxis(Axis);

            var colors = RA.GizmoColors.GetForAxis(Axis);

            torusComponent.colorInterpolator.TargetColor = colors.color;
            torusComponent.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            torusComponent1.colorInterpolator.TargetColor = colors.color;
            torusComponent1.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            torusComponent2.colorInterpolator.TargetColor = colors.color;
            torusComponent2.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            torusComponent.transformInterpolator.TargetPositionOffset = offsets.positionOffset;
            torusComponent.transformInterpolator.TargetRotationOffset = offsets.rotationOffset;

            if (Axis == GizmoAxis.Y)
                offsets.positionOffset += new Vector3(0, 0.25f, 0);

            torusComponent1.transformInterpolator.TargetPositionOffset = offsets.positionOffset;
            torusComponent1.transformInterpolator.TargetRotationOffset = offsets.rotationOffset;

            if (Axis == GizmoAxis.Y)
                offsets.positionOffset -= new Vector3(0, -0.5f, 0);

            torusComponent2.transformInterpolator.TargetPositionOffset = offsets.positionOffset;
            torusComponent2.transformInterpolator.TargetRotationOffset = offsets.rotationOffset;
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
                    float width = Mathf.Max(xWidth, zWidth) + (tubeRadius * 2 + 0.5f);

                    torusComponent.Torus.TubeRadius = tubeRadius;
                    torusComponent.Torus.Radius = width / 2f;
                    torusComponent1.Torus.TubeRadius = tubeRadius;
                    torusComponent1.Torus.Radius = width / 2f ;
                    torusComponent2.Torus.TubeRadius = tubeRadius;
                    torusComponent2.Torus.Radius = width / 2f;

                    if (Axis == GizmoAxis.Z)
                    {
                        torusComponent1.Torus.Radius += 0.25f;
                        torusComponent2.Torus.Radius += 0.5f;
                    }
                }
            }
            else
            {
                Active = false;
            }
        }
    }
}