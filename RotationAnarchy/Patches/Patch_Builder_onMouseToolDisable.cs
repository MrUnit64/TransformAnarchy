using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(Builder), "onMouseToolDisable")]
    internal static class Patch_Builder_onMouseToolDisable
    {

        static bool Prefix(Builder __instance)
        {

            // Call as not active
            RA.Controller.NotifyBuildState(false, __instance);
            return true;

        }
    }
}