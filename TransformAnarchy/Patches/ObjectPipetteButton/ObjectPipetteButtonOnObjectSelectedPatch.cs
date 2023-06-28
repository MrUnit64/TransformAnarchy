using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved
{
    public class ObjectPipetteButtonOnObjectSelectedPatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(ObjectPipetteTool), "onObjectSelected", parameters: new Type[] { typeof(BuildableObject) });

        [HarmonyPrefix]
        public static bool Prefix(BuildableObject buildableObject)
        {
            return false;
            Debug.Log("Hello from Object Pipette selected method!");

        }



        [HarmonyPostfix]
        public static void Postfix(BuildableObject buildableObject)
        {

            Debug.Log("Hello from Object Pipette selected method!");

            // We want to ALWAYS spawn with the gizmo. So we shall.
            TA.MainController.GizmoEnabled = true;
            TA.MainController.GizmoCurrentState = true;

            TA.MainController.SetGizmoTransform(buildableObject.logicTransform.position, buildableObject.logicTransform.rotation);

        }
    }
}
