using UnityEngine;
using HarmonyLib;

namespace RotationAnarchy.Patches
{
    [HarmonyPatch(typeof(CameraController), "updateZoom")]
    internal static class Patch_CameraController_updateZoom
    {
        static void Postfix(ref float ___currentZoom)
        {
            if (RA.Controller.Active)
            {
                RA.Controller.NotifyCameraControllerCurrentZoom(___currentZoom);
            }
        }
    }
}