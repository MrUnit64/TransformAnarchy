namespace RotationAnarchy
{
    using RotationAnarchy.Internal.Utils;
    using UnityEngine;

    public abstract class AbstractGizmoComponent
    {
        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject.transform;
        public TransformInterpolator transformInterpolator { get; private set; }

        public AbstractGizmoComponent(string name = null)
        {
            gameObject = new GameObject(name ?? "GizmoComponent");
            transformInterpolator = new TransformInterpolator(gameObject.transform);
        }

        public virtual void Update()
        {
            transformInterpolator.Update();
        }
    }
}