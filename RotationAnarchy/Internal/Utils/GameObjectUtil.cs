namespace RotationAnarchy.Internal
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class GameObjectUtil
    {
        public static Bounds ComputeTotalBounds(GameObject gameObject)
        {
            if (gameObject == null)
                return default;

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            void ProcessBounds(MeshRenderer mr)
            {
                var b = mr.bounds;
                min = Vector3.Min(min, b.min);
                max = Vector3.Max(max, b.max);
            }

            ComponentUtil.TraverseComponentForeach<MeshRenderer>(gameObject.transform, ProcessBounds);

            Bounds bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

    }


}