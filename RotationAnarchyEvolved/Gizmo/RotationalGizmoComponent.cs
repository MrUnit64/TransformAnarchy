using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalGizmoComponent : GizmoComponent
{
    public override Plane GetPlane()
    {
        return new Plane(transform.forward, transform.position);
    }
}
