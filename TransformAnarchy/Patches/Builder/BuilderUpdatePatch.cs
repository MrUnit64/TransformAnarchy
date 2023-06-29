using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TA
{

    [HarmonyPatch]
    public class BuilderUpdatePatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "Update");

        [HarmonyPrefix]
        public static bool Prefix(
            ref GameObject ___ghost, ref Vector3 ___ghostPos, ref Quaternion ___rotation, 
            ref Vector3 ___forward, ref List<BuildableObject> ___actualBuiltObjects, 
            ref Material ___ghostMaterial, ref Material ___ghostCantBuildMaterial,
            ref Material ___ghostIntersectMaterial,
            ref Dictionary<BuildableObject, BuildableObject.CanBuild> ___builtObjectsCanBuildCache,
            ref BuildableObject.CanBuild ___canBuild)
        {

            // Builder is actually editable?
            if (TA.MainController.CurrentBuilder == null)
            {
                return true;
            }

            Builder b = TA.MainController.CurrentBuilder;

            // INJECT TA CODE
            TA.MainController.OnBuilderUpdate();

            // This init's the special ghost glowing stuff when an object is placed
            if (!___ghost.activeSelf)
            {

                ___ghost.SetActive(true);

                RedoBuildables(b, ref ___canBuild, ref ___ghostCantBuildMaterial,
                    ref ___ghostIntersectMaterial, ref ___ghostMaterial,
                    ref ___builtObjectsCanBuildCache, ref ___actualBuiltObjects, true);

            }

            if (TA.MainController.GizmoEnabled)
            {

                // Do first init of gizmo
                if (!TA.MainController.GizmoCurrentState)
                {
                    TA.MainController.InitGizmoTransform(___ghost, ___ghostPos, ___rotation);
                    TA.MainController.GizmoCurrentState = TA.MainController.GizmoEnabled;
                }

                Vector3 curPos;
                Quaternion curRot;

                TA.MainController.GetGizmoTransform(out curPos, out curRot);

                bool changedPosFlag = curPos != ___ghost.transform.position;
                bool changedRotFlag = curRot != ___ghost.transform.rotation;

                // Update positions for other players if they are different
                if (CommandController.Instance.isInMultiplayerMode())
                {
                    if (changedPosFlag && !UIUtility.isMouseOverUIElement())
                    {
                        CommandController.Instance.multiplayerController.sendCursorPosition(curPos);
                    }
                    if (changedRotFlag && !UIUtility.isMouseOverUIElement())
                    {
                        CommandController.Instance.multiplayerController.sendCursorRotation(curRot);
                    }
                }

                // Basically moves the money label if the gizmos are moved
                if (changedPosFlag || changedRotFlag)
                {
                    RedoBuildables(b, ref ___canBuild, ref ___ghostCantBuildMaterial,
                        ref ___ghostIntersectMaterial, ref ___ghostMaterial,
                        ref ___builtObjectsCanBuildCache, ref ___actualBuiltObjects);

                    if (changedPosFlag)
                    {
                        PatchUtils.InvokeParamless(typeof(Builder), b, "checkEnableUndergroundMode");
                    }
                }

                // Update visual ghost position
                ___ghost.transform.position = curPos;
                ___ghost.transform.rotation = curRot;

                // Update place at ghost position
                ___ghostPos = curPos;
                ___rotation = curRot;
                ___forward = ___rotation * Vector3.forward;

                // Only place if mouse clicked, gizmos aren't in use and mouse isn't over a UI element
                if (Input.GetMouseButtonUp(0) && !TA.MainController.GizmoControlsBeingUsed && !UIUtility.isMouseOverUIElement())
                {

                    // Actually can build
                    if (___canBuild.result)
                    {
                        PatchUtils.InvokeParamless(typeof(Builder), b, "buildObjects");
                    }
                    else
                    {
                        ErrorMessageController.showErrorMessage(___canBuild.message);
                    }
                }

                return false;

            }

            return true;

        }

        public static void RedoBuildables(Builder b, 
            ref BuildableObject.CanBuild ___canBuild, ref Material ___ghostCantBuildMaterial,
            ref Material ___ghostIntersectMaterial, ref Material ___ghostMaterial,
            ref Dictionary<BuildableObject, BuildableObject.CanBuild> ___builtObjectsCanBuildCache,
            ref List<BuildableObject> ___actualBuiltObjects, bool forceUpdate = false)
        {
            // Redo all materials and checks
            PatchUtils.InvokeParamless(typeof(Builder), b, "updatePriceTag");
            ___builtObjectsCanBuildCache.Clear();
            BuildableObject.CanBuild canBuildNew = (BuildableObject.CanBuild)PatchUtils.InvokeParamless(typeof(Builder), b, "canBuildAt");

            if (!canBuildNew.isEquivalent(___canBuild) || forceUpdate)
            {

                PatchUtils.InvokeParamless(typeof(Builder), b, "destroyBuildIndicator");

                if (!canBuildNew.result)
                {
                    PatchUtils.InvokeParamless(typeof(Builder), b, "instantiateNoBuildIndicator");
                }
                else
                {
                    PatchUtils.InvokeParamless(typeof(Builder), b, "instantiateBuildIndicator");
                }

                // Clear and redo materials
                PatchUtils.InvokeParamless(typeof(Builder), b, "clearGhostMaterial");
                Material material;

                if (!canBuildNew.result)
                {
                    material = ___ghostCantBuildMaterial;
                }
                else if (canBuildNew.removesObjects)
                {
                    material = ___ghostIntersectMaterial;
                }
                else
                {
                    material = ___ghostMaterial;
                }

                MethodInfo method = AccessTools.Method(typeof(Builder), "turnIntoGhostMaterial");

                for (int i = 0; i < ___actualBuiltObjects.Count; i++)
                {
                    BuildableObject buildableObject = ___actualBuiltObjects[i];
                    method.Invoke(b, new object[] { buildableObject.transform, material });
                }

                ___canBuild = canBuildNew;

            }
        }
    }
}
