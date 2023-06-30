using HarmonyLib;
using TransformAnarchy;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch]
public class BuilderOnEnablePatch
{

    // Get protected method and make it public so we can patch
    static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "OnEnable");

    [HarmonyPostfix]
    public static void Postfix(Builder __instance, ref Vector3 ___ghostPos, ref Quaternion ___rotation)
    {

        if (TA.MainController.UseTransformFromLastBuilder)
        {

            // We want to ALWAYS spawn with the gizmo. So we shall.
            TA.MainController.SetGizmoEnabled(true);
            ___ghostPos = TA.MainController.positionalGizmo.transform.position;
            ___rotation = TA.MainController.rotationalGizmo.transform.rotation;

            TA.MainController.UpdateUIContent();

        }

        TransformAnarchy.TA.MainController.OnBuilderEnable(__instance);

    }
}