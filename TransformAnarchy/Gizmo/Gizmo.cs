using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace TransformAnarchy
{

    public enum ToolSpace
    {
        LOCAL,
        GLOBAL
    };

    public abstract class Gizmo<T> : MonoBehaviour where T : GizmoComponent
    {

        protected T XComponent;
        protected T YComponent;
        protected T ZComponent;

        public enum Axis
        {
            NONE,
            X,
            Y,
            Z
        };

        protected T _currentlyDragging;
        protected Camera _cachedMaincam;

        [SerializeField]
        protected ToolSpace _rotationMode;

        public ToolSpace CurrentRotationMode
        {
            get
            {
                return _rotationMode;
            }
            set
            {
                if (_rotationMode != value)
                {
                    _rotationMode = value;
                    UpdateGizmoTransforms();
                }
            }
        }

        // Using Layer 28, because 31 was used for some other stuff apparently
        public const int LAYER = 28;
        public const int LAYER_MASK = 1 << LAYER;

        protected DragInformation _lastInfo;

        // Use to store info about drag events on the gizmo
        public readonly struct DragInformation
        {
            public readonly Vector3 StartDragPosition;
            public readonly Vector3 CurrentDragPosition;
            public readonly Vector3 DragDelta;
            public readonly Axis ModifyAxis;

            public override string ToString()
            {
                return $"Drag [Start: {StartDragPosition}, Current: {CurrentDragPosition}, Change: {DragDelta} on Axis: {ModifyAxis.ToString()}]";
            }

            public DragInformation(Vector3 startingPosition, Axis modifyAxis)
            {
                StartDragPosition = startingPosition;
                CurrentDragPosition = startingPosition;
                DragDelta = Vector3.zero;
                ModifyAxis = modifyAxis;
            }

            public DragInformation(Vector3 startingPosition, Vector3 currentPosition, Axis modifyAxis)
            {
                StartDragPosition = startingPosition;
                CurrentDragPosition = currentPosition;
                DragDelta = currentPosition - startingPosition;
                ModifyAxis = modifyAxis;
            }

            public DragInformation(Vector3 startingPosition, Vector3 currentPosition, Vector3 lastPosition, Axis modifyAxis)
            {
                StartDragPosition = startingPosition;
                CurrentDragPosition = currentPosition;
                DragDelta = currentPosition - lastPosition;
                ModifyAxis = modifyAxis;
            }
        }

        public UnityEvent<DragInformation> OnStartDrag = new UnityEvent<DragInformation>();
        public UnityEvent<DragInformation> OnDuringDrag = new UnityEvent<DragInformation>();
        public UnityEvent<DragInformation> OnEndDrag = new UnityEvent<DragInformation>();

        public GameObject SpawnIn;

        protected CommandBuffer _commandBuffer;
        protected CommandBuffer _commandBufferStencil;

        private bool _gizmoActive = false;

        public abstract Quaternion XAxisRotation();
        public abstract Quaternion YAxisRotation();
        public abstract Quaternion ZAxisRotation();
        public virtual Color XAxisColor() => Color.red;
        public virtual Color YAxisColor() => Color.green;
        public virtual Color ZAxisColor() => Color.blue;
        public virtual Color DeselectedColor() => Color.white * 0.5f;
        public virtual Color SelectedColor() => Color.yellow;

        private static float _nonHover = 0.6f;
        private static Color _multNonHover = new Color(_nonHover, _nonHover, _nonHover);

        // Why this specific shade of blue? It's the parkitect ui blue!
        private static Color _outlineHover = new Color(0.6863f, 1f, 1f, 0.3608f);

        public virtual void OnCreate()
        {
            this._commandBuffer = new CommandBuffer();
            this._commandBuffer.name = gameObject.name;

            this._commandBufferStencil = new CommandBuffer();
            this._commandBufferStencil.name = gameObject.name + ".stencil";

            GameObject wo = Instantiate<GameObject>(SpawnIn, transform);
            ZComponent = wo.AddComponent<T>();
            wo.layer = LAYER;
            wo.name = "ZComponent";

            wo = Instantiate<GameObject>(SpawnIn, transform);
            YComponent = wo.AddComponent<T>();
            wo.layer = LAYER;
            wo.name = "YComponent";

            wo = Instantiate<GameObject>(SpawnIn, transform);
            XComponent = wo.AddComponent<T>();
            wo.layer = LAYER;
            wo.name = "XComponent";

            OnStartDrag = new UnityEvent<DragInformation>();
            OnDuringDrag = new UnityEvent<DragInformation>();
            OnEndDrag = new UnityEvent<DragInformation>();

            OnStartDrag.AddListener(OnDragStart);
            OnDuringDrag.AddListener(OnDrag);
            OnEndDrag.AddListener(OnDragEnd);

            UpdateGizmoTransforms();

        }

        public void SetGizmoColour(GizmoComponent checkGizmo, GizmoComponent hoverGizmo, Color axisCol)
        {
            Color mainCol = axisCol;
            Color outlineCol = axisCol;

            // Dragging this
            if (_currentlyDragging == checkGizmo)
            {
                Color selCol = SelectedColor();
                mainCol = selCol;
                outlineCol = selCol;
            }
            // Nobody is dragging
            else if (_currentlyDragging == null)
            {
                if (hoverGizmo == checkGizmo)
                {
                    outlineCol = _outlineHover;
                }
                else
                {
                    mainCol *= _multNonHover;
                    outlineCol = mainCol;
                }
            }
            // Not this gizmo dragged
            else
            {
                Color unSelCol = DeselectedColor();
                mainCol = unSelCol;
                outlineCol = unSelCol;
            }

            checkGizmo.SetColor(mainCol, outlineCol);

        }

        public void SetColorsByActive(GizmoComponent HoverGizmo)
        {
            SetGizmoColour(ZComponent, HoverGizmo, ZAxisColor());
            SetGizmoColour(YComponent, HoverGizmo, YAxisColor());
            SetGizmoColour(XComponent, HoverGizmo, XAxisColor());
        }

        public void SetActiveGizmo(bool active)
        {
            XComponent.gameObject.SetActive(active); 
            YComponent.gameObject.SetActive(active);
            ZComponent.gameObject.SetActive(active);
            _gizmoActive = active;
        }

        public virtual void OnDragStart(DragInformation eventInfo) { }
        public virtual void OnDrag(DragInformation eventInfo) { UpdateGizmoTransforms(); }
        public virtual void OnDragEnd(DragInformation eventInfo) { }

        protected virtual void UpdateGizmoTransforms()
        {
            XComponent.transform.rotation = (_rotationMode == ToolSpace.GLOBAL) ? XAxisRotation() : transform.rotation * XAxisRotation();
            YComponent.transform.rotation = (_rotationMode == ToolSpace.GLOBAL) ? YAxisRotation() : transform.rotation * YAxisRotation();
            ZComponent.transform.rotation = (_rotationMode == ToolSpace.GLOBAL) ? ZAxisRotation() : transform.rotation * ZAxisRotation();
        }
        public virtual void OnDragCheck()
        {

            if (!_gizmoActive)
            {
                return;
            }

            // Get cam
            if (_cachedMaincam == null)
            {
                _cachedMaincam = Camera.main;
                return;
            }

            // do raycast
            RaycastHit hit;
            Ray mouseRay = _cachedMaincam.ScreenPointToRay(Input.mousePosition);
            bool raycastRes = Physics.Raycast(mouseRay, out hit, Mathf.Infinity, LAYER_MASK);

            GizmoComponent Hover = null;

            if (raycastRes && !UIUtility.isMouseOverUIElement())
            {
                Hover = hit.collider.GetComponent<GizmoComponent>();
            }

            // Started drag
            if (Input.GetKeyDown(KeyCode.Mouse0) && Hover != null)
            {

                Axis axis = Axis.NONE;

                if (Hover == XComponent) { _currentlyDragging = XComponent; axis = Axis.X; }
                else if (Hover == YComponent) { _currentlyDragging = YComponent; axis = Axis.Y; }
                else if (Hover == ZComponent) { _currentlyDragging = ZComponent; axis = Axis.Z; }

                _lastInfo = new DragInformation(_currentlyDragging.GetPlaneOffset(mouseRay), axis);
                OnStartDrag.Invoke(_lastInfo);

            }
            // Holding drag
            else if (Input.GetKey(KeyCode.Mouse0) && _currentlyDragging != null)
            {

                Vector3 thisDrag = _currentlyDragging.GetPlaneOffset(mouseRay);
                _lastInfo = new DragInformation(_lastInfo.StartDragPosition, thisDrag, _lastInfo.CurrentDragPosition, _lastInfo.ModifyAxis);
                OnDuringDrag.Invoke(_lastInfo);

            }
            // Stop drag
            else if (_currentlyDragging != null)
            {
                OnEndDrag.Invoke(_lastInfo);
                _currentlyDragging = null;
            }

            SetColorsByActive(Hover);

            // Render objects
            _commandBuffer.Clear();
            _commandBufferStencil.Clear();

            ZComponent?.Render(_commandBuffer, _commandBufferStencil); 
            YComponent?.Render(_commandBuffer, _commandBufferStencil);
            XComponent?.Render(_commandBuffer, _commandBufferStencil);

        }
    }
}