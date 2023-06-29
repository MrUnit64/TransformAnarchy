using HarmonyLib;
using TA;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch]
public class BuilderOnEnablePatch
{

    // Get protected method and make it public so we can patch
    static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "OnEnable");

    [HarmonyPrefix]
    public static void Prefix(Builder __instance)
    {
        TA.TA.MainController.OnBuilderEnable(__instance);
    }
}