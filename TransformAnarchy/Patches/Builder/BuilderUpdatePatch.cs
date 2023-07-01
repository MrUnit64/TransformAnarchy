using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TransformAnarchy
{

    [HarmonyPatch]
    public class BuilderUpdatePatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "Update");

        /**
         *  WHAT A MONSTER!
         *  Basically, we are pretty much rewriting how the game handles rotations, and the Builder code does a LOT,
         *  due to how Harmony works this is the way we get and set variables inside of Builder.
         */

        [HarmonyPrefix]
        public static bool Prefix(
            ref GameObject ___ghost, ref Vector3 ___ghostPos, ref Quaternion ___rotation, 
            ref Vector3 ___forward, ref List<BuildableObject> ___actualBuiltObjects, 
            ref Material ___ghostMaterial, ref Material ___ghostCantBuildMaterial,
            ref Material ___ghostIntersectMaterial,
            ref Dictionary<BuildableObject, BuildableObject.CanBuild> ___builtObjectsCanBuildCache,
            ref BuildableObject.CanBuild ___canBuild)
        {

            return BuilderFunctions.MainTAPrefix(
                ref ___ghost, ref ___ghostPos, ref ___rotation,
                ref ___forward, ref ___actualBuiltObjects,
                ref ___ghostMaterial, ref ___ghostCantBuildMaterial,
                ref ___ghostIntersectMaterial,
                ref ___builtObjectsCanBuildCache,
                ref ___canBuild);

        }
    }
}
