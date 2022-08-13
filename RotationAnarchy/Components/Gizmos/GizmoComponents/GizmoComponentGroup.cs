namespace RotationAnarchy
{
    using RotationAnarchy.Internal.Utils.Meshes;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class GizmoComponentGroup : AbstractGizmoComponent
    {
        public ReadOnlyCollection<AbstractGizmoComponent> GizmoComponents { get; private set; }
        
        private List<AbstractGizmoComponent> _gizmoComponents = new List<AbstractGizmoComponent>();

        public GizmoComponentGroup(string name = null) : base(name)
        {
            GizmoComponents = new ReadOnlyCollection<AbstractGizmoComponent>(_gizmoComponents);
        }

        public void AttachComponent(AbstractGizmoComponent component)
        {
            _gizmoComponents.Add(component);
            component.transform.SetParent(transform, false);
        }

        public void DetachComponent(AbstractGizmoComponent component)
        {
            _gizmoComponents.Remove(component);
            component.transform.SetParent(null);
        }
    }
}