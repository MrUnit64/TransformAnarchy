using HarmonyLib;
using UnityEngine;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(Builder), "changeRotation")]
    internal static class Patch_Builder_changeRotation
    {
        static bool Prefix(Quaternion ___rotation, ref Quaternion __state)
        {
            __state = ___rotation;
            /// TODO: Backport to future versions
            return !(RA.Controller.Active && RA.Controller.GameState == ParkitectState.Placement);
        }

        static void Postfix(int direction, ref Quaternion ___rotation, Quaternion __state, ref Vector3 ___forward, ref bool ___dontAutoRotate)
        {

            // If the mod is currently active /// TODO: Backport to future versions
            if (RA.Controller.Active && RA.Controller.GameState == ParkitectState.Placement)
            {

                // True if in local space, otherwise in world space
                bool localSpaceKey = RA.Controller.IsLocalRotation;

                // Angle to rotate
                float angle = (float)direction * RA.RotationAngle.Value;

                // Init new quat to nothing
                Quaternion performingRotation = Quaternion.identity;

                // Now do a switch statement in order to make the three axes work
                switch (RA.Controller.CurrentRotationAxis)
                {
                    case Axis.X:
                        performingRotation = Quaternion.Euler(angle, 0f, 0f);
                        break;

                    case Axis.Y:
                        performingRotation = Quaternion.Euler(0f, angle, 0f);
                        break;

                    case Axis.Z:
                        performingRotation = Quaternion.Euler(0f, 0f, angle);
                        break;
                }

                // Now perform rotation depending on if in local space or not
                if (localSpaceKey)
                {
                    // Multiplying the rotation by the performing rotation essentially does a local space rotation
                    ___rotation = __state * performingRotation;
                }
                else
                {
                    // Multiplying the performing rotation by the rotation essentially does a world space rotation
                    ___rotation = performingRotation * __state;
                }
          
                ___dontAutoRotate = true;
                ___forward = ___rotation * Vector3.forward;
            }
        }
    }
}