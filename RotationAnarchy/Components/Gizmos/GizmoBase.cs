namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
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

        public void SetActive(bool state)
        {
            GameObject.SetActive(state);
        }

        public virtual void UpdateMesh()
        {

        }

        public virtual void SnapToTransform(Transform transform)
        {

        }

        public virtual void SnapToBuildable(BuildableObject buildable)
        {

        }

        public virtual void SnapToBuilder(Builder builder)
        {

        }

        /// <summary>
        /// Use distance from camera to calculate how "fat" the tube should be
        /// </summary>
        /// <returns></returns>
        protected virtual float ComputeGizmoWidth(Vector3 point)
        {
            float minRadius = 0.05f;
            float maxRadius = 0.5f;
            float maxDistance = 200;
            float minDistance = 10;

            var cam = Camera.main;

            if (!cam)
                return minRadius;

            float d = Vector3.Distance(point, cam.transform.position);
            float l = Mathf.InverseLerp(minDistance, maxDistance, d);
            return Mathf.Lerp(minRadius, maxRadius, l);
        }

    }
}