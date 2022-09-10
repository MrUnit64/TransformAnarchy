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
        public Bounds TargetBounds { get; private set; }
        public float BoundsMax { get; private set; }

        public Axis Axis
        {
            get => _axis;
            set
            {
                if (_axis == value) return;
                _axis = value;
                OnAxisChanged();
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
            if (parent)
            {
                component.transform.SetParent(gameObject.transform, false);
            }
        }

        public virtual void RemoveGizmoComponent(AbstractGizmoComponent component, bool unparent = true)
        {
            gizmoComponents.Remove(component);
            if (unparent)
            {
                component.transform.SetParent(null);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            GameObject target = null;
            if (RA.Controller.ActiveGhost)
            {
                target = RA.Controller.ActiveGhost;
            }
            else if (RA.Controller.SelectedBuildable)
            {
                target = RA.Controller.SelectedBuildable.gameObject;
            }

            if (target != null)
            {
                TargetBounds = GetBoundsFromTarget(target);
            }
            BoundsMax = GetMaxBoundsFromBounds(TargetBounds);

            foreach (var component in gizmoComponents)
            {
                component.Update();
            }
        }

        protected virtual Bounds GetBoundsFromTarget(GameObject target)
        {
            // we are going to do a funny hack here
            var originalRotation = target.transform.rotation;
            target.transform.rotation = Quaternion.identity;
            var bounds = GameObjectUtil.ComputeTotalBounds(target);
            target.transform.rotation = originalRotation;
            return bounds;
        }

        protected virtual float GetMaxBoundsFromBounds(Bounds bounds)
        {
            var xWidth = bounds.max.x - bounds.min.x;
            var zWidth = bounds.max.z - bounds.min.z;
            return Mathf.Max(xWidth, zWidth);
        }

        public virtual void SnapTo(Vector3 position, Quaternion rotation)
        {
            gameObject.transform.position = position;
            gameObject.transform.rotation = RA.Controller.IsLocalRotation ? rotation : Quaternion.identity;
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
            if (!RA.Controller.ActiveGhost) return;
            var tr = RA.Controller.ActiveGhost.transform;
            SnapTo(tr.position, tr.rotation);
        }

        public virtual void SnapToSelectedBuildable()
        {
            if (RA.Controller.SelectedBuildable)
            {
                var tr = RA.Controller.SelectedBuildable.transform;
                SnapTo(tr.position, tr.rotation);
            }
        }
    }
}