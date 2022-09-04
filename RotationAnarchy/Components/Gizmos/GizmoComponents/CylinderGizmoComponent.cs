namespace RotationAnarchy
{
    using RotationAnarchy.Internal.Utils.Meshes;
    using System;
    using System.Collections.Generic;

    public class CylinderGizmoComponent : AbstractRenderedMeshGizmoComponent
    {
        public CylinderMesh Cylinder { get; private set; }

        public CylinderGizmoComponent(string name = null) : base(name)
        {
            Cylinder = new CylinderMesh(0.2f, 0.2f, 1f, 8, 1);
            meshFilter.mesh = Cylinder.mesh;
        }

        public override void Update()
        {
            base.Update();
            Cylinder.UpdateMesh();
        }
    }
}