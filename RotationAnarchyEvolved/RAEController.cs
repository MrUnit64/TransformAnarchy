using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Parkitect;

namespace RotationAnarchyEvolved
{
    public class RAEController : MonoBehaviour
    {
        public GameObject ArrowGO;
        public GameObject RingGO;

        public PositionalGizmo positionalGizmo;
        public RotationalGizmo rotationalGizmo;

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

        public void Update()
        {
            rotationalGizmo.transform.position = positionalGizmo.transform.position;
            positionalGizmo.transform.rotation = rotationalGizmo.transform.rotation;
        }
    }
}
