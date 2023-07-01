using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TransformAnarchy
{
    public class RotationalGizmo : Gizmo<RotationalGizmoComponent>
    {

        // Init stuff we will need for this drag. includes the starting rotation, total angle rotated and the axis to do it on
        public override void OnDragStart(DragInformation eventInfo)
        {
            _startingRotation = transform.rotation;
            _totalRotationAngleSoFar = 0;

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

        // Handles the 
        public override void OnDrag(DragInformation eventInfo)
        {

            Quaternion axisRotation = Quaternion.identity;
            Vector3 lastLocalPosition = (eventInfo.CurrentDragPosition - eventInfo.DragDelta) - transform.position;
            Vector3 thisLocalPosition = eventInfo.CurrentDragPosition - transform.position;

            float currentRotationDelta = Vector3.SignedAngle(lastLocalPosition, thisLocalPosition, _startingAxis);

            _totalRotationAngleSoFar += currentRotationDelta;

            // Snap
            if (TA.MainController.ShouldSnap)
            {

                float gridStepSize = 90f / TA.MainController.GridSubdivision;

                float gridRound = Mathf.RoundToInt(_totalRotationAngleSoFar / gridStepSize);

                if (gridRound != 0)
                {
                    axisRotation = Quaternion.AngleAxis(gridRound * gridStepSize, _startingAxis);
                }
            }
            else
            {
                axisRotation = Quaternion.AngleAxis(_totalRotationAngleSoFar, _startingAxis);
            }

            transform.rotation = axisRotation * _startingRotation;

            UpdateGizmoTransforms();
            TA.MainController.UpdateBuilderGridToGizmo();

        }

        private float _totalRotationAngleSoFar;
        private Quaternion _startingRotation;
        private Vector3 _startingAxis;

        public void UpdatePosition(Vector3 newPos)
        {
            transform.position = newPos;
            UpdateGizmoTransforms();
        }

        public override Quaternion XAxisRotation() => Quaternion.LookRotation(Vector3.right, Vector3.up);
        public override Quaternion YAxisRotation() => Quaternion.LookRotation(Vector3.up, -Vector3.forward);
        public override Quaternion ZAxisRotation() => Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }
}