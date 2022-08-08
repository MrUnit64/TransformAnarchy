namespace RotationAnarchy.Internal.Utils
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class TransformInterpolator
    {
        public Transform Transform { get; private set; }

        public Vector3 CurrentPositionOffset { get; private set; }
        public Quaternion CurrentRotationOffset { get; private set; }

        public Vector3 TargetPositionOffset { get; set; }
        public Quaternion TargetRotationOffset { get; set; }

        public bool Interpolate { get; set; } = true;
        public float InterpolationSpeed { get; set; } = RA.GizmoParamLerp;

        public TransformInterpolator(Transform transform)
        {
            this.Transform = transform;
        }

        public void Update()
        {
            if (Interpolate)
            {
                float delta = Time.unscaledDeltaTime;
                CurrentPositionOffset = Vector3.Lerp(CurrentPositionOffset, TargetPositionOffset, InterpolationSpeed * delta);
                CurrentRotationOffset = Quaternion.Lerp(CurrentRotationOffset, TargetRotationOffset, InterpolationSpeed * delta);
            }
            else
            {
                CurrentPositionOffset = TargetPositionOffset;
                CurrentRotationOffset = TargetRotationOffset;
            }

            Transform.localPosition = CurrentPositionOffset;
            Transform.localRotation = CurrentRotationOffset;
        }
    }


}