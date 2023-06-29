using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TA
{
    [DefaultExecutionOrder(-10)]
    public class TAController : MonoBehaviour
    {

        public GameObject ArrowGO;
        public GameObject RingGO;

        public Builder CurrentBuilder;
        public float BuilderSize;
        public bool GizmoCurrentState = false;
        public bool GizmoEnabled = false;
        public bool GizmoControlsBeingUsed = false;

        public enum Tool
        {
            MOVE,
            ROTATE
        };

        public Tool CurrentTool = Tool.MOVE;
        public ToolSpace CurrentSpace = ToolSpace.LOCAL;

        public PositionalGizmo positionalGizmo;
        public RotationalGizmo rotationalGizmo;
        private Camera _cachedMaincam;

        public GameObject UITransform;
        public Button UIToolButton;
        public Button UISpaceButton;
        public Image UIToolIcon;
        public Image UISpaceIcon;

        // Allowed builder types
        public static HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
            typeof(FlatRideBuilder),
            typeof(BlueprintBuilder)
        };

        public void OnBuilderEnable(Builder builder)
        {

            Debug.Log($"Builder: {builder}, type {builder.GetType()}");

            if (builder == CurrentBuilder)
            {
                return;
            }

            if (builder != null)
            {
                if (!AllowedBuilderTypes.Contains(builder.GetType()))
                {
                    OnBuilderDisable();
                    return;
                }
            }

            Debug.Log("Builder enabled");

            CurrentBuilder = builder;

            UpdateUIContent();

        }

        public void OnBuilderDisable()
        {

            Debug.Log("Builder disabled");
            CurrentBuilder = null;
            GizmoEnabled = false;
            GizmoCurrentState = false;
            positionalGizmo.SetActiveGizmo(false);
            rotationalGizmo.SetActiveGizmo(false);
            CurrentTool = Tool.MOVE;
            CurrentSpace = ToolSpace.LOCAL;

            UpdateUIContent();

        }

        public void InitGizmoTransform(GameObject ghost, Vector3 position, Quaternion rotation)
        {

            SetGizmoTransform(position, rotation);

            BuilderSize = Mathf.Clamp(ghost.GetRecursiveBounds().size.magnitude * 1.1f, 1f, 50f);

            positionalGizmo.transform.localScale = Vector3.one * BuilderSize;
            rotationalGizmo.transform.localScale = Vector3.one * BuilderSize;

        }

        public void GetGizmoTransform(out Vector3 wsPos, out Quaternion wsRot)
        {
            wsPos = positionalGizmo.transform.position;
            wsRot = rotationalGizmo.transform.rotation;
        }

        public void SetGizmoTransform(Vector3 position, Quaternion rotation)
        {
            positionalGizmo.transform.position = position;
            rotationalGizmo.transform.rotation = rotation;
        }

        public void SetGizmoMoving(bool moving)
        {
            GizmoControlsBeingUsed = moving;
        }

        public void ToggleGizmoTool()
        {
            switch (CurrentTool)
            {
                case Tool.MOVE:
                    CurrentTool = Tool.ROTATE;
                    break;
                case Tool.ROTATE:
                    CurrentTool = Tool.MOVE;
                    break;
            }

            UpdateUIContent();

        }

        public void ToggleGizmoSpace()
        {
            switch (CurrentSpace)
            {
                case ToolSpace.LOCAL:
                    CurrentSpace = ToolSpace.GLOBAL;
                    break;
                case ToolSpace.GLOBAL:
                    CurrentSpace = ToolSpace.LOCAL;
                    break;
            }

            UpdateUIContent();

        }

        public void UpdateUIContent()
        {
            UIToolIcon.sprite = TA.MoveSprite;
            UISpaceIcon.sprite = TA.RotateSprite;
            UITransform.SetActive(CurrentBuilder != null && GizmoEnabled);
        }

        public void UpdateUIPosition()
        {

            if (_cachedMaincam == null)
            {
                return;
            }

            Vector3 diag = Vector3.right + Vector3.up;

            // left and up relative to cam from position of gizmo, with width calced
            UITransform.transform.position = _cachedMaincam.WorldToScreenPoint(
                positionalGizmo.transform.position +
                _cachedMaincam.transform.rotation * (diag * BuilderSize));

        }

        public void OnEnable()
        {

            Debug.Log("Enabling TAController");

            // Positional Gizmo
            positionalGizmo = (new GameObject()).AddComponent<PositionalGizmo>();
            positionalGizmo.gameObject.name = "Positional Gizmo";
            positionalGizmo.SpawnIn = TA.ArrowGO;
            positionalGizmo.OnCreate();

            Debug.Log("Positional gizmo Init");

            // Rotational Gizmo
            rotationalGizmo = (new GameObject()).AddComponent<RotationalGizmo>();
            rotationalGizmo.gameObject.name = "Rotational Gizmo";
            rotationalGizmo.SpawnIn = TA.RingGO;
            rotationalGizmo.OnCreate();

            Debug.Log("Rotational gizmo Init");

            positionalGizmo.OnDuringDrag.AddListener(a => SetGizmoMoving(true));
            positionalGizmo.OnEndDrag.AddListener(a => StartCoroutine(WaitToSetMovingOff()));

            rotationalGizmo.OnDuringDrag.AddListener(a => SetGizmoMoving(true));
            rotationalGizmo.OnEndDrag.AddListener(a => StartCoroutine(WaitToSetMovingOff()));

            Debug.Log("Gizmo events Init");

            Debug.Log("Creating UI for TA");

            // Ui window time.
            TA.UiHolder.SetActive(false);
            UITransform = Instantiate(TA.UiHolder, Parkitect.UI.UIWorldOverlayController.Instance.transform);

            Debug.Log("Inited main transform");

            // Get stuff
            UIToolButton = UITransform.transform.Find("Gizmo_Button").GetComponent<Button>();
            UIToolIcon = UIToolButton.transform.Find("Image").GetComponent<Image>();

            Debug.Log("Inited UIToolButton");
            Debug.Log($"UIToolButton: {UIToolButton.name}");
            Debug.Log($"UIToolIcon parent: {UIToolIcon.transform.parent.name}");

            UISpaceButton = UITransform.transform.Find("Space_Button").GetComponent<Button>();
            UISpaceIcon = UIToolButton.transform.Find("Image").GetComponent<Image>();

            Debug.Log("Inited UISpaceButton");
            Debug.Log($"UISpaceButton: {UISpaceButton.name}");
            Debug.Log($"UIToolIcon parent: {UISpaceIcon.transform.parent.name}");

            UIToolButton.onClick.AddListener(ToggleGizmoTool);
            UISpaceButton.onClick.AddListener(ToggleGizmoSpace);

            Debug.Log("Inited Events");

            UpdateUIContent();

        }

        public IEnumerator WaitToSetMovingOff()
        {
            yield return null;
            SetGizmoMoving(false);
        }

        public void OnDisable()
        {

            UIToolButton.onClick.RemoveListener(ToggleGizmoTool);
            UISpaceButton.onClick.RemoveListener(ToggleGizmoSpace);

            Destroy(UITransform);

            positionalGizmo.OnDuringDrag.RemoveListener(a => SetGizmoMoving(true));
            positionalGizmo.OnEndDrag.RemoveListener(a => StartCoroutine(WaitToSetMovingOff()));
            rotationalGizmo.OnDuringDrag.RemoveListener(a => SetGizmoMoving(true));
            rotationalGizmo.OnEndDrag.RemoveListener(a => StartCoroutine(WaitToSetMovingOff()));

            Destroy(positionalGizmo.gameObject);
            Destroy(rotationalGizmo.gameObject);

            // Clear bit
            if (_cachedMaincam == null) return;
            _cachedMaincam.cullingMask = _cachedMaincam.cullingMask & (~Gizmo<PositionalGizmoComponent>.LAYER_MASK);

            Debug.Log($"TA - Disabled");

        }

        public void OnBuilderUpdate()
        {

            if (_cachedMaincam == null)
            {
                _cachedMaincam = Camera.main;

                if (_cachedMaincam != null)
                {
                    _cachedMaincam.cullingMask = _cachedMaincam.cullingMask | Gizmo<PositionalGizmoComponent>.LAYER_MASK;
                }
                else
                {
                    return;
                }
            }

            if (InputManager.getKeyDown("toggleGizmoOn") && CurrentBuilder != null)
            {
                GizmoEnabled = !GizmoEnabled;

                UpdateUIContent();

                if (!GizmoEnabled)
                {
                    GizmoCurrentState = false;
                }
            }
            else if (CurrentBuilder == null)
            {
                OnBuilderDisable();
            }
            else if (!GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(false);
                rotationalGizmo.SetActiveGizmo(false);
                return;
            }

            // toggles
            if (InputManager.getKeyDown("toggleGizmoSpace"))
            {
                ToggleGizmoSpace();
            }

            if (InputManager.getKeyDown("toggleGizmoTool"))
            {
                ToggleGizmoTool();
            }

            if (CurrentTool == Tool.MOVE && GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(true);
                rotationalGizmo.SetActiveGizmo(false);
                positionalGizmo.CurrentRotationMode = CurrentSpace;
                positionalGizmo.OnDragCheck();
            }
            else if (CurrentTool == Tool.ROTATE && GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(false);
                rotationalGizmo.SetActiveGizmo(true);
                rotationalGizmo.CurrentRotationMode = CurrentSpace;
                rotationalGizmo.OnDragCheck();
            }

            rotationalGizmo.transform.position = positionalGizmo.transform.position;
            positionalGizmo.transform.rotation = rotationalGizmo.transform.rotation;

            // Update UI position
            UpdateUIPosition();

        }
    }
}
