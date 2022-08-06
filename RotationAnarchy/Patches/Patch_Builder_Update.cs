using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(Builder), "Update")]
    internal static class Patch_Builder_Update
    {
        private static bool _previousFrameActive;

        static void Postfix(ref Quaternion ___rotation, ref Vector3 ___forward)
        {
            // We reset the rotation of the Builder if RA has just deactivated.
            if (!RA.Controller.Active)
            {
                if(_previousFrameActive)
                {
                    ___forward = Vector3.forward;
                    ___rotation = Quaternion.LookRotation(___forward);
                }
            }

            _previousFrameActive = RA.Controller.Active;
        }
    }
}