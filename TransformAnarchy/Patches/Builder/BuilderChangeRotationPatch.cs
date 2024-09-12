using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace TransformAnarchy
{
    [HarmonyPatch]
    internal class BuilderChangeRotationPatch
    {
        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "changeRotation", parameters: new Type[] { typeof(int) });

        [HarmonyPrefix]
        static bool Prefix(Quaternion ___rotation, ref Quaternion __state)
        {
            __state = ___rotation;
            return false;
        }

        [HarmonyPostfix]
        static void Postfix(int direction, ref Quaternion ___rotation, Quaternion __state, ref Vector3 ___forward, ref bool ___dontAutoRotate)
        {
            float angle = (float)direction * TA.TASettings.rotationAngle;
            ___rotation = Quaternion.Euler(0f, angle, 0f) * __state;
            ___forward = ___rotation * Vector3.forward;
            ___dontAutoRotate = true;
        }
    }
}
