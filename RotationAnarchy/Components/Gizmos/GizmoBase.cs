namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class GizmoBase
    {
        public GameObject GameObject { get; private set; }
        public Vector3 LocalPositionOffset { get; private set; }
        public Quaternion LocalRotationOffset { get; private set; }

        protected float localPosOffsetMagnitude;

        public Material material
        {
            get => meshRenderer.material;
            set => meshRenderer.material = value;
        }

        protected Mesh mesh;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;

        public GizmoBase(Vector3 localPositionOffset, Quaternion localRotationOffset)
        {
            this.LocalPositionOffset = localPositionOffset;
            this.localPosOffsetMagnitude = localPositionOffset.magnitude;
            this.LocalRotationOffset = localRotationOffset;
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

        public virtual void SnapTo(Vector3 position, Quaternion rotation)
        {
            Vector3 dir = (rotation * LocalPositionOffset) * localPosOffsetMagnitude;
            GameObject.transform.position = position + dir;
            GameObject.transform.rotation = rotation * LocalRotationOffset;
        }

        public virtual void SnapToTransform(Transform transform)
        {
            SnapTo(transform.position, transform.rotation);
        }

        public virtual void SnapToBuildable(BuildableObject buildable)
        {
            SnapToTransform(buildable.transform);
        }

        public virtual void SnapToActiveBuilder()
        {
            var tr = RA.Controller.ActiveGhost.transform;
            SnapTo(tr.position, tr.rotation);
        }

        /// <summary>
        /// Use distance from camera to calculate how "fat" the tube should be
        /// </summary>
        /// <returns></returns>
        protected virtual float ComputeGizmoWidth(Vector3 point)
        {
            float minRadius = 0.025f;
            float maxRadius = 0.35f;
            float maxDistance = 500;
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