using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Parkitect.UI;

namespace TA
{
    [HarmonyPatch]
    public class ObjectPipetteButtonOnObjectSelectedPatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(ObjectPipetteButton), "onObjectSelected", parameters: new Type[] { typeof(BuildableObject) });

        [HarmonyPostfix]
        public static void Postfix(BuildableObject buildableObject, Builder ___builder)
        {

            Debug.Log("Hello from Object Pipette selected method!");

            // We want to ALWAYS spawn with the gizmo. So we shall.
            TA.MainController.GizmoEnabled = true;
            TA.MainController.GizmoCurrentState = false;

            Traverse builderTrv = Traverse.Create(___builder);

            // set ghost rot and loc
            builderTrv.Field("ghostPos").SetValue(buildableObject.logicTransform.position);
            builderTrv.Field("rotation").SetValue(buildableObject.logicTransform.rotation);

        }
    }
}
