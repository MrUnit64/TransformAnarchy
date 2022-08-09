namespace RotationAnarchy
{
    using RotationAnarchy.Internal.Utils.Meshes;
    using System;
    using System.Collections.Generic;

    public class ArrowGizmoComponent : AbstractRenderedMeshGizmoComponent
    {
        public CylinderMesh Arrow { get; private set; }

        public ArrowGizmoComponent(string name = null) : base(name)
        {
            Arrow = new CylinderMesh(0.2f, 0, 0.5f, 8, 1);
            meshFilter.mesh = Arrow.mesh;
        }

        public override void Update()
        {
            base.Update();
            Arrow.UpdateMesh();
        }
    }
}