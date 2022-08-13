namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public class GizmoColors
    {
        public GizmoAxisColorBlock X, Y, Z;

        public GizmoAxisColorBlock GetForAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.X: return X;
                case Axis.Y: return Y;
                case Axis.Z: return Z;
                default: return X;
            }
        }
    }

    public class GizmoOffsets
    {
        public GizmoOffsetsBlock X, Y, Z;

        public GizmoOffsetsBlock GetForAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.X: return X;
                case Axis.Y: return Y;
                case Axis.Z: return Z;
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
        public Vector3 positionOffset;
        public Quaternion rotationOffset;
    }

    public abstract class AbstractRenderedMeshGizmo : AbstractGizmo
    {
        public Material BaseMaterial => baseMaterial;
        public Material StencilMaterial => stencilMaterial;

        protected Material baseMaterial;
        protected Material stencilMaterial;
        protected CommandBuffer commandBuffer;
        protected CommandBuffer commandBufferStencil;
        protected HighlightOverlayController.HighlightHandle overlayHandle;
        protected List<Renderer> renderers = new List<Renderer>();

        protected int sId_Color;
        protected int sId_MainTex;

        /// <summary>
        /// Do not do anything in the constructor
        /// </summary>
        /// <param name="name"></param>
        public AbstractRenderedMeshGizmo(string name = null) : base(name) { }

        protected override void OnConstruct()
        {
            base.OnConstruct();
            sId_Color = Shader.PropertyToID("_Color");
            sId_MainTex = Shader.PropertyToID("_MainTex");

            this.stencilMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlayStencil"));
            this.baseMaterial = new Material(AssetManager.Instance.highlightOverlayMaterial);
            this.stencilMaterial.SetFloat("_Cutoff", 0.5f);

            this.baseMaterial.SetFloat("_BaseBrightness", 1f);
            this.baseMaterial.SetFloat("_BaseBrightnessBlink", 1f);
            this.baseMaterial.SetFloat("_BlinkSpeed", 0f);
            this.baseMaterial.SetFloat("_GlowBrightness", 4f);
            this.baseMaterial.SetFloat("_Cutoff", 0.8f);
            this.baseMaterial.SetFloat("_RimPower", 0.5f);
            this.baseMaterial.SetFloat("_BlinkStencil", 0.5f);
            this.baseMaterial.SetFloat("_Checkerboard", 0f);

            this.commandBuffer = new CommandBuffer();
            this.commandBuffer.name = gameObject.name;

            this.commandBufferStencil = new CommandBuffer();
            this.commandBufferStencil.name = gameObject.name + ".stencil";
        }

        public override void AddGizmoComponent(AbstractGizmoComponent component, bool parent = true)
        {
            base.AddGizmoComponent(component);
            if(component is AbstractRenderedMeshGizmoComponent rendered)
            {
                rendered.meshRenderer.material = BaseMaterial;
                renderers.Add(rendered.meshRenderer);
            }
        }

        public override void RemoveGizmoComponent(AbstractGizmoComponent component, bool unparent = true)
        {
            base.RemoveGizmoComponent(component, unparent);
            if (component is AbstractRenderedMeshGizmoComponent rendered)
            {
                renderers.Remove(rendered.meshRenderer);
            }
        }

        public override void Destroy()
        {
            if (overlayHandle != null)
                overlayHandle.remove();

            base.Destroy();
        }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);

            if (overlayHandle == null && HighlightOverlayController.Instance != null)
                overlayHandle = HighlightOverlayController.Instance.add(renderers, fixedCustomColor: Color.white);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);

            overlayHandle.remove();
            overlayHandle = null;
        }

        public override void Update()
        {
            base.Update();
            OnRender();
        }

        protected virtual void OnRender()
        {
            if (!Active)
                return;

            commandBuffer.Clear();
            commandBufferStencil.Clear();

            foreach (var component in gizmoComponents)
            {
                if (component is AbstractRenderedMeshGizmoComponent rendered)
                {
                    this.commandBufferStencil.SetGlobalColor(sId_Color, rendered.colorInterpolator.CurrentColor);
                    this.commandBuffer.SetGlobalColor(sId_Color, rendered.colorInterpolator.CurrentOutlineColor);
                    this.commandBufferStencil.DrawRenderer(rendered.meshRenderer, stencilMaterial);
                    this.commandBuffer.DrawRenderer(rendered.meshRenderer, rendered.meshRenderer.material);
                    HighlightOverlayControllerUtil.ChangeHighlightColor(rendered.meshRenderer, rendered.colorInterpolator.CurrentOutlineColor);
                }
            }
        }

        /// <summary>
        /// Use distance from camera to calculate how "fat" the tube should be
        /// </summary>
        /// <returns></returns>
        protected virtual float ComputeGizmoWidth(Vector3 point)
        {
            //float minRadius = 0.012f;
            //float maxRadius = 0.90f;
            if (!Camera.main)
                return RA.GizmoWidthMin.Value;

            float minZoom = 75f;
            float maxZoom = 500f;
            float zoom = Mathf.Clamp(RA.Controller.CurrentZoom, minZoom, maxZoom);
            float frac = Mathf.InverseLerp(minZoom, maxZoom, zoom);

            return Mathf.Lerp(RA.GizmoWidthMin.Value, RA.GizmoWidthMax.Value, frac);

            /*if (RA.Controller.GameState == ParkitectState.Placement)
            {
                return width * RA.PlacementGizmoWidth.Value;
            }
            else
            {
                return width * RA.GizmoWidth.Value;
            }*/
        }
    }
}