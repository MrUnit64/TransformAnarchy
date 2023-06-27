using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Parkitect;

namespace RotationAnarchyEvolved
{

    // Needs to happen before builder.
    [DefaultExecutionOrder(-1)]
    public class RAEController : MonoBehaviour
    {

        public GameObject ArrowGO;
        public GameObject RingGO;

        public Builder _currentBuilder;

        public PositionalGizmo positionalGizmo;
        public RotationalGizmo rotationalGizmo;
        private Camera _cachedMaincam;

        // Allowed builder types
        public static HashSet<Type> AllowedBuilderTypes = new HashSet<Type>()
        {
            typeof(DecoBuilder),
            typeof(FlatRideBuilder),
        };

        public void SetBuilder(Builder builder)
        {

            if (builder == _currentBuilder)
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

            _currentBuilder = builder;
            Debug.Log("New builder");

        }

        public void Start()
        {

            // Positional Gizmo
            positionalGizmo = (new GameObject()).AddComponent<PositionalGizmo>();
            positionalGizmo.gameObject.name = "Positional Gizmo";
            positionalGizmo.SpawnIn = ArrowGO;

            // Rotational Gizmo
            rotationalGizmo = (new GameObject()).AddComponent<RotationalGizmo>();
            rotationalGizmo.gameObject.name = "Rotational Gizmo";
            rotationalGizmo.SpawnIn = RingGO;

        }

        public void OnDisable()
        {
            // Clear bit
            if (_cachedMaincam == null) return;
            _cachedMaincam.cullingMask = _cachedMaincam.cullingMask & (~Gizmo<PositionalGizmoComponent>.LAYER_MASK);
        }

        public void Update()
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

            positionalGizmo.OnDragCheck();
            rotationalGizmo.OnDragCheck();

            rotationalGizmo.transform.position = positionalGizmo.transform.position;
            positionalGizmo.transform.rotation = rotationalGizmo.transform.rotation;

        }
    }
}
