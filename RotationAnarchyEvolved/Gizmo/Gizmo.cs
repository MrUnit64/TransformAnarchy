using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    
    public enum Mode
    {
        LOCAL,
        GLOBAL
    };

    [SerializeField]
    protected Mode _rotationMode;

    public Mode CurrentRotationMode
    {
        get
        {
            return _rotationMode;
        }
        set
        {
            _rotationMode = value;
            UpdateGizmoTransforms();
        }
    }

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

    public abstract Quaternion XAxisRotation();
    public abstract Quaternion YAxisRotation();
    public abstract Quaternion ZAxisRotation();
    public virtual Color XAxisColor() => Color.red;
    public virtual Color YAxisColor() => Color.green;
    public virtual Color ZAxisColor() => Color.blue;
    public virtual Color DeselectedColor() => Color.white * 0.5f;

    public virtual void Start()
    {

        GameObject wo = Instantiate<GameObject>(SpawnIn, transform);
        ZComponent = wo.AddComponent<T>();
        ZComponent.SetColor(ZAxisColor());
        //wo.layer = LAYER;

        wo = Instantiate<GameObject>(SpawnIn, transform);
        YComponent = wo.AddComponent<T>();
        YComponent.SetColor(YAxisColor());
        //wo.layer = LAYER;

        wo = Instantiate<GameObject>(SpawnIn, transform);
        XComponent = wo.AddComponent<T>();
        XComponent.SetColor(XAxisColor());
        //wo.layer = LAYER;

        OnDuringDrag.AddListener(OnDrag);

        UpdateGizmoTransforms();

    }

    public void SetColorsByActive()
    {
        ZComponent.SetColor((_currentlyDragging == ZComponent || _currentlyDragging == null) ? ZAxisColor() : DeselectedColor());
        YComponent.SetColor((_currentlyDragging == YComponent || _currentlyDragging == null) ? YAxisColor() : DeselectedColor());
        XComponent.SetColor((_currentlyDragging == XComponent || _currentlyDragging == null) ? XAxisColor() : DeselectedColor());
    }

    public virtual void OnDrag(DragInformation eventInfo) { UpdateGizmoTransforms(); }
    protected virtual void UpdateGizmoTransforms()
    {
        XComponent.transform.rotation = (_rotationMode == Mode.GLOBAL) ? XAxisRotation() : transform.rotation * XAxisRotation();
        YComponent.transform.rotation = (_rotationMode == Mode.GLOBAL) ? YAxisRotation() : transform.rotation * YAxisRotation();
        ZComponent.transform.rotation = (_rotationMode == Mode.GLOBAL) ? ZAxisRotation() : transform.rotation * ZAxisRotation();
    }

    public virtual void Update()
    {

        // Get cam
        if (_cachedMaincam == null)
        {
            _cachedMaincam = Camera.main;

            if (_cachedMaincam == null)
            {
                return;
            }
        }

        // do raycast
        RaycastHit hit;
        Ray mouseRay = _cachedMaincam.ScreenPointToRay(Input.mousePosition);
        bool raycastRes = Physics.Raycast(mouseRay, out hit, Mathf.Infinity);

        // Started drag
        if (Input.GetKeyDown(KeyCode.Mouse0) && raycastRes)
        {

            GizmoComponent hitComponent = hit.collider.GetComponent<GizmoComponent>();
            Axis axis = Axis.NONE;

            if (hitComponent == XComponent) { _currentlyDragging = XComponent; axis = Axis.X; }
            else if (hitComponent == YComponent) { _currentlyDragging = YComponent; axis = Axis.Y; }
            else if (hitComponent == ZComponent) { _currentlyDragging = ZComponent; axis = Axis.Z; }

            _lastInfo = new DragInformation(hitComponent.GetPlaneOffset(mouseRay), axis);
            OnStartDrag.Invoke(_lastInfo);
            SetColorsByActive();

        }
        // Holding drag
        else if (Input.GetKey(KeyCode.Mouse0) && _currentlyDragging != null){

            Vector3 thisDrag = _currentlyDragging.GetPlaneOffset(mouseRay);
            _lastInfo = new DragInformation(_lastInfo.StartDragPosition, thisDrag, _lastInfo.CurrentDragPosition, _lastInfo.ModifyAxis);
            OnDuringDrag.Invoke(_lastInfo);

        }
        // Stop drag
        else if (_currentlyDragging != null)
        {
            OnEndDrag.Invoke(_lastInfo);
            _currentlyDragging = null;
            SetColorsByActive();
        }
    }
}
