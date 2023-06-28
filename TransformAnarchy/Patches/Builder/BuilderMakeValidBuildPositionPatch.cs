using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved
{

    [HarmonyPatch]
    public class BuilderMakeValidBuildPositionPatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "makeValidBuildPosition", parameters: new Type[]{typeof(Vector3)});

        [HarmonyPrefix]
        public static bool Prefix(Vector3 position, ref Vector3 __result)
        {

            // Skip 
            if (TA.MainController.CurrentBuilder != null && TA.MainController.GizmoEnabled)
            {
                __result = position;
                return false;
            }

            return true;

        }
    }
}
