namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public enum GizmoAxis
    {
        X,
        Y,
        Z
    }

    public class GizmoColors
    {
        public GizmoAxisColorBlock X, Y, Z;

        public GizmoAxisColorBlock GetForAxis(GizmoAxis axis)
        {
            switch (axis)
            {
                case GizmoAxis.X: return X;
                case GizmoAxis.Y: return Y;
                case GizmoAxis.Z: return Z;
                default: return X;
            }
        }
    }

    public class GizmoOffsets
    {
        public GizmoOffsetsBlock X, Y, Z;

        public GizmoOffsetsBlock GetForAxis(GizmoAxis axis)
        {
            switch (axis)
            {
                case GizmoAxis.X: return X;
                case GizmoAxis.Y: return Y;
                case GizmoAxis.Z: return Z;
                default: return X;
            }
        }
    }

    public struct GizmoAxisColorBlock
    {
        public Color color;
        public Color outlineColor;
    }

    public struct GizmoOffsetsBlock
    {
        public Vector3 localPositionOffset;
        public Quaternion localRotationOffset;
    }

    public abstract class MeshGizmo : GizmoBase
    {
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
        protected Texture2D colorTex;
        protected List<Renderer> renderers = new List<Renderer>();
        protected HighlightOverlayController.HighlightHandle overlayHandle;

        protected int sId_Color;
        protected int sId_MainTex;

        private GizmoAxisColorBlock targetColorBlock;
        private GizmoOffsetsBlock targetOffsetsBlock;

        private bool initializedParams;

        private Color _currentColor;
        private Color _currentOutlineColor;
        private Vector3 _currentLocalPositionOffset;
        private Quaternion _currentLocalRotationOffset;

        public MeshGizmo(string cmdBufferId, GizmoAxis axis) : base(axis)
        {
            sId_Color = Shader.PropertyToID("_Color");
            sId_MainTex = Shader.PropertyToID("_MainTex");

            this.colorTex = new Texture2D(1, 1);

            //this.stencilMaterial = new Material(Shader.Find("Custom/SimpleBlit"));
            this.stencilMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlayStencil"));
            this.baseMaterial = new Material(AssetManager.Instance.highlightOverlayMaterial);

            //this.baseMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlay"));

            this.stencilMaterial.SetTexture(sId_MainTex, colorTex);
            this.stencilMaterial.SetVector("_AreaParams", new Vector4(1, 1, 0, 0));
            //this.stencilMaterial.SetFloat("_Cutoff", 0.5f);

            this.baseMaterial.SetFloat("_BaseBrightness", 1f);
            this.baseMaterial.SetFloat("_BaseBrightnessBlink", 1f);
            this.baseMaterial.SetFloat("_BlinkSpeed", 0f);
            this.baseMaterial.SetFloat("_GlowBrightness", 4f);
            this.baseMaterial.SetFloat("_Cutoff", 0.8f);
            this.baseMaterial.SetFloat("_RimPower", 0.5f);
            this.baseMaterial.SetFloat("_BlinkStencil", 0.5f);
            this.baseMaterial.SetFloat("_Checkerboard", 0f);

            this.commandBuffer = new CommandBuffer();
            this.commandBuffer.name = cmdBufferId;

            this.commandBufferStencil = new CommandBuffer();
            this.commandBufferStencil.name = cmdBufferId + ".stencil";

            Construct();
        }

        protected virtual void Construct()
        {
            mesh = new Mesh();
            mesh.MarkDynamic();

            meshFilter = GameObject.AddComponent<MeshFilter>();
            meshRenderer = GameObject.AddComponent<MeshRenderer>();
            meshCollider = GameObject.AddComponent<MeshCollider>();

            renderers.Add(meshRenderer);

            meshFilter.mesh = mesh;

            material = baseMaterial;

            UpdateMesh();
        }

        public override void Destroy()
        {
            base.Destroy();

            GameObject.Destroy(mesh);
            GameObject.Destroy(material);
            mesh = null;
            meshFilter = null;
            meshRenderer = null;
            meshCollider = null;
            material = null;
        }

        protected override void OnActiveChanged()
        {
            base.OnActiveChanged();

            if (Active)
            {
                Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
                Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);

                //this.commandBufferStencil.SetGlobalTexture(this.mainTexturePropertyID, tex);
                //this.commandBufferStencil.SetGlobalTexture(sId_MainTex, colorTex);
                this.commandBufferStencil.SetGlobalColor(sId_Color, _currentColor);
                this.commandBufferStencil.DrawRenderer(meshRenderer, stencilMaterial);
                //this.commandBuffer.SetGlobalColor(sId_Color, color);
                this.commandBuffer.DrawRenderer(meshRenderer, material);

                if (HighlightOverlayController.Instance != null)
                    overlayHandle = HighlightOverlayController.Instance.add(renderers, fixedCustomColor: _currentOutlineColor);
            }
            else
            {
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
                commandBuffer.Clear();
                commandBufferStencil.Clear();

                if (overlayHandle != null)
                    overlayHandle.remove();
            }
        }

        public virtual void UpdateMesh()
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!Active)
                return;

            if(initializedParams)
            {
                float delta = Time.unscaledDeltaTime;
                _currentColor = Color.Lerp(_currentColor, targetColorBlock.color, RA.GizmoParamLerp * delta);
                _currentOutlineColor = Color.Lerp(_currentOutlineColor, targetColorBlock.outlineColor, RA.GizmoParamLerp * delta);
                _currentLocalPositionOffset = Vector3.Lerp(_currentLocalPositionOffset, targetOffsetsBlock.localPositionOffset, RA.GizmoParamLerp * delta);
                _currentLocalRotationOffset = Quaternion.Lerp(_currentLocalRotationOffset, targetOffsetsBlock.localRotationOffset, RA.GizmoParamLerp * delta);
            }
            else
            {
                _currentColor = targetColorBlock.color;
                _currentOutlineColor = targetColorBlock.outlineColor;
                _currentLocalPositionOffset = targetOffsetsBlock.localPositionOffset;
                _currentLocalRotationOffset = targetOffsetsBlock.localRotationOffset;
                initializedParams = true;

                RA.Instance.LOG("initializedParams");
            }

            this.baseMaterial.SetColor(sId_Color, _currentColor);
        }

        protected override void OnAxisChanged()
        {
            base.OnAxisChanged();

            targetColorBlock = RA.GizmoColors.GetForAxis(Axis);
            targetOffsetsBlock = RA.GizmoOffsets.GetForAxis(Axis);
        }

        protected override GizmoOffsetsBlock GetOffsets()
        {
            return RA.GizmoOffsets.GetForAxis(Axis);
        }

        /// <summary>
        /// Use distance from camera to calculate how "fat" the tube should be
        /// </summary>
        /// <returns></returns>
        protected virtual float ComputeGizmoWidth(Vector3 point)
        {
            float minRadius = 0.004f;
            float maxRadius = 0.125f;
            float maxDistance = 500;
            float minDistance = 10;

            var cam = Camera.main;

            if (!cam)
                return minRadius;

            float d = Vector3.Distance(point, cam.transform.position);
            float l = Mathf.InverseLerp(minDistance, maxDistance, d);
            float width = Mathf.Lerp(minRadius, maxRadius, l);

            if (RA.Controller.GameState == ParkitectState.Placement)
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