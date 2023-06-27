using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RotationAnarchyEvolved
{
    public class RotationalGizmoComponent : GizmoComponent
    {
        public override Plane GetPlane()
        {
            return new Plane(transform.forward, transform.position);
        }
    }
}
