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
        private static bool _previousFrameActive;

        static bool Prefix(Quaternion ___rotation, ref Quaternion __state)
        {
            __state = ___rotation;
            return !RA.Controller.Active;
        }

        static void Postfix(int direction, ref Quaternion ___rotation, Quaternion __state, ref Vector3 ___forward, ref bool ___dontAutoRotate)
        {
            if (RA.Controller.Active)
            {
                bool directionKey = RA.DirectionHotkey.Pressed;
                bool localSpaceKey = RA.LocalRotationHotkey.Pressed;

                float angle = (float)direction * RA.RotationAngle.Value;

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

                ___dontAutoRotate = true;
                ___forward = ___rotation * Vector3.forward;
            }
        }
    }
}