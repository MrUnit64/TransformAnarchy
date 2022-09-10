using UnityEngine;

namespace RotationAnarchy
{
    using Internal.Utils.Meshes;

    public class TorusGizmoComponent : AbstractRenderedMeshGizmoComponent
    {
        public TorusMesh Torus { get; private set; }
        public MeshCollider MeshCollider { get; private set; }

        public TorusGizmoComponent(string name = null) : base(name)
        {
            Torus = new TorusMesh();
            meshFilter.mesh = Torus.mesh;
            MeshCollider = gameObject.AddComponent<MeshCollider>();
            if (name != null)
            {
                MeshCollider.name = name;
            }
            MeshCollider.sharedMesh = Torus.mesh;
        }

        public override void Update()
        {
            base.Update();
            Torus.UpdateMesh();
            MeshCollider.sharedMesh = Torus.mesh;
        }
    }
}