using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TransformAnarchy
{
    public class PositionalGizmo : Gizmo<PositionalGizmoComponent>
    {

        // Init stuff we will need for this drag. includes the starting rotation, total angle rotated and the axis to do it on
        public override void OnDragStart(DragInformation eventInfo)
        {
            _startingPosition = transform.position;
            _totalMoveSoFar = 0;

            Vector3 AxisChange = Vector3.zero;

            switch (eventInfo.ModifyAxis)
            {
                case Axis.NONE:
                    return;

                case Axis.X:
                    AxisChange = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.right : transform.right;
                    break;

                case Axis.Y:
                    AxisChange = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.up : transform.up;
                    break;

                case Axis.Z:
                    AxisChange = (_rotationMode == ToolSpace.GLOBAL) ? Vector3.forward : transform.forward;
                    break;

            }

            _startingAxis = AxisChange;

        }

        public override void OnDrag(DragInformation eventInfo)
        {

            Vector3 thisPosition = Vector3.zero;
            Vector3 projVec = Vector3.Project(eventInfo.DragDelta, _startingAxis);
            _totalMoveSoFar += (Vector3.Dot(_startingAxis, projVec) > 0 ? 1 : -1) * projVec.magnitude;

            // Snap
            if (TA.MainController.ShouldSnap)
            {

                float gridStepSize = 1f / TA.MainController.GridSubdivision;

                float gridRound = Mathf.RoundToInt(_totalMoveSoFar / gridStepSize);

                if (gridRound != 0)
                {
                    thisPosition += _startingAxis * (gridRound * gridStepSize);
                }
            }
            else
            {
                thisPosition += _startingAxis * _totalMoveSoFar;
            }

            transform.position = _startingPosition + thisPosition;

            UpdateGizmoTransforms();

        }

        public override void OnDragEnd(DragInformation eventInfo)
        {
            TA.MainController.UpdateBuilderGridToGizmo();
        }

        private float _totalMoveSoFar;
        private Vector3 _startingPosition;
        private Vector3 _startingAxis;

        public void UpdateRotation(Quaternion newRot)
        {
            transform.rotation = newRot;
            UpdateGizmoTransforms();
        }

        public override Quaternion XAxisRotation() => Quaternion.LookRotation(Vector3.right, Vector3.up);
        public override Quaternion YAxisRotation() => Quaternion.LookRotation(Vector3.up, -Vector3.forward);
        public override Quaternion ZAxisRotation() => Quaternion.LookRotation(Vector3.forward, Vector3.up);

    }
}