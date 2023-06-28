namespace RotationAnarchyEvolved
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class StencilHandler : MonoBehaviour
    {
        public Material BaseMaterial => baseMaterial;
        public Material StencilMaterial => stencilMaterial;

        protected Material baseMaterial;
        protected Material stencilMaterial;
        protected CommandBuffer commandBuffer;
        protected CommandBuffer commandBufferStencil;
        protected HighlightOverlayController.HighlightHandle overlayHandle;
        protected List<Renderer> renderers = new List<Renderer>();

        public void SetColor(Color col)
        {
            _color = col;
            _outlineColor = col * new Color(0.7f, 0.7f, 0.7f, 0.7f);
        }

        private Color _color;
        private Color _outlineColor;

        protected int sId_Color;
        protected int sId_MainTex;

        public void Start()
        {
            sId_Color = Shader.PropertyToID("_Color");
            sId_MainTex = Shader.PropertyToID("_MainTex");

            this.stencilMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlayStencil"));
            this.baseMaterial = new Material(AssetManager.Instance.highlightOverlayMaterial);

            this.baseMaterial.SetFloat("_BaseBrightness", 1f);
            this.baseMaterial.SetFloat("_BaseBrightnessBlink", 1f);
            this.baseMaterial.SetFloat("_BlinkSpeed", 0f);
            this.baseMaterial.SetFloat("_GlowBrightness", 4f);
            // This needs to be 0.0 in order for custom colour textured transparent objects to render the gizmo correctly.
            // Do not ask why. We don't talk about it.
            this.baseMaterial.SetFloat("_Cutoff", 0.0f);
            this.baseMaterial.SetFloat("_RimPower", 0.5f);
            this.baseMaterial.SetFloat("_BlinkStencil", 0.5f);
            this.baseMaterial.SetFloat("_Checkerboard", 0f);

            this.commandBuffer = new CommandBuffer();
            this.commandBuffer.name = gameObject.name;

            this.commandBufferStencil = new CommandBuffer();
            this.commandBufferStencil.name = gameObject.name + ".stencil";

            renderers = GetComponentsInChildren<Renderer>().ToList();
        }

        public void Destroy()
        {
            if (overlayHandle != null)
                overlayHandle.remove();
        }

        public void OnEnabled()
        {
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }

        public void OnDisabled()
        {
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBufferStencil);
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }

        public void Update()
        {
            commandBuffer.Clear();
            commandBufferStencil.Clear();

            foreach (Renderer r in renderers)
            {
                this.commandBufferStencil.SetGlobalColor(sId_Color, _color);
                this.commandBuffer.SetGlobalColor(sId_Color, _outlineColor);
                this.commandBufferStencil.DrawRenderer(r, stencilMaterial);
                this.commandBuffer.DrawRenderer(r, r.material);
                HighlightOverlayControllerUtil.ChangeHighlightColor(r, _outlineColor);
            }
        }
    }
}