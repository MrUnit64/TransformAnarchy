using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionalGizmoComponent : GizmoComponent
{
    public override Plane GetPlane()
    {
        return new Plane(transform.up, transform.position);
    }
}
