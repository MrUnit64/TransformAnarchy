using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public Button UIBuildButton;
        public Button UIGizmoToggleButton;
        public Button UIResetRotationButton;
        public Image UIToolIcon;
        public Image UISpaceIcon;

        public Toggle DecoBuilderTab;
        public bool UseTransformFromLastBuilder = false;
        public bool PipetteWaitForMouseUp = false;

        // We cannot directly build the builder. So we instead do this.
        public bool ForceBuildThisFrame = false;

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

            if (CurrentBuilder == null)
            {
                return;
            }

            Debug.Log("Builder disabled");

            UseTransformFromLastBuilder = GizmoEnabled && CurrentBuilder.GetType() == typeof(DecoBuilder);
            StartCoroutine(StoppedBuildingWatch());

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
            UpdateUIContent();
        }

        public void ResetGizmoRotation()
        {
            rotationalGizmo.transform.rotation = Quaternion.identity;
            UpdateBuilderGridToGizmo();
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
            UIToolIcon.sprite = (CurrentTool == Tool.MOVE) ? TA.RotateSprite : TA.MoveSprite;
            UISpaceIcon.sprite = (CurrentSpace == ToolSpace.LOCAL) ? TA.GlobalSprite : TA.LocalSprite;
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
                _cachedMaincam.transform.rotation * (new Vector3(0.75f, 0.75f, 0) * BuilderSize));

        }

        public void UpdateBuilderGridToGizmo()
        {
            GameController.Instance.terrainGridProjector.transform.position = positionalGizmo.transform.position;
            GameController.Instance.terrainGridBuilderProjector.transform.position = positionalGizmo.transform.position;
            GameController.Instance.terrainGridProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, rotationalGizmo.transform.forward);
            GameController.Instance.terrainGridBuilderProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, rotationalGizmo.transform.forward);
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
            UISpaceIcon = UISpaceButton.transform.Find("Image").GetComponent<Image>();

            Debug.Log("Inited UISpaceButton");
            Debug.Log($"UISpaceButton: {UISpaceButton.name}");
            Debug.Log($"UIToolIcon parent: {UISpaceIcon.transform.parent.name}");

            UIBuildButton = UITransform.transform.Find("Build_Button").GetComponent<Button>();
            UIGizmoToggleButton = UITransform.transform.Find("Cancel_Button").GetComponent<Button>();
            UIResetRotationButton = UITransform.transform.Find("Reset_Button").GetComponent<Button>(); 

            UIToolButton.onClick.AddListener(ToggleGizmoTool);
            UISpaceButton.onClick.AddListener(ToggleGizmoSpace);

            UIBuildButton.onClick.AddListener(() => ForceBuildThisFrame = true);
            UIGizmoToggleButton.onClick.AddListener(() => SetGizmoEnabled(!GizmoEnabled));
            UIResetRotationButton.onClick.AddListener(() => rotationalGizmo.transform.rotation = Quaternion.identity);


            Debug.Log("Inited Events");

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

        public void OnDisable()
        {

            UIToolButton.onClick.RemoveListener(ToggleGizmoTool);
            UISpaceButton.onClick.RemoveListener(ToggleGizmoSpace);

            UIBuildButton.onClick.RemoveListener(() => ForceBuildThisFrame = true);
            UIGizmoToggleButton.onClick.RemoveListener(() => SetGizmoEnabled(!GizmoEnabled));
            UIResetRotationButton.onClick.RemoveListener(() => rotationalGizmo.transform.rotation = Quaternion.identity);

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

        public void OnBeforeInit()
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

            bool snap = InputManager.getKey("BuildingSnapToGrid");
            bool setGizmoOn = InputManager.getKeyDown("toggleGizmoOn");

            if (InputManager.getKeyDown("toggleGizmoOn") && CurrentBuilder != null)
            {
                SetGizmoEnabled(!GizmoEnabled);
            }
            else if (CurrentBuilder == null)
            {
                OnBuilderDisable();
            }
            else if (!GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(false);
                rotationalGizmo.SetActiveGizmo(false);
                GameController.Instance.terrainGridProjector.transform.position = Vector3.zero;
                GameController.Instance.terrainGridBuilderProjector.transform.position = Vector3.zero;
                GameController.Instance.terrainGridProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                GameController.Instance.terrainGridBuilderProjector.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

                GridSubdivision = 1;
            }

            if (snap != ShouldSnap)
            {
                UpdateBuilderGridToGizmo();
                GameController.Instance.terrainGridProjector.setHighIntensityEnabled(snap);
                GameController.Instance.terrainGridBuilderProjector.setHighIntensityEnabled(snap);
                ShouldSnap = snap;
            }

            if (ShouldSnap)
            {

                float currentSub = GridSubdivision;

                if (setGizmoOn && GizmoEnabled)
                {
                    GridSubdivision = 1;
                }
                else
                {
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
                        GameController.Instance.terrainGridBuilderProjector.setGridSubdivision(GridSubdivision);
                    }
                }
            }
        }

        public void OnBuilderUpdate()
        {

            bool gridMode = InputManager.getKey("BuildingSnapToGrid");

            // Keybinds
            if (InputManager.getKeyDown("toggleGizmoSpace") && !gridMode)
            {
                ToggleGizmoSpace();
            }
            if (InputManager.getKeyDown("toggleGizmoTool") && !gridMode)
            {
                ToggleGizmoTool();
            }

            // Toggle tools based on the current state
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

            // Keep both gizmos sync'd with eachother
            rotationalGizmo.transform.position = positionalGizmo.transform.position;
            positionalGizmo.transform.rotation = rotationalGizmo.transform.rotation;

            // Update UI position
            UpdateUIPosition();

        }
    }
}
