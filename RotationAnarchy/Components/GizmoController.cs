namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GizmoController : ModComponent
    {
        PlacementModeGizmo placementRotationGizmo;
        TranslationGizmo translationGizmo;
        private TrackballModeGizmo _trackballModeGizmo;

        private List<AbstractGizmo> gizmos = new List<AbstractGizmo>();

        public override void OnApplied()
        {
            placementRotationGizmo = new PlacementModeGizmo();
            placementRotationGizmo.Axis = Axis.Y;
            gizmos.Add(placementRotationGizmo);

            translationGizmo = new TranslationGizmo();
            gizmos.Add(translationGizmo);

            _trackballModeGizmo = new TrackballModeGizmo();
            gizmos.Add(_trackballModeGizmo);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!Camera.main) // Gizmos are dependent on camera
                return;

            foreach (var gizmo in gizmos)
            {
                gizmo.Update();
            }

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
            foreach (var gizmo in gizmos)
            {
                gizmo.Destroy();
            }
            gizmos.Clear();
            placementRotationGizmo = null;
            translationGizmo = null;
            _trackballModeGizmo = null;
        }
    }


}