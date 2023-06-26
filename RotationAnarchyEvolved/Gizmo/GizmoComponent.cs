using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class GizmoComponent : MonoBehaviour
{

    public Color _color;
    public Color _outlineColor;

    public Material BaseMaterial => baseMaterial;
    public Material StencilMaterial => stencilMaterial;

    protected Material baseMaterial;
    protected Material stencilMaterial;
    protected CommandBuffer commandBuffer;
    protected CommandBuffer commandBufferStencil;
    protected List<Renderer> renderers = new List<Renderer>();

    protected int sId_Color;
    protected int sId_MainTex;

    public void OnEnable()
    {
        sId_Color = Shader.PropertyToID("_Color");
        sId_MainTex = Shader.PropertyToID("_MainTex");

        this.stencilMaterial = new Material(Shader.Find("Rollercoaster/GhostOverlayStencil"));
        this.baseMaterial = new Material(AssetManager.Instance.highlightOverlayMaterial);

        this.baseMaterial.SetFloat("_BaseBrightness", 1f);
        this.baseMaterial.SetFloat("_BaseBrightnessBlink", 1f);
        this.baseMaterial.SetFloat("_BlinkSpeed", 0f);
        this.baseMaterial.SetFloat("_GlowBrightness", 4f);
        this.baseMaterial.SetFloat("_Cutoff", 0.5f);
        this.baseMaterial.SetFloat("_RimPower", 0.5f);
        this.baseMaterial.SetFloat("_BlinkStencil", 0.5f);
        this.baseMaterial.SetFloat("_Checkerboard", 0f);

        this.commandBuffer = new CommandBuffer();
        this.commandBuffer.name = gameObject.name;

        this.commandBufferStencil = new CommandBuffer();
        this.commandBufferStencil.name = gameObject.name + ".stencil";

        renderers = GetComponentsInChildren<Renderer>().ToList();

        foreach (Renderer r in renderers)
        {
            r.material = baseMaterial;
        }

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
        _outlineColor = col * new Color(0.7f, 0.7f, 0.7f, 0.7f);

        baseMaterial.color = _color;
        stencilMaterial.color = _outlineColor;
    }

    public void OnDisable()
    {
        Destroy(stencilMaterial);
        Destroy(baseMaterial);
    }

    public void Update()
    {
        commandBuffer.Clear();
        commandBufferStencil.Clear();

        foreach (Renderer r in renderers)
        {
            this.commandBufferStencil.SetGlobalColor(sId_Color, _color);
            this.commandBuffer.SetGlobalColor(sId_Color, _color);
            this.commandBufferStencil.DrawRenderer(r, stencilMaterial);
            this.commandBuffer.DrawRenderer(r, baseMaterial);
        }
    }
}
