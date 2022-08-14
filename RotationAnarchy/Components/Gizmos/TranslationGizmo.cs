namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using RotationAnarchy.Internal;
    using RotationAnarchy.Internal.Utils.Meshes;
    using UnityEngine;

    public class TranslationGizmo : AbstractRenderedMeshGizmo
    {
        GizmoOffsets gizmoOffsets = new GizmoOffsets()
        {
            X = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.right) },
            Y = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.forward) },
            Z = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.down) },
        };

        public TranslationGizmo() : base("Translation Gizmo") { }

        protected override void OnConstruct()
        {
            base.OnConstruct();

        }

        protected override void OnAxisChanged()
        {
            base.OnAxisChanged();

            var offsets = gizmoOffsets.GetForAxis(Axis);
            var colors = RA.GizmoColors.GetForAxis(Axis);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (RA.Controller.Active && RA.Controller.GameState == ParkitectState.None && RA.Controller.GizmoActive && RA.Controller.SelectedBuildable)
            {
                Active = true;
                SnapToSelectedBuildable();

                var buildable = RA.Controller.SelectedBuildable;

                if (Input.GetKeyDown(KeyCode.Keypad8))
                    buildable.transform.position += Vector3.forward;

                if (Input.GetKeyDown(KeyCode.Keypad4))
                    buildable.transform.position += Vector3.left;

                if (Input.GetKeyDown(KeyCode.Keypad6))
                    buildable.transform.position += Vector3.right;

                if (Input.GetKeyDown(KeyCode.Keypad2))
                    buildable.transform.position += Vector3.back;
            }
            else
            {
                Active = false;
            }
        }
    }
}