using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved
{

    [HarmonyPatch]
    public class BuilderUpdatePatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "Update");

        [HarmonyPrefix]
        public static bool Prefix(ref GameObject ___ghost, ref Vector3 ___ghostPos, ref Quaternion ___rotation, ref List<BuildableObject> ___actualBuiltObjects, ref Material ___ghostMaterial)
        {
            return BuilderUpdateSharedCode.Update<Builder>(ref ___ghost, ref ___ghostPos, ref ___rotation, ref ___actualBuiltObjects, ref ___ghostMaterial);
        }
    }
}
