using System;
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

        public Quaternion Rotation { get; private set; }

        public void Reset()
        {
            _dragStart = null;
            _initialRotation = null;
            _hemisphereRadius = null;
            _camera = null;
        }

        public void Initialize(Quaternion rotation, float radius, Vector3 ghostPos, Vector3 dragStartPos)
        {
            _camera = Camera.main;
            if (_camera == null) throw new InvalidOperationException("Camera was null");

            _viewPlane = new Plane(_camera.transform.forward * -1, ghostPos);
            _initialRotation = rotation;
            _hemisphereRadius = radius;

            var mouseRay = _camera.ScreenPointToRay(dragStartPos);
            _viewPlane.Value.Raycast(_mouseRay, out var distance);
            _dragStart = SnapToHemisphere(
                ghostPos,
                _viewPlane.Value.normal,
                _hemisphereRadius.Value,
                mouseRay.GetPoint(distance));
        }

        public void UpdateState(Vector3 ghostPos, Vector3 dragPos)
        {

            if (_viewPlane == null) throw new InvalidOperationException($"{nameof(_viewPlane)} was null");
            if (_hemisphereRadius == null) throw new InvalidOperationException($"{nameof(_hemisphereRadius)} was null");
            if (_dragStart == null) throw new InvalidOperationException($"{nameof(_dragStart)} was null");
            if (_initialRotation == null) throw new InvalidOperationException($"{nameof(_initialRotation)} was null");

            _mouseRay = _camera.ScreenPointToRay(dragPos);
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
            var rotationAngle = Vector3.Angle(startVec, currentVec);
            var trackballRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);

            Rotation = trackballRotation * _initialRotation.Value;
        }

        public override void OnApplied()
        {
            // TODO: Extract from Update?
        }

        public override void OnReverted()
        {
            // TODO: Extract from Update?
        }

        private static Vector3 SnapToHemisphere(Vector3 center, Vector3 normal, float radius, Vector3 position)
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