namespace RotationAnarchy
{
    using RotationAnarchy.Internal.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class AbstractGizmoComponent
    {
        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject.transform;
        public TransformInterpolator transformInterpolator { get; private set; }
        public IReadOnlyCollection<AbstractGizmoComponent> Children { get; private set; }

        private List<AbstractGizmoComponent> _children = new List<AbstractGizmoComponent>();
        
        public AbstractGizmoComponent(string name = null)
        {
            gameObject = new GameObject(name ?? "GizmoComponent");
            transformInterpolator = new TransformInterpolator(gameObject.transform);
            Children = _children;
        }

        public virtual void Update()
        {
            transformInterpolator.Update();
        }

        public void AttachComponent(AbstractGizmoComponent component)
        {
            _children.Add(component);
            component.transform.SetParent(transform, false);
        }

        public void DetachComponent(AbstractGizmoComponent component)
        {
            _children.Remove(component);
            component.transform.SetParent(null);
        }
    }
}