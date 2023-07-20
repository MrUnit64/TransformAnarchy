using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Parkitect.UI;

namespace TransformAnarchy
{
    [HarmonyPatch]
    public class ObjectPipetteButtonOnObjectSelectedPatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(ObjectPipetteButton), "onObjectSelected", parameters: new Type[] { typeof(BuildableObject) });

        [HarmonyPostfix]
        public static void Postfix(BuildableObject buildableObject, Builder ___builder)
        {

            if (TA.MainController.UseTransformFromLastBuilder || !InputManager.getKey("usePipetteGizmo"))
            {
                return;
            }

            // Run the stuff
            TA.MainController.UseTransformFromLastBuilder = true;


            // We want to ALWAYS spawn with the gizmo. So we shall.
            TA.MainController.SetGizmoEnabled(true);
            TA.MainController.SetGizmoTransform(buildableObject.logicTransform.position, buildableObject.logicTransform.rotation);
            TA.MainController.UpdateUIContent();
            TA.MainController.PipetteWaitForMouseUp = true;

            // We cannot make this any faster unfortunately 
            Traverse builderTrv = Traverse.Create(___builder);

            // set ghost rot and loc
            builderTrv.Field("ghostPos").SetValue(buildableObject.logicTransform.position);
            builderTrv.Field("rotation").SetValue(buildableObject.logicTransform.rotation);

        }
    }
}
