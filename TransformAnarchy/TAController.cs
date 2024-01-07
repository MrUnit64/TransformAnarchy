using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Parkitect.UI;
using UnityEngine.Rendering;

namespace TransformAnarchy
{
    [DefaultExecutionOrder(-10)]
    public class TAController : MonoBehaviour
    {

        public GameObject ArrowGO;
        public GameObject RingGO;

        public Builder CurrentBuilder;
        public float BuilderSize;
        public float GridSubdivision = 1f;
        public bool ShouldSnap = false;
        public bool GizmoCurrentState = false;
        public bool GizmoEnabled = false;
        public bool GizmoControlsBeingUsed = false;
        public bool IsEditingOrigin = false;
        public bool useFixedGizmoSize = true;
        // public float gizmoSize;

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
        private Camera gizmoCamera;

        private Transform _gizmoHelperParent;
        private Transform _gizmoHelperChild;

        public GameObject UITransform;

        public struct UIButton
        {
            public Button button;
            public Image icon;
            public UITooltip tooltip;

            public UIButton(Button b, Image i = null, UITooltip t = null)
            {
                this.button = b;
                this.icon = i;
                this.tooltip = t;
            }
        }

        public UIButton UIToolButton;
        public UIButton UISpaceButton;
        public UIButton UIBuildButton;
        public UIButton UIGizmoToggleButton;
        public UIButton UIResetRotationButton;
        public UIButton UIPivotEdit;
        public UIButton UIPivotCancel;

        // Flags
        public bool UseTransformFromLastBuilder = false;
        public bool PipetteWaitForMouseUp = false;
        private bool _alreadyToggledThisFrame = false;

        // We cannot directly build the builder. So we instead do this.
        public bool ForceBuildThisFrame = false;
        private bool _dontUpdateGrid = false;

        // Allowed builder types
        public static HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
            typeof(FlatRideBuilder),
            typeof(BlueprintBuilder)
        };

        public void OnBuilderEnable(Builder builder)
        {

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

            if (!UseTransformFromLastBuilder)
            {
                _gizmoHelperChild.transform.localPosition = Vector3.zero;
                _gizmoHelperChild.transform.localRotation = Quaternion.identity;
            }

            CurrentBuilder = builder;
            UpdateUIContent();
            SetGizmoCamera();

        }

        public void OnBuilderDisable()
        {

            if (CurrentBuilder == null)
            {
                return;
            }

            UseTransformFromLastBuilder = GizmoEnabled && CurrentBuilder.GetType() == typeof(DecoBuilder);
            StartCoroutine(StoppedBuildingWatch());

            CurrentBuilder = null;
            GizmoEnabled = false;
            GizmoCurrentState = false;
            positionalGizmo.SetActiveGizmo(false);
            rotationalGizmo.SetActiveGizmo(false);
            CurrentTool = Tool.MOVE;
            CurrentSpace = ToolSpace.LOCAL;

            ClearBuilderGrid();
            UpdateUIContent();

        }

        public void InitGizmoTransform(GameObject ghost, Vector3 position, Quaternion rotation)
        {

            SetGizmoTransform(position, rotation);

            // Object size based gizmo
            if (TA.TASettings.gizmoStyle == 2)
            {
                BuilderSize = Mathf.Clamp(ghost.GetRecursiveBounds().size.magnitude * 1.1f, 1f, 50f * TA.TASettings.gizmoSize);

                positionalGizmo.transform.localScale = Vector3.one * BuilderSize;
                rotationalGizmo.transform.localScale = Vector3.one * BuilderSize;
            }
            // Fixed size based gizmo
            else if (TA.TASettings.gizmoStyle == 0) 
            {
                BuilderSize = TA.TASettings.gizmoSize;
                positionalGizmo.transform.localScale = Vector3.one * BuilderSize;
                rotationalGizmo.transform.localScale = Vector3.one * BuilderSize;
            }
        }

        // Screen size based gizmo, run every frame in OnBuilderUpdate if enabled
        public void UpdateGizmoSize()
        {
            if (TA.TASettings.gizmoStyle == 1)
            {
                // Get the distance between the gizmo position and the camera
                float screenDistance = Vector3.Distance(positionalGizmo.transform.position, Camera.main.transform.position);

                // Calculate the gizmo size based on the screen size and the distance from the camera
                BuilderSize = Mathf.Clamp((Screen.height / 30000f) * screenDistance * TA.TASettings.gizmoSize, 0.2f, 50f);

                // Set the gizmo size
                positionalGizmo.transform.localScale = Vector3.one * BuilderSize;
                rotationalGizmo.transform.localScale = Vector3.one * BuilderSize;
            }
        }

        public void GetBuildTransform(out Vector3 wsPos, out Quaternion wsRot)
        {
            wsPos = _gizmoHelperChild.transform.position;
            wsRot = _gizmoHelperChild.transform.rotation;
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
            _gizmoHelperParent.transform.position = positionalGizmo.transform.position;
            _gizmoHelperParent.transform.rotation = rotationalGizmo.transform.rotation;
            UpdateBuilderGridToGizmo();
        }

        public void SetGizmoMoving(bool moving)
        {
            GizmoControlsBeingUsed = moving;
        }

        public void SetGizmoEnabled(bool setTo, bool setGizmoCurrentState = false)
        {

            GizmoEnabled = setTo;
            GizmoCurrentState = setGizmoCurrentState;

            if (!GizmoEnabled)
            {
                _gizmoHelperChild.transform.localPosition = Vector3.zero;
                _gizmoHelperChild.transform.localRotation = Quaternion.identity;
                IsEditingOrigin = false;
            }

            StartCoroutine(WaitToAllowToggle());
            UpdateUIContent();

            if (setTo)
            {
                UpdateBuilderGridToGizmo();
            }
            else
            {
                ClearBuilderGrid();
            }
        }

        public void ResetGizmoRotation()
        {


            Vector3 lastFullPosition = _gizmoHelperChild.transform.position;
            Quaternion lastFullRotation = _gizmoHelperChild.transform.rotation;

            _gizmoHelperParent.transform.rotation = Quaternion.identity;

            if (IsEditingOrigin)
            {
                _gizmoHelperChild.transform.position = lastFullPosition;
                _gizmoHelperChild.transform.rotation = lastFullRotation;
            }

            positionalGizmo.transform.position = _gizmoHelperParent.transform.position;
            rotationalGizmo.transform.rotation = _gizmoHelperParent.transform.rotation;

            UpdateGizmoTransforms();
            UpdateBuilderGridToGizmo();

        }

        public void ResetPivot()
        {

            Vector3 cachedBuilderPos = _gizmoHelperChild.transform.position;
            Quaternion cachedBuilderRot = _gizmoHelperChild.transform.rotation;

            _gizmoHelperParent.transform.position = cachedBuilderPos;
            _gizmoHelperParent.transform.rotation = cachedBuilderRot;
            positionalGizmo.transform.position = cachedBuilderPos;
            rotationalGizmo.transform.rotation = cachedBuilderRot;
            _gizmoHelperChild.transform.localPosition = Vector3.zero;
            _gizmoHelperChild.transform.localRotation = Quaternion.identity;

            IsEditingOrigin = false;

            UpdateGizmoTransforms();
            UpdateUIContent();

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

        public void TogglePivotEdit()
        {
            IsEditingOrigin = !IsEditingOrigin;
            UpdateUIContent();
        }

        public void UpdateUIContent()
        {

            // Pivot editing update
            UIBuildButton.button.interactable = !IsEditingOrigin;

            UIPivotEdit.icon.sprite = (IsEditingOrigin) ? TA.TickSprite : TA.OriginMoveSprite;
            UIPivotEdit.tooltip.text = (IsEditingOrigin) ? "Keep pivot changes" : "Change pivot";

            // Icon updates
            UIToolButton.icon.sprite = (CurrentTool == Tool.MOVE) ? TA.RotateSprite : TA.MoveSprite;
            UIToolButton.tooltip.text = (CurrentTool == Tool.MOVE) ? "Rotate tool" : "Move tool";
            UISpaceButton.icon.sprite = (CurrentSpace == ToolSpace.LOCAL) ? TA.GlobalSprite : TA.LocalSprite;
            UISpaceButton.tooltip.text = (CurrentSpace == ToolSpace.LOCAL) ? "Global space" : "Local space";

            // Main update
            UITransform.SetActive(CurrentBuilder != null && GizmoEnabled);
        }

        public void UpdateUIPosition()
        {

            if (_cachedMaincam == null)
            {
                return;
            }

            // left and up relative to cam from position of gizmo, with width calced
            UITransform.transform.position = _cachedMaincam.WorldToScreenPoint(
                positionalGizmo.transform.position +
                _cachedMaincam.transform.rotation * (new Vector3(0.9f, 0.9f, 0) * BuilderSize));

        }

        public void UpdateBuilderGridToGizmo()
        {
            GameController.Instance.terrainGridProjector.transform.position = positionalGizmo.transform.position;
            GameController.Instance.terrainGridBuilderProjector.transform.position = positionalGizmo.transform.position;
            GameController.Instance.terrainGridProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, rotationalGizmo.transform.forward);
            GameController.Instance.terrainGridBuilderProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, rotationalGizmo.transform.forward);
        }

        private void UpdateGizmoTransforms()
        {
            // Keep both gizmos sync'd with eachother
            rotationalGizmo.UpdatePosition(positionalGizmo.transform.position);
            positionalGizmo.UpdateRotation(rotationalGizmo.transform.rotation);

            _gizmoHelperParent.transform.position = positionalGizmo.transform.position;
            _gizmoHelperParent.transform.rotation = rotationalGizmo.transform.rotation;
        }

        public void ClearBuilderGrid()
        {
            GameController.Instance.terrainGridProjector.transform.position = Vector3.zero;
            GameController.Instance.terrainGridBuilderProjector.transform.position = Vector3.zero;
            GameController.Instance.terrainGridProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            GameController.Instance.terrainGridBuilderProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        }

        public void SetGizmoCamera()
        {
            if (_cachedMaincam != null)
            {
                // Configure gizmo camera settings
                if (TA.TASettings.gizmoRenderBehaviourString == 0)
                {
                    gizmoCamera.cullingMask = 1 << 28;
                    // Set depth to normal
                    gizmoCamera.depth = _cachedMaincam.depth + 100;
                }
                else
                {
                    // Don't question this
                    gizmoCamera.cullingMask = 1 << 31;
                    // Set depth to render on top
                    gizmoCamera.depth = _cachedMaincam.depth - 1;
                }
            }
            else
            {
                return;
            }
        }

        public void OnEnable()
        {

            Debug.Log("TA: Enabling TAController");

            // Spawn gizmo offset helpers (i honestly tried to do this without them and I suffered)
            _gizmoHelperParent = new GameObject().GetComponent<Transform>();
            _gizmoHelperParent.SetParent(this.transform, false);
            _gizmoHelperParent.transform.localPosition = Vector3.zero;
            _gizmoHelperParent.transform.localRotation = Quaternion.identity;

            _gizmoHelperChild = new GameObject().GetComponent<Transform>();
            _gizmoHelperChild.SetParent(_gizmoHelperParent, false);
            _gizmoHelperParent.transform.localPosition = Vector3.zero;
            _gizmoHelperParent.transform.localRotation = Quaternion.identity;


            // Positional Gizmo
            positionalGizmo = (new GameObject()).AddComponent<PositionalGizmo>();
            positionalGizmo.gameObject.name = "Positional Gizmo";
            positionalGizmo.SpawnIn = TA.ArrowGO;
            positionalGizmo.OnCreate();

            // Rotational Gizmo
            rotationalGizmo = (new GameObject()).AddComponent<RotationalGizmo>();
            rotationalGizmo.gameObject.name = "Rotational Gizmo";
            rotationalGizmo.SpawnIn = TA.RingGO;
            rotationalGizmo.OnCreate();

            positionalGizmo.OnDuringDrag.AddListener(a => SetGizmoMoving(true));
            positionalGizmo.OnEndDrag.AddListener(a => StartCoroutine(WaitToSetMovingOff()));

            rotationalGizmo.OnDuringDrag.AddListener(a => SetGizmoMoving(true));
            rotationalGizmo.OnEndDrag.AddListener(a => StartCoroutine(WaitToSetMovingOff()));

            // Ui window time.
            TA.UiHolder.SetActive(false);
            UITransform = Instantiate(TA.UiHolder, Parkitect.UI.UIWorldOverlayController.Instance.transform);

            // Temp vars
            Button b;
            Image i;
            UITooltip t;

            b = UITransform.transform.Find("Gizmo_Button").GetComponent<Button>();
            i = b.transform.Find("Image").GetComponent<Image>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            UIToolButton = new UIButton(b, i, t);

            b = UITransform.transform.Find("Space_Button").GetComponent<Button>();
            i = b.transform.Find("Image").GetComponent<Image>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            UISpaceButton = new UIButton(b, i, t);

            b = UITransform.transform.Find("Build_Button").GetComponent<Button>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            t.text = "Build";
            UIBuildButton = new UIButton(b, null, t);

            b = UITransform.transform.Find("Reset_Button").GetComponent<Button>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            t.text = "Reset rotation";
            UIResetRotationButton = new UIButton(b, null, t);

            b = UITransform.transform.Find("Cancel_Button").GetComponent<Button>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            t.text = "Use basic move";
            UIGizmoToggleButton = new UIButton(b, null, t);

            b = UITransform.transform.Find("Pivot_Set_Button").GetComponent<Button>();
            i = b.transform.Find("Image").GetComponent<Image>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            UIPivotEdit = new UIButton(b, i, t);

            b = UITransform.transform.Find("Pivot_Cancel_Button").GetComponent<Button>();
            t = b.gameObject.AddComponent<UITooltip>();
            t.context = "Transform Anarchy";
            t.text = "Cancel pivot changes";
            UIPivotCancel = new UIButton(b, null, t);

            UIToolButton.button.onClick.AddListener(ToggleGizmoTool);
            UISpaceButton.button.onClick.AddListener(ToggleGizmoSpace);
            UIPivotEdit.button.onClick.AddListener(TogglePivotEdit);
            UIPivotCancel.button.onClick.AddListener(ResetPivot);

            UIBuildButton.button.onClick.AddListener(() => ForceBuildThisFrame = true && !IsEditingOrigin);
            UIGizmoToggleButton.button.onClick.AddListener(() => SetGizmoEnabled(!GizmoEnabled));
            UIResetRotationButton.button.onClick.AddListener(ResetGizmoRotation);

            Debug.Log("TA: transform Anarchy initialized");

            UpdateUIContent();

        }

        // basically wait two frames in order to make sure 
        public IEnumerator StoppedBuildingWatch()
        {
            yield return null;
            yield return null;
            UseTransformFromLastBuilder = false;
        }

        public IEnumerator WaitToSetMovingOff()
        {
            yield return null;
            SetGizmoMoving(false);
        }

        public IEnumerator WaitToAllowToggle()
        {
            _alreadyToggledThisFrame = true;
            yield return null;
            _alreadyToggledThisFrame = false;
        }

        public void OnDisable()
        {

            UIToolButton.button.onClick.RemoveListener(ToggleGizmoTool);
            UISpaceButton.button.onClick.RemoveListener(ToggleGizmoSpace);
            UIPivotEdit.button.onClick.RemoveListener(TogglePivotEdit);
            UIPivotCancel.button.onClick.RemoveListener(ResetPivot);

            UIBuildButton.button.onClick.RemoveListener(() => ForceBuildThisFrame = true && !IsEditingOrigin);
            UIGizmoToggleButton.button.onClick.RemoveListener(() => SetGizmoEnabled(!GizmoEnabled));
            UIResetRotationButton.button.onClick.RemoveListener(ResetGizmoRotation);

            Destroy(UITransform);

            positionalGizmo.OnDuringDrag.RemoveListener(a => SetGizmoMoving(true));
            positionalGizmo.OnEndDrag.RemoveListener(a => StartCoroutine(WaitToSetMovingOff()));
            rotationalGizmo.OnDuringDrag.RemoveListener(a => SetGizmoMoving(true));
            rotationalGizmo.OnEndDrag.RemoveListener(a => StartCoroutine(WaitToSetMovingOff()));

            Destroy(positionalGizmo.gameObject);
            Destroy(rotationalGizmo.gameObject);

            ClearBuilderGrid();

            // Clear bit
            if (_cachedMaincam == null) return;
            _cachedMaincam.cullingMask = _cachedMaincam.cullingMask & (~Gizmo<PositionalGizmoComponent>.LAYER_MASK);

            Debug.Log($"TA - Disabled");

        }

        public void OnBeforeInit()
        {

            if (_cachedMaincam == null)
            {
                // Cache the main camera
                _cachedMaincam = Camera.main;

                if (_cachedMaincam != null)
                {
                    _cachedMaincam.cullingMask = _cachedMaincam.cullingMask | Gizmo<PositionalGizmoComponent>.LAYER_MASK;

                    // Create a new camera for gizmo rendering
                    GameObject gizmoCameraObject = new GameObject("GizmoCamera");
                    gizmoCamera = gizmoCameraObject.AddComponent<Camera>();

                    // Set gizmo camera as a child of the main camera
                    gizmoCamera.transform.parent = _cachedMaincam.transform;

                    // Copy relevant properties from the main camera
                    gizmoCamera.CopyFrom(_cachedMaincam);

                    // Set to only depth buffer
                    gizmoCamera.clearFlags = CameraClearFlags.Depth;

                    // Set the layer mask for gizmo rendering
                    gizmoCamera.cullingMask = 1 << 28;

                }
                else
                {
                    return;
                }
            }

            if (InputManager.getKeyDown("toggleGizmoOn") && CurrentBuilder != null && !_alreadyToggledThisFrame && !UIUtility.isInputFieldFocused())
            {
                Debug.Log("TA: Toggled building mode");
                SetGizmoEnabled(!GizmoEnabled);
                _dontUpdateGrid = true;
                GridSubdivision = GameController.Instance.terrainGridBuilderProjector.gridSubdivision;
            }
            else if (CurrentBuilder == null)
            {
                OnBuilderDisable();
            }
            else if (!GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(false);
                rotationalGizmo.SetActiveGizmo(false);
            }
        }

        public void OnBuilderUpdate()
        {

            bool gridMode = InputManager.getKey("BuildingSnapToGrid");
            bool updateGizmo = ShouldSnap != gridMode || _dontUpdateGrid;
            ShouldSnap = gridMode;

            if (gridMode && !_dontUpdateGrid)
            {

                float currentSub = GridSubdivision;

                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    GridSubdivision = 10f;
                }
                else
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        if (Input.GetKeyDown(i.ToString() ?? "") || Input.GetKeyDown("[" + i.ToString() + "]"))
                        {
                            GridSubdivision = (float)i;
                        }
                    }
                }

                if (currentSub != GridSubdivision)
                {
                    updateGizmo = true;
                }
            }
            else
            {
                ClearBuilderGrid();
            }

            if (updateGizmo)
            {
                GameController.Instance.terrainGridBuilderProjector.setGridSubdivision(GridSubdivision);
                UpdateBuilderGridToGizmo();
            }

            if (_dontUpdateGrid)
            {
                _dontUpdateGrid = false;
            }

            // Reimplement size hotkeys directly
            if (InputManager.getKey("BuildingIncreaseObjectSize") && !UIUtility.isInputFieldFocused())
            {
                BuilderFunctions.changeSize.Invoke(CurrentBuilder, new object[] { 0.01f });


            }
            else if (InputManager.getKey("BuildingDecreaseObjectSize") && !UIUtility.isInputFieldFocused())
            {
                BuilderFunctions.changeSize.Invoke(CurrentBuilder, new object[] { -0.01f });
            }

            // Keybinds
            if (InputManager.getKeyDown("toggleGizmoSpace") && !gridMode && !UIUtility.isInputFieldFocused())
            {
                ToggleGizmoSpace();
            }
            if (InputManager.getKeyDown("toggleGizmoTool") && !gridMode && !UIUtility.isInputFieldFocused())
            {
                ToggleGizmoTool();
            }
            if (InputManager.getKeyDown("resetGizmoTool") && !gridMode && !UIUtility.isInputFieldFocused())
            {
                ResetGizmoRotation();
            }
            if (InputManager.getKeyDown("togglePivotEdit") && !gridMode && !UIUtility.isInputFieldFocused())
            {
                TogglePivotEdit();
            }
            if (InputManager.getKeyDown("cancelPivotEdit") && !gridMode && !UIUtility.isInputFieldFocused())
            {
                ResetPivot();
            }

            // Toggle tools based on the current state
            if (CurrentTool == Tool.MOVE && GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(true);
                rotationalGizmo.SetActiveGizmo(false);
                positionalGizmo.CurrentRotationMode = CurrentSpace;
                rotationalGizmo.CurrentRotationMode = CurrentSpace;

                // cache position
                Vector3 currentPosition = positionalGizmo.transform.position;

                positionalGizmo.OnDragCheck();

                if (IsEditingOrigin)
                {
                    _gizmoHelperParent.transform.position = positionalGizmo.transform.position;
                    _gizmoHelperChild.transform.position -= (positionalGizmo.transform.position - currentPosition);
                }
            }
            else if (CurrentTool == Tool.ROTATE && GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(false);
                rotationalGizmo.SetActiveGizmo(true);
                positionalGizmo.CurrentRotationMode = CurrentSpace;
                rotationalGizmo.CurrentRotationMode = CurrentSpace;

                Quaternion lastFullRotation = _gizmoHelperChild.transform.rotation;
                Vector3 lastFullPosition = _gizmoHelperChild.transform.position;

                rotationalGizmo.OnDragCheck();

                if (IsEditingOrigin)
                {
                    _gizmoHelperParent.transform.rotation = rotationalGizmo.transform.rotation;
                    _gizmoHelperChild.transform.position = lastFullPosition;
                    _gizmoHelperChild.transform.rotation = lastFullRotation;
                }
            }

            // Update gizmo size
            UpdateGizmoSize();

            // Sync gizmos
            UpdateGizmoTransforms();

            // Update UI position
            UpdateUIPosition();

        }
    }
}
