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
            RA.Controller.NotifyBuildState(true, __instance);
            return true;

        }
    }
}