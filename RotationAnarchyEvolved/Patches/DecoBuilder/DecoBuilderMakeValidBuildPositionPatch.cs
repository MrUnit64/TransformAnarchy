using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved
{

    [HarmonyPatch]
    public class DecoBuilderMakeValidBuildPositionPatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(DecoBuilder), "makeValidBuildPosition", parameters: new Type[]{typeof(Vector3)});

        [HarmonyPrefix]
        public static bool Prefix(Vector3 position, ref Vector3 __result)
        {

            // Skip 
            if (RAE.MainController.CurrentBuilder != null && RAE.MainController.GizmoEnabled)
            {
                __result = position;
                return false;
            }

            return true;

        }
    }
}
