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

        public void Start()
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

            Debug.Log($"All done: {overlayHandle}");

        }

        public abstract Plane GetPlane();

        public Vector3 GetPlaneOffset(Ray ray)
        {
            GetPlane().Raycast(ray, out float enter);
            return ray.GetPoint(enter);
        }

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
        }
        public virtual void OnDisable()
        {

            if (overlayHandle != null)
            {
                overlayHandle.remove();
                overlayHandle = null;
            }

            Destroy(stencilMaterial);
            Destroy(baseMaterial);
        }

        public void Render(CommandBuffer commandBuffer, CommandBuffer commandBufferStencil)
        {

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
