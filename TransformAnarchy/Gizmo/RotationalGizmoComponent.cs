using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RotationAnarchyEvolved
{
    public class RotationalGizmoComponent : GizmoComponent
    {
        public override Vector3 GetPlaneOffset(Ray ray)
        {
            (new Plane(transform.forward, transform.position)).Raycast(ray, out float enter);
            return ray.GetPoint(enter);
        }
    }
}
