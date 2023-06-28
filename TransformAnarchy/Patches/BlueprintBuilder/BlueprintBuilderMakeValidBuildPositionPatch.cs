using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved
{

    [HarmonyPatch]
    public class BlueprintBuilderMakeValidBuildPositionPatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(BlueprintBuilder), "makeValidBuildPosition", parameters: new Type[]{typeof(Vector3)});

        [HarmonyPrefix]
        public static bool Prefix(Vector3 position, ref Vector3 __result)
        {

            // Skip 
            if (TA.MainController.CurrentBuilder != null && TA.MainController.GizmoEnabled)
            {
                Debug.Log("BlueprintBuilder: Position valid position was changed.");
                __result = position;
                return false;
            }

            return true;

        }
    }
}
