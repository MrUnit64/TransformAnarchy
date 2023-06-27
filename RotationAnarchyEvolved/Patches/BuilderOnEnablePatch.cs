using HarmonyLib;
using RotationAnarchyEvolved;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch]
public class BuilderOnEnablePatch
{

    // Get protected method and make it public so we can patch
    static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "OnEnable");

    [HarmonyPrefix]
    public static void PreOnEnable(Builder __instance)
    {
        RAE.MainController.SetBuilder(__instance);
    }
}