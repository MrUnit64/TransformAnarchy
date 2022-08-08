namespace RotationAnarchy
{
    using RotationAnarchy.Internal.Utils.Meshes;
    using System;
    using System.Collections.Generic;

    public class TorusGizmoComponent : AbstractRenderedMeshGizmoComponent
    {
        public TorusMesh Torus { get; private set; }

        public TorusGizmoComponent(string name = null) : base(name)
        {
            Torus = new TorusMesh();
            meshFilter.mesh = Torus.mesh;
        }

        public override void Update()
        {
            base.Update();
            Torus.UpdateMesh();
        }
    }
}