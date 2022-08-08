namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GizmoController : ModComponent
    {
        PlacementModeGizmo placementRotationGizmo;

        private List<AbstractGizmo> gizmos = new List<AbstractGizmo>();

        public override void OnApplied()
        {
            placementRotationGizmo = new PlacementModeGizmo();
            placementRotationGizmo.Axis = GizmoAxis.Y;
            gizmos.Add(placementRotationGizmo);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!Camera.main) // Gizmos are dependent on camera
                return;

            for (int i = 0; i < gizmos.Count; i++)
            {
                gizmos[i].Update();
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

            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    var debug = GetComponent<RADebug>();
            //    var material = placementRotationGizmo.stencilMaterial = debug.allMaterials[currentDebugMaterial++];
            //
            //    RA.Instance.LOG($"Material: {material.name}, shader: {material.shader.name}");
            //
            //    //for (int i = 0; i < material.shader.GetPropertyCount(); i++)
            //    //{
            //    //    var id = material.shader.GetPropertyNameId(i);
            //    //    var type = material.shader.GetPropertyType(i);
            //    //    var name = material.shader.GetPropertyName(i);
            //    //
            //    //    string start = $"prop {i}: {name}, type: {type}, value:";
            //    //
            //    //    switch (type)
            //    //    {
            //    //        case UnityEngine.Rendering.ShaderPropertyType.Color:
            //    //            RA.Instance.LOG(start + material.GetColor(id));
            //    //            break;
            //    //        case UnityEngine.Rendering.ShaderPropertyType.Vector:
            //    //            RA.Instance.LOG(start + material.GetVector(id));
            //    //            break;
            //    //        case UnityEngine.Rendering.ShaderPropertyType.Float:
            //    //            RA.Instance.LOG(start + material.GetFloat(id));
            //    //            break;
            //    //        case UnityEngine.Rendering.ShaderPropertyType.Range:
            //    //            RA.Instance.LOG(start + material.GetFloat(id));
            //    //            break;
            //    //        case UnityEngine.Rendering.ShaderPropertyType.Texture:
            //    //            RA.Instance.LOG(start + material.GetTexture(id)?.na);
            //    //            break;
            //    //        default:
            //    //            break;
            //    //    }
            //    //
            //    //}
            //
            //    if (currentDebugMaterial >= debug.allMaterials.Count)
            //    {
            //        currentDebugMaterial = 0;
            //    }
            //}
        }

        public override void OnReverted()
        {
            for (int i = gizmos.Count - 1; i >= 0; i--)
            {
                gizmos[i].Destroy();
            }
            gizmos.Clear();
            placementRotationGizmo = null;
        }
    }


}