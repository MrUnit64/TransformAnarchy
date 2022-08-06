namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GizmoController : ModComponent
    {
        RotationAxisGizmo testGizmo;
        private int currentDebugMaterial;

        bool showDebugGizmo = true;

        public override void OnApplied()
        {
            testGizmo = new RotationAxisGizmo(new Vector3(0,0.1f, 0), Quaternion.LookRotation(Vector3.up));
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            testGizmo.SetActive(false);

            if (RA.Controller.Active)
            {
                if(RA.Controller.GameState == ParkitectState.Placement)
                {
                    if(RA.Controller.ActiveBuilder)
                    {
                        if(RA.Controller.ActiveGhost)
                        {
                            testGizmo.SetActive(true);
                            testGizmo.SnapToActiveBuilder();
                        }
                    }
                }
            }

            //if (Input.GetKeyDown(KeyCode.Keypad5))
            //    showDebugGizmo = !showDebugGizmo;
            //
            //if (showDebugGizmo && testGizmo != null && Camera.main != null)
            //{
            //    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, float.MaxValue))
            //    {
            //        testGizmo.GameObject.transform.position = hit.point + new Vector3(0, 10, 0);
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    var debug = GetComponent<RADebug>();
            //    testGizmo.material = debug.allMaterials[currentDebugMaterial++];
            //    if (currentDebugMaterial >= debug.allMaterials.Count)
            //    {
            //        currentDebugMaterial = 0;
            //    }
            //}
        }

        public override void OnReverted()
        {
            testGizmo.Destroy();
            testGizmo = null;
        }
    }


}