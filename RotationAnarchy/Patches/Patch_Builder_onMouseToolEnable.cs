using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(Builder), "onMouseToolEnable")]
    internal static class Patch_Builder_onMouseToolEnable
    {

        static bool Prefix(Builder __instance)
        {

            // Call as active
            RotationAnarchyMod.Controller.SetBuildState(true, __instance);
            return true;

        }
    }
}