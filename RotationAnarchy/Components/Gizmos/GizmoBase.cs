namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class GizmoBase
    {
        public bool Active => GameObject.activeInHierarchy;
        public GameObject GameObject { get; private set; }
        
        public GizmoAxis Axis
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

        private GizmoAxis _axis;

        public GizmoBase(GizmoAxis axis)
        {
            this._axis = axis;
            GameObject = new GameObject("Gizmo");
            OnAxisChanged();
        }

        protected abstract GizmoOffsetsBlock GetOffsets();

        public void SetActive(bool state)
        {
            if (GameObject.activeInHierarchy != state)
            {
                GameObject.SetActive(state);
                OnActiveChanged();
            }
        }

        protected virtual void OnActiveChanged()
        {
        }

        protected virtual void OnAxisChanged()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void Destroy()
        {
            GameObject.Destroy(GameObject);
        }

        public virtual void SnapTo(Vector3 position, Quaternion rotation)
        {
            var data = GetOffsets();

            if(RA.Controller.IsLocalRotation)
            {
                Vector3 dir = (rotation * data.localPositionOffset) * data.localPositionOffset.magnitude;
                GameObject.transform.position = position + dir;
                GameObject.transform.rotation = rotation * data.localRotationOffset;
            }
            else
            {
                GameObject.transform.position = position;
                GameObject.transform.rotation = data.localRotationOffset;
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