using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TA
{
    public class RotationalGizmo : Gizmo<RotationalGizmoComponent>
    {

        public override void OnDrag(DragInformation eventInfo)
        {

            Vector3 AxisChange = Vector3.zero;
            Vector3 RotationCalcAxis = Vector3.zero;

            switch (eventInfo.ModifyAxis)
            {
                case Axis.NONE:
                    return;

                case Axis.X:
                    AxisChange = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.right : transform.right;
                    RotationCalcAxis = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.up : transform.up;
                    break;

                case Axis.Y:
                    AxisChange = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.up : transform.up;
                    RotationCalcAxis = (_rotationMode == ToolSpace.GLOBAL) ? -Vector3.forward : -transform.forward;
                    break;

                case Axis.Z:
                    AxisChange = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.forward : transform.forward;
                    RotationCalcAxis = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.up : transform.up;
                    break;

            }

            Vector3 lastLocalPosition = (eventInfo.CurrentDragPosition - eventInfo.DragDelta) - transform.position;
            Vector3 thisLocalPosition = eventInfo.CurrentDragPosition - transform.position;

            float rotationDelta = Vector3.SignedAngle(lastLocalPosition, thisLocalPosition, AxisChange);

            transform.rotation = Quaternion.AngleAxis(rotationDelta, AxisChange) * transform.rotation;

            UpdateGizmoTransforms();

        }

        public override Quaternion XAxisRotation() => Quaternion.LookRotation(Vector3.right, Vector3.up);
        public override Quaternion YAxisRotation() => Quaternion.LookRotation(Vector3.up, -Vector3.forward);
        public override Quaternion ZAxisRotation() => Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }
}