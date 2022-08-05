using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(Builder), "changeRotation")]
    internal static class Patch_Builder_changeRotation
    {
        static bool Prefix(Quaternion ___rotation, ref Quaternion __state)
        {
            __state = ___rotation;
            return false;
        }
        static void Postfix(int direction, ref Quaternion ___rotation, Quaternion __state, ref Vector3 ___forward, ref bool ___dontAutoRotate)
        {
            bool directionKey = RotationAnarchyMod.DirectionHotkey.Pressed;
            bool localSpaceKey = RotationAnarchyMod.LocalRotationHotkey.Pressed;

            float angle = (float)direction * RotationAnarchyMod.RotationAngle.Value;

            if (RotationAnarchyMod.Controller.Active)
            {
                if (directionKey)
                {
                    // Local Space
                    if (localSpaceKey)
                    {
                        ___rotation = __state * Quaternion.Euler(0f, 0f, angle);
                    }
                    // World Space
                    else
                    {
                        ___rotation = Quaternion.Euler(0f, 0f, angle) * __state;
                    }
                }
                else
                {
                    // Local Space
                    if (localSpaceKey)
                    {
                        ___rotation = __state * Quaternion.Euler(0f, angle, 0f);
                    }
                    // World Space
                    else
                    {
                        ___rotation = Quaternion.Euler(0f, angle, 0f) * __state;
                    }
                }
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