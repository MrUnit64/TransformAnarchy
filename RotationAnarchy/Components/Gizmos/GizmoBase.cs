namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

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
        protected Material baseMaterial;
        public Material stencilMaterial;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;
        protected CommandBuffer commandBuffer;
        protected CommandBuffer commandBufferStencil;
        protected Color color;
        protected Texture2D colorTex;

        protected int sId_Color;
        protected int sId_MainTex;

        public GizmoBase(string cmdBufferId, Vector3 localPositionOffset, Quaternion localRotationOffset, Color color)
        {
            sId_Color = Shader.PropertyToID("_Color");
            sId_MainTex = Shader.PropertyToID("_MainTex");


            this.color = color;
            this.colorTex = new Texture2D(1, 1);

            this.stencilMaterial = new Material(Shader.Find("Custom/SimpleBlit"));
            this.baseMaterial = new Material(AssetManager.Instance.highlightOverlayMaterial);

            //this.stencilMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlayStencil"));
            //this.baseMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlay"));

            this.stencilMaterial.SetTexture("_MainTex", colorTex);
            this.stencilMaterial.SetVector("_AreaParams", new Vector4(1, 1, 0, 0));
            //this.stencilMaterial.SetFloat("_Cutoff", 0.5f);

            this.baseMaterial.SetColor("_Color", color);
            this.baseMaterial.SetFloat("_BaseBrightness", 1f);
            this.baseMaterial.SetFloat("_BaseBrightnessBlink", 1f);
            this.baseMaterial.SetFloat("_BlinkSpeed", 0f);
            this.baseMaterial.SetFloat("_GlowBrightness", 4f);
            this.baseMaterial.SetFloat("_Cutoff", 0.5f);
            this.baseMaterial.SetFloat("_RimPower", 0.5f);
            this.baseMaterial.SetFloat("_BlinkStencil", 0.5f);
            this.baseMaterial.SetFloat("_Checkerboard", 0f);

            this.commandBuffer = new CommandBuffer();
            this.commandBuffer.name = cmdBufferId;

            this.commandBufferStencil = new CommandBuffer();
            this.commandBufferStencil.name = cmdBufferId + ".stencil";

            this.LocalPositionOffset = localPositionOffset;
            this.localPosOffsetMagnitude = localPositionOffset.magnitude;
            this.LocalRotationOffset = localRotationOffset;

            SetColor(color);
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

            material = baseMaterial;

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
            if (GameObject.activeInHierarchy != state)
            {
                GameObject.SetActive(state);

                if (state)
                {
                    Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
                    Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);

                    //this.commandBufferStencil.SetGlobalTexture(this.mainTexturePropertyID, tex);
                    this.commandBufferStencil.SetGlobalTexture(sId_MainTex, colorTex);
                    this.commandBufferStencil.SetGlobalColor(sId_Color, color);
                    this.commandBufferStencil.DrawRenderer(meshRenderer, stencilMaterial);
                    //this.commandBuffer.SetGlobalColor(sId_Color, color);
                    this.commandBuffer.DrawRenderer(meshRenderer, material);
                }
                else
                {
                    Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
                    Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
                    commandBuffer.Clear();
                    commandBufferStencil.Clear();
                }
            }
        }

        public virtual void UpdateMesh()
        {
        }

        public virtual void SetColor(Color color)
        {
            this.color = color;
            colorTex.SetPixel(0, 0, color);
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
            float minRadius = 0.002f;
            float maxRadius = 0.125f;
            float maxDistance = 500;
            float minDistance = 10;

            var cam = Camera.main;

            if (!cam)
                return minRadius;

            float d = Vector3.Distance(point, cam.transform.position);
            float l = Mathf.InverseLerp(minDistance, maxDistance, d);
            float width = Mathf.Lerp(minRadius, maxRadius, l);

            if(RA.Controller.GameState == ParkitectState.Placement)
            {
                return width * RA.PlacementGizmoWidth.Value;
            }
            else
            {
                return width * RA.GizmoWidth.Value;
            }
        }
    }
}