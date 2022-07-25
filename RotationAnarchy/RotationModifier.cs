using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy
{
    [HarmonyPatch(typeof(Builder), "changeRotation")]
    internal static class RotationModifier
    {
        static bool Prefix(Quaternion ___rotation, ref Quaternion __state)
        {
            __state = ___rotation;
            return false;
        }
        static void Postfix(int direction, ref Quaternion ___rotation, Quaternion __state, ref Vector3 ___forward, ref bool ___dontAutoRotate)
        {
            bool orientationKey = InputManager.getKey("orientationKey");
            float angle = (float)direction * Settings.rotationAngle;
            if (orientationKey)
            {
                ___rotation = Quaternion.Euler(0f, 0f, angle) * __state;
            }
            else
            {
                ___rotation = Quaternion.Euler(0f, angle, 0f) * __state;
            }
            ___dontAutoRotate = true;
            ___forward = ___rotation * Vector3.forward;
        }
    }
}