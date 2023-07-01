using HarmonyLib;
using TransformAnarchy;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using Parkitect;
using Parkitect.UI;

[HarmonyPatch]
public class BuilderOnDisablePatch
{

    // Get protected method and make it public so we can patch
    static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "OnDisable");

    [HarmonyPrefix]
    public static void Prefix()
    {
        TransformAnarchy.TA.MainController.OnBuilderDisable();
    }
}