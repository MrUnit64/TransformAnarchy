namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ColorInterpolator
    {
        public Color CurrentColor { get; private set; } = new Color(1, 1, 1, 1);
        public Color CurrentOutlineColor { get; private set; } = new Color(1, 1, 1, 1);

        public Color TargetColor { get; set; } = new Color(1, 1, 1, 1);
        public Color TargetOutlineColor { get; set; } = new Color(1, 1, 1, 1);

        public bool Interpolate { get; set; } = true;
        public float InterpolationSpeed { get; set; } = RA.GizmoParamLerp;

        public ColorInterpolator()
        {
            CurrentColor = CurrentOutlineColor = Color.white;
        }

        public void Update()
        {
            if (Interpolate)
            {
                float delta = Time.unscaledDeltaTime;
                CurrentColor = Color.Lerp(CurrentColor, TargetColor, InterpolationSpeed * delta);
                CurrentOutlineColor = Color.Lerp(CurrentOutlineColor, TargetOutlineColor, InterpolationSpeed * delta);
            }
            else
            {
                CurrentColor = TargetColor;
                CurrentOutlineColor = TargetOutlineColor;
            }
        }
    }


}