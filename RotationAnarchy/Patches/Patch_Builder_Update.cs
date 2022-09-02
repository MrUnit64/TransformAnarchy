using System;
using UnityEngine;
using HarmonyLib;
using RotationAnarchy.Internal;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(Builder), "Update")]
    internal static class Patch_Builder_Update
    {
        private static bool previousFrameActive;

        static bool Prefix(ref Quaternion ___rotation, ref Vector3 ___forward, ref GameObject ___ghost, ref bool ___dontAutoRotate)
        {
            if (!RA.Controller.Active) return true;
            if (!RA.Controller.IsDragRotation) return true;

            if (Input.GetMouseButtonUp(0))
            {
                ___forward = ___rotation * Vector3.forward;
                RA.TrackballController.Reset();
                return false;
            }

            var camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("Camera was null");
            var ghostPos = ___ghost.transform.position;

            if (Input.GetMouseButtonDown(0))
            {
                RA.TrackballController.Initialize(
                    ___rotation,
                    GameObjectUtil.ComputeTotalBounds(___ghost).extents.magnitude,
                    ghostPos, Input.mousePosition);

                return false;
            }

            if (Input.GetMouseButton(0))
            {
                RA.TrackballController.UpdateState(ghostPos, Input.mousePosition);

                ___dontAutoRotate = true;
                ___rotation = RA.TrackballController.Rotation;
                ___ghost.transform.rotation = RA.TrackballController.Rotation;

                return false;
            }

            return false;
        }

        

        static void Postfix(ref Quaternion ___rotation, ref Vector3 ___forward, ref GameObject ___ghost)
        {
            // We reset the rotation of the Builder if RA has just deactivated.
            if (!RA.Controller.Active)
            {
                if(previousFrameActive)
                {
                    ___forward = Vector3.forward;
                    ___rotation = Quaternion.LookRotation(___forward);
                }
            }
            else
            {
                RA.Controller.NotifyGhost(___ghost);
            }

            previousFrameActive = RA.Controller.Active;
        }
    }
}