namespace RotationAnarchy
{
    using UnityEngine;

    public class AbstractRenderedMeshGizmoComponent : AbstractGizmoComponent
    {
        public MeshFilter meshFilter { get; private set; }
        public MeshRenderer meshRenderer { get; private set; }
        public ColorInterpolator colorInterpolator { get; private set; }

        private MaterialPropertyBlock materialPropertyBlock;
        protected int sId_Color;

        public AbstractRenderedMeshGizmoComponent(string name =  null) : base(name)
        {
            this.meshFilter = gameObject.AddComponent<MeshFilter>();
            this.meshRenderer = gameObject.AddComponent<MeshRenderer>();
            this.colorInterpolator = new ColorInterpolator();
            this.materialPropertyBlock = new MaterialPropertyBlock();
            this.sId_Color = Shader.PropertyToID("_Color");
        }

        public override void Update()
        {
            base.Update();
            colorInterpolator.Update();

            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor(sId_Color, colorInterpolator.CurrentColor);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}