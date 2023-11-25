using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TransformAnarchy
{
    [HarmonyPatch(typeof(Builder), "changeRotation")]
    internal static class VanillaRotationAngle
    {
        static bool Prefix(Quaternion ___rotation, ref Quaternion __state)
        {
            __state = ___rotation;
            return false;
        }
        static void Postfix(int direction, ref Quaternion ___rotation, Quaternion __state, ref Vector3 ___forward, ref bool ___dontAutoRotate)
        {
            bool orientationKey = InputManager.getKey("orientationKey");
            float angle = (float)direction * TA.TASettings.rotationAngle;
            ___rotation = Quaternion.Euler(0f, angle, 0f) * __state;
            ___dontAutoRotate = true;
            ___forward = ___rotation * Vector3.forward;
        }
    }
}
