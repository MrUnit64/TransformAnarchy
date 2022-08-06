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
            testGizmo = new RotationAxisGizmo("RA.RotationGizmoY", new Vector3(0,0.1f, 0), Quaternion.LookRotation(Vector3.up), new Color32(181, 230, 29, 255));
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if(!Camera.main) // Gizmos are dependent on camera
                return;

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

            return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var debug = GetComponent<RADebug>();
                var material = testGizmo.stencilMaterial = debug.allMaterials[currentDebugMaterial++];
            
                RA.Instance.LOG($"Material: {material.name}, shader: {material.shader.name}");
            
                //for (int i = 0; i < material.shader.GetPropertyCount(); i++)
                //{
                //    var id = material.shader.GetPropertyNameId(i);
                //    var type = material.shader.GetPropertyType(i);
                //    var name = material.shader.GetPropertyName(i);
                //
                //    string start = $"prop {i}: {name}, type: {type}, value:";
                //
                //    switch (type)
                //    {
                //        case UnityEngine.Rendering.ShaderPropertyType.Color:
                //            RA.Instance.LOG(start + material.GetColor(id));
                //            break;
                //        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                //            RA.Instance.LOG(start + material.GetVector(id));
                //            break;
                //        case UnityEngine.Rendering.ShaderPropertyType.Float:
                //            RA.Instance.LOG(start + material.GetFloat(id));
                //            break;
                //        case UnityEngine.Rendering.ShaderPropertyType.Range:
                //            RA.Instance.LOG(start + material.GetFloat(id));
                //            break;
                //        case UnityEngine.Rendering.ShaderPropertyType.Texture:
                //            RA.Instance.LOG(start + material.GetTexture(id)?.na);
                //            break;
                //        default:
                //            break;
                //    }
                //
                //}
            
                if (currentDebugMaterial >= debug.allMaterials.Count)
                {
                    currentDebugMaterial = 0;
                }
            }
        }

        public override void OnReverted()
        {
            testGizmo.Destroy();
            testGizmo = null;
        }
    }


}