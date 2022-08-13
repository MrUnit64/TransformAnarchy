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

        GizmoOffsets gizmoOffsets = new GizmoOffsets()
        {
            X = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.right) },
            Y = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.forward) },
            Z = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.down) },
        };

        public PlacementModeGizmo() : base("Placement Mode Gizmo") { }

        protected override void OnConstruct()
        {
            base.OnConstruct();
            AddGizmoComponent(torusComponent = new TorusGizmoComponent("Rotation Circle"));
            torusComponent.Torus.forwardZ = false;

            AddGizmoComponent(ArrowComponent1 = new ArrowGizmoComponent("Rotation Arrow1"));
            ArrowComponent1.transform.SetParent(torusComponent.transform, false);

            AddGizmoComponent(ArrowComponent2 = new ArrowGizmoComponent("Rotation Arrow2"));
            ArrowComponent2.transform.SetParent(torusComponent.transform, false);
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

        }

        Vector3 rotationVec;

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (RA.Controller.Active && RA.Controller.GameState == ParkitectState.Placement && RA.Controller.ActiveGhost)
            {
                Active = true;
                SnapToActiveGhost();

                if (RA.Controller.IsDirectionHorizontal)
                    Axis = Axis.Z;
                else
                    Axis = Axis.Y;

                // first we need total bounds of the object
                if (RA.Controller.ActiveGhost)
                {
                    var ghost = RA.Controller.ActiveGhost;

                    float tubeRadius = ComputeGizmoWidth(ghost.transform.position);

                    // now we need to update gizmo dimensions to the size of the object
                    float xWidth = GhostBounds.max.x - GhostBounds.min.x;
                    float zWidth = GhostBounds.max.z - GhostBounds.min.z;

                    float torusDiameterFromBounds = Mathf.Max(xWidth, zWidth) + (tubeRadius * 2 + 0.5f);

                    torusComponent.Torus.TubeRadius = tubeRadius;
                    torusComponent.Torus.Radius = torusDiameterFromBounds / 2f;

                    // needs to be flat number otherwise accumulates multiplication
                    float arrowRadius = tubeRadius + .1f; 

                    // Set arrow radius/length depending on tube size and radius
                    ArrowComponent1.Arrow.BottomRadius = arrowRadius;
                    ArrowComponent1.Arrow.Length = arrowRadius * 2;

                    // Set arrow radius/length depending on tube size and radius
                    ArrowComponent2.Arrow.BottomRadius = arrowRadius;
                    ArrowComponent2.Arrow.Length = arrowRadius * 2;

                    // Set up arrow offsets
                    Vector3 positionOffset = Vector3.left * (torusDiameterFromBounds / 2.0f);

                    // This one faces towards rotation only if shift held down
                    ArrowComponent1.transformInterpolator.TargetPositionOffset = positionOffset;
                    ArrowComponent1.transformInterpolator.TargetRotationOffset = Quaternion.LookRotation(Vector3.forward);

                    // This one faces towards rotation normally
                    ArrowComponent2.transformInterpolator.TargetPositionOffset = -positionOffset;
                    ArrowComponent2.transformInterpolator.TargetRotationOffset = Quaternion.LookRotation(-Vector3.forward);
                }
            }
            else
            {
                Active = false;
            }
        }
    }
}