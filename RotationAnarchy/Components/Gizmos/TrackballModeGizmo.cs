using System;
using RotationAnarchy.Internal.Utils;

namespace RotationAnarchy
{
    using Internal;
    using UnityEngine;

    public class TrackballModeGizmo : AbstractRenderedMeshGizmo
    {
        public const string TorusXName = "Torus X";
        public const string TorusYName = "Torus Y";
        public const string TorusZName = "Torus Z";

        private GizmoComponentGroup _groupTorus;
        private TorusGizmoComponent _visualTorusX;
        private TorusGizmoComponent _visualTorusY;
        private TorusGizmoComponent _visualTorusZ;

        private readonly GizmoOffsets _gizmoOffsets = new GizmoOffsets
        {
            X = new GizmoOffsetsBlock
            {
                positionOffset = new Vector3(0, 0, 0),
                rotationOffset = Quaternion.LookRotation(Vector3.right) * Quaternion.LookRotation(Vector3.down)
            },
            Y = new GizmoOffsetsBlock
                {positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.forward)},
            Z = new GizmoOffsetsBlock
                {positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.down)},
        };

        public TrackballModeGizmo() : base("Placement Mode Gizmo")
        {
        }

        protected override void OnConstruct()
        {
            base.OnConstruct();

            // Toruses
            AddGizmoComponent(_groupTorus = new GizmoComponentGroup("VisualGroupTorus"));

            AddGizmoComponent(_visualTorusX = new TorusGizmoComponent(TorusXName));
            _visualTorusX.Torus.ForwardZ = false;
            _visualTorusX.Torus.Arc = 360;
            _visualTorusX.Torus.Taper = false;
            _visualTorusX.transformInterpolator.TargetRotationOffset = _gizmoOffsets.X.rotationOffset;
            _visualTorusX.colorInterpolator.TargetColor = GetColorsForAxis(Axis.X).color;
            _visualTorusX.colorInterpolator.TargetOutlineColor = GetColorsForAxis(Axis.X).outlineColor;
            _groupTorus.AttachComponent(_visualTorusX);

            AddGizmoComponent(_visualTorusY = new TorusGizmoComponent(TorusYName));
            _visualTorusY.Torus.ForwardZ = false;
            _visualTorusY.Torus.Arc = 360;
            _visualTorusY.Torus.Taper = false;
            _visualTorusY.transformInterpolator.TargetRotationOffset = _gizmoOffsets.Y.rotationOffset;
            _visualTorusY.colorInterpolator.TargetColor = GetColorsForAxis(Axis.Y).color;
            _visualTorusY.colorInterpolator.TargetOutlineColor = GetColorsForAxis(Axis.Y).outlineColor;
            _groupTorus.AttachComponent(_visualTorusY);

            AddGizmoComponent(_visualTorusZ = new TorusGizmoComponent(TorusZName));
            _visualTorusZ.Torus.ForwardZ = false;
            _visualTorusZ.Torus.Arc = 360;
            _visualTorusZ.Torus.Taper = false;
            _visualTorusZ.transformInterpolator.TargetRotationOffset = _gizmoOffsets.Z.rotationOffset;
            _visualTorusZ.colorInterpolator.TargetColor = GetColorsForAxis(Axis.Z).color;
            _visualTorusZ.colorInterpolator.TargetOutlineColor = GetColorsForAxis(Axis.Z).outlineColor;
            _groupTorus.AttachComponent(_visualTorusZ);
        }

        private static GizmoAxisColorBlock GetColorsForAxis(Axis axis)
        {
            var colorBlock = RA.GizmoColors.GetForAxis(axis);
            if (RA.TrackballController.SelectedAxis == axis)
                return colorBlock;
            return new GizmoAxisColorBlock
            {
                color = colorBlock.color.Scale(0.3f),
                outlineColor = colorBlock.outlineColor.Scale(0.3f)
            };
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (RA.Controller.ActiveBuilder)
            {
                var deco = RA.Controller.ActiveBuilder.getBuiltObject() as Deco;
                if (deco)
                {
                    DebugGUI.DrawValue("orientToSurfaceNormal", deco.orientToSurfaceNormal);
                    DebugGUI.DrawValue("axisToOrientAlongSurfaceNormal", deco.axisToOrientAlongSurfaceNormal);
                    DebugGUI.DrawValue("stickToAnySurface", deco.stickToAnySurface);
                    DebugGUI.DrawValue("stickToHorizontalSurfaces", deco.stickToHorizontalSurfaces);
                    DebugGUI.DrawValue("stickToSurfaceForwardOffset", deco.stickToSurfaceForwardOffset);
                    DebugGUI.DrawValue("stickToVerticalSurfaces", deco.stickToVerticalSurfaces);
                    DebugGUI.DrawValue("stickToTerrain", deco.stickToTerrain);
                    DebugGUI.DrawValue("snapToDefaultHeightWhenStickingToSurface",
                        deco.snapToDefaultHeightWhenStickingToSurface);

                    if (deco.orientToSurfaceNormal)
                        return;
                }
            }

            if (RA.Controller.Active && RA.Controller.GameState == ParkitectState.Trackball &&
                RA.Controller.ActiveGhost)
            {
                Active = true;
                SnapToActiveGhost();

                Axis = RA.Controller.CurrentRotationAxis;

                var offsets = _gizmoOffsets.GetForAxis(Axis);
                DebugGUI.DrawValue("Current Placement Gizmo Axis", Axis);
                DebugGUI.DrawValue("Current Placement Gizmo offsetPos", offsets.positionOffset);
                DebugGUI.DrawValue("Current Placement Gizmo offsetRot", offsets.rotationOffset);


                // first we need total bounds of the object
                if (!RA.Controller.ActiveGhost) return;
                var ghost = RA.Controller.ActiveGhost;

                var tubeRadius = ComputeGizmoWidth(ghost.transform.position);

                // now we need to update gizmo dimensions to the size of the object
                var torusDiameterFromBounds = BoundsMax + (tubeRadius * 2 + 0.5f);


                _visualTorusX.Torus.TubeRadius = tubeRadius;
                _visualTorusX.Torus.Radius = torusDiameterFromBounds;
                _visualTorusY.Torus.TubeRadius = tubeRadius;
                _visualTorusY.Torus.Radius = torusDiameterFromBounds;
                _visualTorusZ.Torus.TubeRadius = tubeRadius;
                _visualTorusZ.Torus.Radius = torusDiameterFromBounds;
                
                _visualTorusX.colorInterpolator.TargetColor = GetColorsForAxis(Axis.X).color;
                _visualTorusX.colorInterpolator.TargetOutlineColor = GetColorsForAxis(Axis.X).outlineColor;
                _visualTorusY.colorInterpolator.TargetColor = GetColorsForAxis(Axis.Y).color;
                _visualTorusY.colorInterpolator.TargetOutlineColor = GetColorsForAxis(Axis.Y).outlineColor;
                _visualTorusZ.colorInterpolator.TargetColor = GetColorsForAxis(Axis.Z).color;
                _visualTorusZ.colorInterpolator.TargetOutlineColor = GetColorsForAxis(Axis.Z).outlineColor;

                var scale = RA.Controller.HoldingChangeHeightKey ? 1.0f : -1.0f;
                var t = _groupTorus.transformInterpolator.transform;
                t.localScale = new Vector3(t.localScale.x, t.localScale.y, scale);
            }
            else
            {
                Active = false;
            }
        }

        protected override float GetMaxBoundsFromBounds(Bounds bounds) =>
            bounds.extents.magnitude;
    }
}