namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class GizmoBase
    {
        public GameObject GameObject { get; private set; }
        
        protected Mesh mesh;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;
        protected Material material;

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

            //material = new Material(Shader.Find())
        }
    }
}