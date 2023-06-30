using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TransformAnarchy
{
    public class PositionalGizmoComponent : GizmoComponent
    {

		// Adapted from https://github.com/HiddenMonk/Unity3DRuntimeTransformGizmo/tree/master
		// Basically, planes aren't good for Axes. this fixes that.
		public static Vector3 ClosestPointsOnTwoLines(Vector3 mouseRayPos, Vector3 mouseRayDir, Vector3 axesBegin, Vector3 axesDirection)
		{

			float a = Vector3.Dot(mouseRayDir, mouseRayDir);
			float b = Vector3.Dot(mouseRayDir, axesDirection);
			float e = Vector3.Dot(axesDirection, axesDirection);

			float d = a * e - b * b;

			if (d != 0f)
			{

				Vector3 r = mouseRayPos - axesBegin;
				float c = Vector3.Dot(mouseRayDir, r);
				float f = Vector3.Dot(axesDirection, r);

				float s = (b * f - c * e) / d;
				float t = (a * f - c * b) / d;

				return axesBegin + axesDirection * t;

			}
			else
			{
				return axesBegin + Vector3.Project(mouseRayPos - axesBegin, axesDirection);
			}
		}

        public override Vector3 GetPlaneOffset(Ray ray)
        {
			return ClosestPointsOnTwoLines(ray.origin, ray.direction, transform.position, transform.forward);
		}
    }
}
