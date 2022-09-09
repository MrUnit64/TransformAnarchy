using System;
using RotationAnarchy.Internal.Utils;
using UnityEngine;

namespace RotationAnarchy.Internal
{
    public class TrackballController : ModComponent
    {
        private Vector3? _dragStart;
        private Quaternion? _initialRotation;
        private float? _hemisphereRadius;
        private Plane? _viewPlane;
        private Ray _mouseRay;
        private Camera _camera;

        private Vector3? _axisRayPos;
        private Vector3? _localRotAxis;
        public Quaternion Rotation { get; private set; }
        public float? AngleAmount { get; private set; }

        public Axis? SelectedAxis { get; private set; }


        public void Reset()
        {
            _dragStart = null;
            _initialRotation = null;
            _hemisphereRadius = null;
            _camera = null;
            _localRotAxis = null;
            SelectedAxis = null;
            AngleAmount = null;
        }

        public void BeginDrag(Quaternion rotation, float radius, Vector3 dragStartPos)
        {
            if (_camera == null) throw new InvalidOperationException("Camera was null");

            var ghostPos = RA.Controller.ActiveGhost.transform.position;
            _viewPlane = new Plane(_camera.transform.forward * -1, ghostPos);
            _initialRotation = rotation;
            _hemisphereRadius = radius;
            AngleAmount = null;

            _mouseRay = _camera.ScreenPointToRay(dragStartPos);

            if(RA.Controller.HoldingChangeHeightKey)
            {
                _viewPlane.Value.Raycast(_mouseRay, out var distance);
                _dragStart = SnapToHemisphere(
                    ghostPos,
                    _viewPlane.Value.normal,
                    _hemisphereRadius.Value,
                    _mouseRay.GetPoint(distance));
            }
            else if (SelectedAxis != null && _axisRayPos != null)
            {
                // Set the axis here to avoid axis shifts towards the "poles" of the gizmo axes
                // Free rotation mode is unaffected as the rotation cannot exceed a hemisphere
                _localRotAxis = RA.Controller.IsLocalRotation
                    ? Rotation * GetAxisVector(SelectedAxis.Value)
                    : GetAxisVector(SelectedAxis.Value);
                
                _dragStart = ClosestPointForAxis(
                    ghostPos,
                    _localRotAxis.Value,
                    _hemisphereRadius.Value,
                    _axisRayPos.Value
                    );
            }
        }

        public void UpdateDragPos(Vector3 dragPos)
        {
            if (_viewPlane == null) throw new InvalidOperationException($"{nameof(_viewPlane)} was null");
            if (_hemisphereRadius == null) throw new InvalidOperationException($"{nameof(_hemisphereRadius)} was null");
            if (_initialRotation == null) throw new InvalidOperationException($"{nameof(_initialRotation)} was null");
            if (_dragStart == null) return;

            var ghostPos = RA.Controller.ActiveGhost.transform.position;
            _mouseRay = _camera.ScreenPointToRay(dragPos);

            Quaternion trackballRotation;
            if (SelectedAxis != null && _localRotAxis != null)
            {
                var axisPlane = new Plane(_localRotAxis.Value, ghostPos);
                axisPlane.Raycast(_mouseRay, out var distance);
                var planeIntersect = _mouseRay.GetPoint(distance);
                var closestOnAxis = ClosestPointWithinDistance(ghostPos, _hemisphereRadius.Value, planeIntersect);
                
                if (Vector3.Distance(_dragStart.Value, closestOnAxis) <= float.Epsilon) return;
                var startVec = _dragStart.Value - ghostPos;
                var currentVec = closestOnAxis - ghostPos;
                
                AngleAmount = Vector3.SignedAngle(startVec, currentVec, _localRotAxis.Value);
                if (RA.Controller.AngleSnapActive)
                    AngleAmount = AngleAmount.Value.RoundToMultipleOf(RA.RotationAngle.Value);
                trackballRotation = Quaternion.AngleAxis(AngleAmount.Value, _localRotAxis.Value);
            }
            else
            {
                _viewPlane.Value.Raycast(_mouseRay, out var distance);
                var currentPos = SnapToHemisphere(
                    ghostPos,
                    _viewPlane.Value.normal,
                    _hemisphereRadius.Value,
                    _mouseRay.GetPoint(distance));

                if (Vector3.Distance(_dragStart.Value, currentPos) <= float.Epsilon) return;

                var startVec = _dragStart.Value - ghostPos;
                var currentVec = currentPos - ghostPos;

                var rotationAxis = Vector3.Cross(startVec, currentVec).normalized;
                AngleAmount = Vector3.Angle(startVec, currentVec);
                if (RA.Controller.AngleSnapActive)
                    AngleAmount = AngleAmount.Value.RoundToMultipleOf(RA.RotationAngle.Value);
                trackballRotation = Quaternion.AngleAxis(AngleAmount.Value, rotationAxis);
            }

            Rotation = trackballRotation * _initialRotation.Value;
        }

        public override void OnApplied()
        {
            // TODO: Extract from Update?
        }

        public override void OnReverted()
        {
            Reset();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (RA.Controller.GameState != ParkitectState.Trackball) return;
            if (_dragStart != null) return; // Avoid changing the axis while dragging
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }
            _mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
            if (RA.Controller.HoldingChangeHeightKey)
            {
                SelectedAxis = null;
                return;
            }
            
            if (!Physics.Raycast(_mouseRay, out var hitInfo)) return;

            switch (hitInfo.collider.name)
            {
                case TrackballModeGizmo.TorusXName:
                    SelectedAxis = Axis.X;
                    break;
                case TrackballModeGizmo.TorusYName:
                    SelectedAxis = Axis.Y;
                    break;
                case TrackballModeGizmo.TorusZName:
                    SelectedAxis = Axis.Z;
                    break;
            }

            if (SelectedAxis != null)
            {
                _axisRayPos = _mouseRay.GetPoint(hitInfo.distance);
            }
        }

        private static Vector3 GetAxisVector(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Vector3.right;
                case Axis.Y:
                    return Vector3.up;
                case Axis.Z:
                    return Vector3.forward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        private static Vector3 ClosestPointForAxis(
            Vector3 center,
            Vector3 axisNormal,
            float radius,
            Vector3 position)
        {
            axisNormal.Normalize();

            var offsetFromCenter = position - center;
            var distance = Vector3.Dot(offsetFromCenter, axisNormal);
            var projected = position - distance * axisNormal;

            Debug.Log(
                $"Closest from {position} in plane {projected} at distance {Vector3.Distance(position, projected)}");

            var circlePosition = ClosestPointWithinDistance(center, radius, projected);

            Debug.Log($"Snapped onto circle at {circlePosition}");
            return circlePosition;
        }

        private static Vector3 ClosestPointWithinDistance(
            Vector3 center,
            float distance,
            Vector3 position
        )
        {
            var offset = position - center;
            var limitedOffset = offset.normalized * distance;
            return center + limitedOffset;
        }

        private static Vector3 SnapToHemisphere(
            Vector3 center,
            Vector3 normal,
            float radius,
            Vector3 position)
        {
            var rotationToFlat = Quaternion.FromToRotation(normal, Vector3.up).normalized;
            var rotationFromFlat = Quaternion.FromToRotation(Vector3.up, normal).normalized;

            var offsetFromCenter = position - center;
            var offsetInPlane = rotationToFlat * offsetFromCenter;
            var unitPosition = offsetInPlane / radius;

            var distanceFromCenter = unitPosition.magnitude;
            if (distanceFromCenter > 1)
            {
                var borderSnapPos = new Vector3(unitPosition.x, 0, unitPosition.z).normalized;
                return rotationFromFlat * (borderSnapPos * radius) + center;
            }

            var hemisphereY = (float) Math.Sqrt(1 - Math.Pow(unitPosition.x, 2) - Math.Pow(unitPosition.z, 2));
            var snapPos = new Vector3(unitPosition.x, hemisphereY, unitPosition.z);
            return rotationFromFlat * (snapPos * radius) + center;
        }
    }
}