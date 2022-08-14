namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using RotationAnarchy.Internal;
    using RotationAnarchy.Internal.Utils.Meshes;
    using UnityEngine;

    public class PlacementModeGizmo : AbstractRenderedMeshGizmo
    {
        GizmoComponentGroup groupTorus;
        TorusGizmoComponent visualTorus1;
        TorusGizmoComponent visualTorus2;

        GizmoComponentGroup groupArrows;
        ArrowGizmoComponent visualArrow1;
        ArrowGizmoComponent visualArrow2;

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

            // Toruses
            AddGizmoComponent(groupTorus = new GizmoComponentGroup("VisualGroupTorus"));

            AddGizmoComponent(visualTorus1 = new TorusGizmoComponent("Torus 1"));
            visualTorus1.Torus.ForwardZ = false;
            visualTorus1.Torus.Arc = 175;
            visualTorus1.Torus.Taper = true;
            groupTorus.AttachComponent(visualTorus1);

            AddGizmoComponent(visualTorus2 = new TorusGizmoComponent("Torus 2"));
            visualTorus2.Torus.ForwardZ = false;
            visualTorus2.Torus.Arc = 175;
            visualTorus2.Torus.Taper = true;
            visualTorus2.transformInterpolator.Interpolate = false;
            visualTorus2.transformInterpolator.TargetRotationOffset = Quaternion.LookRotation(Vector3.back);
            groupTorus.AttachComponent(visualTorus2);

            // Arrows
            AddGizmoComponent(groupArrows = new GizmoComponentGroup("VisualGroupArrows"));
            groupTorus.AttachComponent(groupArrows);

            AddGizmoComponent(visualArrow1 = new ArrowGizmoComponent("Rotation Arrow1"));
            groupArrows.AttachComponent(visualArrow1);

            AddGizmoComponent(visualArrow2 = new ArrowGizmoComponent("Rotation Arrow2"));
            groupArrows.AttachComponent(visualArrow2);
        }

        protected override void OnAxisChanged()
        {
            base.OnAxisChanged();

            var offsets = gizmoOffsets.GetForAxis(Axis);
            var colors = RA.GizmoColors.GetForAxis(Axis);

            visualTorus1.colorInterpolator.TargetColor = colors.color;
            visualTorus1.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            visualTorus2.colorInterpolator.TargetColor = colors.color;
            visualTorus2.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            visualArrow1.colorInterpolator.TargetColor = colors.color;
            visualArrow1.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            visualArrow2.colorInterpolator.TargetColor = colors.color;
            visualArrow2.colorInterpolator.TargetOutlineColor = colors.outlineColor;

            groupTorus.transformInterpolator.TargetPositionOffset = offsets.positionOffset;
            groupTorus.transformInterpolator.TargetRotationOffset = offsets.rotationOffset;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (RA.Controller.ActiveBuilder)
            {
                Deco deco = RA.Controller.ActiveBuilder.getBuiltObject() as Deco;
                if (deco)
                {
                    DebugGUI.DrawValue("orientToSurfaceNormal", deco.orientToSurfaceNormal);
                    DebugGUI.DrawValue("axisToOrientAlongSurfaceNormal", deco.axisToOrientAlongSurfaceNormal);
                    DebugGUI.DrawValue("stickToAnySurface", deco.stickToAnySurface);
                    DebugGUI.DrawValue("stickToHorizontalSurfaces", deco.stickToHorizontalSurfaces);
                    DebugGUI.DrawValue("stickToSurfaceForwardOffset", deco.stickToSurfaceForwardOffset);
                    DebugGUI.DrawValue("stickToVerticalSurfaces", deco.stickToVerticalSurfaces);
                    DebugGUI.DrawValue("stickToTerrain", deco.stickToTerrain);
                    DebugGUI.DrawValue("snapToDefaultHeightWhenStickingToSurface", deco.snapToDefaultHeightWhenStickingToSurface);

                    if (deco.orientToSurfaceNormal)
                        return;
                }
            }

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

                    visualTorus1.Torus.TubeRadius = tubeRadius;
                    visualTorus1.Torus.Radius = torusDiameterFromBounds / 2f;
                    visualTorus2.Torus.TubeRadius = tubeRadius;
                    visualTorus2.Torus.Radius = torusDiameterFromBounds / 2f;

                    if (RA.Controller.HoldingChangeHeightKey)
                    {
                        visualTorus1.Torus.TaperEndRadius = tubeRadius;
                        visualTorus2.Torus.TaperEndRadius = tubeRadius;
                        visualTorus1.Torus.TaperStartRadius = tubeRadius * 0.1f;
                        visualTorus2.Torus.TaperStartRadius = tubeRadius * 0.1f;
                    }
                    else
                    {
                        visualTorus1.Torus.TaperEndRadius = tubeRadius * 0.1f;
                        visualTorus2.Torus.TaperEndRadius = tubeRadius * 0.1f;
                        visualTorus1.Torus.TaperStartRadius = tubeRadius;
                        visualTorus2.Torus.TaperStartRadius = tubeRadius;
                    }


                    // needs to be flat number otherwise accumulates multiplication
                    float arrowRadius = tubeRadius + tubeRadius/2;

                    // Set arrow radius/length depending on tube size and radius
                    visualArrow1.Arrow.BottomRadius = arrowRadius;
                    visualArrow1.Arrow.Length = arrowRadius * 2;

                    // Set arrow radius/length depending on tube size and radius
                    visualArrow2.Arrow.BottomRadius = arrowRadius;
                    visualArrow2.Arrow.Length = arrowRadius * 2;

                    // Set up arrow offsets
                    Vector3 positionOffset = Vector3.left * (torusDiameterFromBounds / 2.0f);
                    Vector3 arrowDirection = RA.Controller.HoldingChangeHeightKey ? -Vector3.forward : Vector3.forward;

                    // This one faces towards rotation only if shift held down
                    visualArrow1.transformInterpolator.TargetPositionOffset = positionOffset;
                    visualArrow1.transformInterpolator.TargetRotationOffset = Quaternion.LookRotation(arrowDirection);

                    // This one faces towards rotation normally
                    visualArrow2.transformInterpolator.TargetPositionOffset = -positionOffset;
                    visualArrow2.transformInterpolator.TargetRotationOffset = Quaternion.LookRotation(-arrowDirection);
                }
            }
            else
            {
                Active = false;
            }
        }
    }
}