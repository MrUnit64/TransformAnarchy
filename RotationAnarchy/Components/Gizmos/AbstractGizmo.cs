namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Starts inactive
    /// </summary>
    public abstract class AbstractGizmo : GameObjectWrapper
    {
        public Bounds GhostBounds { get; private set; }

        public Axis Axis
        {
            get => _axis;
            set
            {
                if (_axis != value)
                {
                    _axis = value;
                    OnAxisChanged();
                }
            }
        }

        private Axis _axis;
        protected List<AbstractGizmoComponent> gizmoComponents = new List<AbstractGizmoComponent>();

        /// <summary>
        /// Do not do anything in the constructor
        /// </summary>
        /// <param name="name"></param>
        public AbstractGizmo(string name = null) : base(name) { }

        protected virtual void OnAxisChanged() { }

        public virtual void AddGizmoComponent(AbstractGizmoComponent component, bool parent = true)
        {
            gizmoComponents.Add(component);
            if(parent)
            {
                component.transform.SetParent(gameObject.transform, false);
            }
        }

        public virtual void RemoveGizmoComponent(AbstractGizmoComponent component, bool unparent = true)
        {
            gizmoComponents.Remove(component);
            if(unparent)
            {
                component.transform.SetParent(null);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (RA.Controller.ActiveGhost)
            {
                var ghost = RA.Controller.ActiveGhost;
                // we are going to do a funny hack here
                var originalRotation = ghost.transform.rotation;
                ghost.transform.rotation = Quaternion.identity;
                GhostBounds = GameObjectUtil.ComputeTotalBounds(ghost);
                ghost.transform.rotation = originalRotation;
            }

            foreach (var component in gizmoComponents)
            {
                component.Update();
            }
        }

        public virtual void SnapTo(Vector3 position, Quaternion rotation)
        {
            gameObject.transform.position = position;
            if(RA.Controller.IsLocalRotation)
            {
                gameObject.transform.rotation = rotation;
            }
        }

        public virtual void SnapToTransform(Transform transform)
        {
            SnapTo(transform.position, transform.rotation);
        }

        public virtual void SnapToBuildable(BuildableObject buildable)
        {
            SnapToTransform(buildable.transform);
        }

        public virtual void SnapToActiveGhost()
        {
            if(RA.Controller.ActiveGhost)
            {
                var tr = RA.Controller.ActiveGhost.transform;
                SnapTo(tr.position, tr.rotation);
            }
        }
    }
}