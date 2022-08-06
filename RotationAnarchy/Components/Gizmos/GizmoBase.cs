namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class GizmoBase
    {
        public GameObject GameObject { get; private set; }
        public Material material
        {
            get => meshRenderer.material;
            set => meshRenderer.material = value;
        }

        protected Mesh mesh;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;

        public GizmoBase()
        {
            Construct();
        }

        protected virtual void Construct()
        {
            GameObject = new GameObject("Gizmo");
            mesh = new Mesh();
            mesh.MarkDynamic();

            meshFilter = GameObject.AddComponent<MeshFilter>();
            meshRenderer = GameObject.AddComponent<MeshRenderer>();
            meshCollider = GameObject.AddComponent<MeshCollider>();

            meshFilter.mesh = mesh;

            material = AssetManager.Instance.diffuseMaterial;

            UpdateMesh();
        }

        public virtual void Destroy()
        {
            GameObject.Destroy(mesh);
            GameObject.Destroy(material);
            GameObject.Destroy(GameObject);
            mesh = null;
            meshFilter = null;
            meshRenderer = null;
            meshCollider = null;
            material = null;
        }

        public virtual void UpdateMesh()
        {

        }
    }
}