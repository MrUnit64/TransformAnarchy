using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace RotationAnarchyEvolved
{
    public abstract class GizmoComponent : MonoBehaviour
    {

        public Color _color;
        public Color _outlineColor;

        public Material BaseMaterial => baseMaterial;
        public Material StencilMaterial => stencilMaterial;

        protected Material baseMaterial;
        protected Material stencilMaterial;
        protected HighlightOverlayController.HighlightHandle overlayHandle;
        protected List<Renderer> renderers;

        protected int sId_Color;
        protected int sId_MainTex;

        public abstract Vector3 GetPlaneOffset(Ray ray);

        public void SetColor(Color col)
        {
            _color = col;
            _outlineColor = col;
            _outlineColor.a = 0.1f;

            if (baseMaterial != null && stencilMaterial != null)
            {
                baseMaterial.color = _color;
                stencilMaterial.color = _outlineColor;
            }

        }
        
        public virtual void OnDestroy()
        {
            if (overlayHandle != null)
                overlayHandle.remove();

            Destroy(stencilMaterial);
            Destroy(baseMaterial);

        }
        public virtual void OnDisable()
        {

            if (overlayHandle != null)
            {
                overlayHandle.remove();
                overlayHandle = null;
            }
        }

        public virtual void OnEnable()
        {
            sId_Color = Shader.PropertyToID("_Color");
            sId_MainTex = Shader.PropertyToID("_MainTex");

            this.stencilMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlayStencil"));
            this.baseMaterial = new Material(Shader.Find("Unlit/Color"));

            renderers = GetComponentsInChildren<Renderer>().ToList();

            foreach (Renderer r in renderers)
            {
                r.material = baseMaterial;
            }

            if (overlayHandle == null && HighlightOverlayController.Instance != null && renderers != null && renderers.Count > 0)
                overlayHandle = HighlightOverlayController.Instance.add(renderers, fixedCustomColor: _color);
        }

        public void Render(CommandBuffer commandBuffer, CommandBuffer commandBufferStencil)
        {

            if (!this.enabled)
            {
                return;
            }

            foreach (Renderer r in renderers)
            {
                commandBufferStencil.SetGlobalColor(sId_Color, _color);
                commandBuffer.SetGlobalColor(sId_Color, _outlineColor);
                commandBufferStencil.DrawRenderer(r, stencilMaterial);
                commandBuffer.DrawRenderer(r, baseMaterial);

                if (overlayHandle != null)
                    HighlightOverlayControllerUtil.ChangeHighlightColor(r, _outlineColor);

            }
        }
    }
}
