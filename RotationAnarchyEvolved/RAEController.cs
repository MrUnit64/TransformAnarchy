using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Parkitect;
using System.Collections;

namespace RotationAnarchyEvolved
{

    // Needs to happen before builder.
    [DefaultExecutionOrder(-1)]
    public class RAEController : MonoBehaviour
    {

        public GameObject ArrowGO;
        public GameObject RingGO;

        public Builder CurrentBuilder;
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

        // Allowed builder types
        public static HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
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
                    return;
                }
            }

            CurrentBuilder = builder;

        }

        public void OnBuilderDisable()
        {
            CurrentBuilder = null;
            GizmoEnabled = false;
            GizmoCurrentState = false;
            CurrentTool = Tool.MOVE;
            CurrentSpace = ToolSpace.LOCAL;
    }

        public void InitGizmoTransform(GameObject ghost, Vector3 position, Quaternion rotation)
        {
            positionalGizmo.transform.position = position;
            rotationalGizmo.transform.rotation = rotation;

            float sizeExtent = Mathf.Clamp(ghost.GetRecursiveBounds().extents.magnitude * 1.5f, 1f, 50f);

            positionalGizmo.transform.localScale = Vector3.one * sizeExtent;
            rotationalGizmo.transform.localScale = Vector3.one * sizeExtent;

        }

        public void GetGizmoTransform(out Vector3 wsPos, out Quaternion wsRot)
        {
            wsPos = positionalGizmo.transform.position;
            wsRot = rotationalGizmo.transform.rotation;
        }

        public void SetGizmoMoving(bool moving)
        {
            GizmoControlsBeingUsed = moving;
        }

        public void OnEnable()
        {
            // Positional Gizmo
            positionalGizmo = (new GameObject()).AddComponent<PositionalGizmo>();
            positionalGizmo.gameObject.name = "Positional Gizmo";
            positionalGizmo.SpawnIn = RAE.ArrowGO;
            positionalGizmo.OnCreate();

            // Rotational Gizmo
            rotationalGizmo = (new GameObject()).AddComponent<RotationalGizmo>();
            rotationalGizmo.gameObject.name = "Rotational Gizmo";
            rotationalGizmo.SpawnIn = RAE.RingGO;
            rotationalGizmo.OnCreate();

            positionalGizmo.OnDuringDrag.AddListener(a => SetGizmoMoving(true));
            positionalGizmo.OnEndDrag.AddListener(a => StartCoroutine(WaitToSetMovingOff()));

            rotationalGizmo.OnDuringDrag.AddListener(a => SetGizmoMoving(true));
            rotationalGizmo.OnEndDrag.AddListener(a => StartCoroutine(WaitToSetMovingOff()));

            Debug.Log($"RAE - Enabled");
            Debug.Log($"Created {positionalGizmo} and {rotationalGizmo}");

        }

        public IEnumerator WaitToSetMovingOff()
        {
            yield return null;
            SetGizmoMoving(false);
        }

        public void OnDisable()
        {

            positionalGizmo.OnDuringDrag.RemoveListener(a => SetGizmoMoving(true));
            positionalGizmo.OnEndDrag.RemoveListener(a => StartCoroutine(WaitToSetMovingOff()));
            rotationalGizmo.OnDuringDrag.RemoveListener(a => SetGizmoMoving(true));
            rotationalGizmo.OnEndDrag.RemoveListener(a => StartCoroutine(WaitToSetMovingOff()));

            Destroy(positionalGizmo.gameObject);
            Destroy(rotationalGizmo.gameObject);

            // Clear bit
            if (_cachedMaincam == null) return;
            _cachedMaincam.cullingMask = _cachedMaincam.cullingMask & (~Gizmo<PositionalGizmoComponent>.LAYER_MASK);

            Debug.Log($"RAE - Disabled");

        }

        public void Update()
        {

            Debug.Log($"RAEController - PositionalGizmo = {positionalGizmo}");
            Debug.Log($"RAEController - RotationalGizmo = {rotationalGizmo}");

            if (InputManager.getKeyDown("toggleGizmoOn") && CurrentBuilder != null)
            {
                GizmoEnabled = true;
            }

            if (!GizmoEnabled)
            {
                positionalGizmo.SetActiveGizmo(false);
                rotationalGizmo.SetActiveGizmo(false);
                return;
            }

            // toggles
            if (InputManager.getKeyDown("toggleGizmoSpace"))
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
            }

            if (InputManager.getKeyDown("toggleGizmoTool"))
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

            rotationalGizmo.transform.position = positionalGizmo.transform.position;
            positionalGizmo.transform.rotation = rotationalGizmo.transform.rotation;

        }
    }
}
