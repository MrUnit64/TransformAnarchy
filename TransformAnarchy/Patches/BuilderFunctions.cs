using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Parkitect;
using UnityEngine;


namespace TransformAnarchy
{
    public static class BuilderFunctions
    {

        public static MethodBase checkEnableUndergroundMode = AccessTools.Method(typeof(Builder), "checkEnableUndergroundMode");

        public static MethodBase buildObjects = AccessTools.Method(typeof(Builder), "buildObjects");

        public static MethodBase updatePriceTag = AccessTools.Method(typeof(Builder), "updatePriceTag");

        public static MethodBase destroyBuildIndicator = AccessTools.Method(typeof(Builder), "destroyBuildIndicator");

        public static MethodBase instantiateNoBuildIndicator = AccessTools.Method(typeof(Builder), "instantiateNoBuildIndicator");

        public static MethodBase instantiateBuildIndicator = AccessTools.Method(typeof(Builder), "instantiateBuildIndicator");

        public static MethodBase clearGhostMaterial = AccessTools.Method(typeof(Builder), "clearGhostMaterial");

        public static MethodBase turnIntoGhostMaterial = AccessTools.Method(typeof(Builder), "turnIntoGhostMaterial", parameters: new Type[] { typeof(Transform), typeof(Material) });

        public static MethodBase canBuildAt = AccessTools.Method(typeof(Builder), "canBuildAt");

        public static MethodBase changeSize = AccessTools.Method(typeof(Builder), "changeSize", parameters: new Type[] { typeof(float) });

        public static bool MainTAPrefix(
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

            bool refreshRepresentation = false;

            // Do early update of TA
            TA.MainController.OnBeforeInit();

            Builder b = TA.MainController.CurrentBuilder;

            if (!___ghost.activeSelf)
            {
                ___ghost.SetActive(true);
                refreshRepresentation = true;
            }


            if (TA.MainController.GizmoEnabled)
            {

                // Do first init of gizmo
                if (!TA.MainController.GizmoCurrentState)
                {
                    TA.MainController.InitGizmoTransform(___ghost, ___ghostPos, ___rotation);
                    TA.MainController.GizmoCurrentState = true;
                    refreshRepresentation = true;
                }

                refreshRepresentation |= TA.MainController.UseTransformFromLastBuilder;

                if (TA.MainController.UseTransformFromLastBuilder)
                {
                    TA.MainController.UseTransformFromLastBuilder = false;
                }

                // Do main code
                TA.MainController.OnBuilderUpdate();

                Vector3 curPos;
                Quaternion curRot;

                TA.MainController.GetBuildTransform(out curPos, out curRot);

                bool changedPosFlag = curPos != ___ghost.transform.position;
                bool changedRotFlag = curRot != ___ghost.transform.rotation;

                refreshRepresentation |= changedPosFlag || changedRotFlag;

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

                // Update visual ghost position
                ___ghost.transform.position = curPos;
                ___ghost.transform.rotation = curRot;

                // Update place at ghost position
                ___ghostPos = curPos;
                ___rotation = curRot;
                ___forward = ___rotation * Vector3.forward;

                // Basically moves the money label if the gizmos are moved
                if (refreshRepresentation)
                {
                    RedoBuildables(b, ref ___canBuild, ref ___ghostCantBuildMaterial,
                        ref ___ghostIntersectMaterial, ref ___ghostMaterial,
                        ref ___builtObjectsCanBuildCache, ref ___actualBuiltObjects, true);

                    PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.checkEnableUndergroundMode);
                }

                bool mouseUp = Input.GetMouseButtonUp(0);
                bool buildKeyUp = InputManager.getKeyUp("buildObject");

                // Only place if mouse clicked (or build key pressed), gizmos aren't in use and mouse isn't over a UI element
                if ((mouseUp && !TA.MainController.GizmoControlsBeingUsed
                    && !UIUtility.isMouseOverUIElement() && UIUtility.isMouseUsable() && !UIUtility.isInputFieldFocused()
                    && !TA.MainController.PipetteWaitForMouseUp && !TA.MainController.IsEditingOrigin
                    || TA.MainController.ForceBuildThisFrame) || (buildKeyUp
                    && !UIUtility.isMouseOverUIElement() && !UIUtility.isInputFieldFocused()
                    && !TA.MainController.PipetteWaitForMouseUp && !TA.MainController.IsEditingOrigin
                    || TA.MainController.ForceBuildThisFrame))
                {

                    if (TA.MainController.ForceBuildThisFrame)
                    {
                        TA.MainController.ForceBuildThisFrame = false;
                    }

                    // Actually can build
                    if (___canBuild.result)
                    {
                        PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.buildObjects);
                    }
                    else
                    {
                        ErrorMessageController.showErrorMessage(___canBuild.message);
                    }
                }

                if (TA.MainController.PipetteWaitForMouseUp && mouseUp)
                {
                    TA.MainController.PipetteWaitForMouseUp = false;
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
            PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.updatePriceTag);
            ___builtObjectsCanBuildCache.Clear();
            BuildableObject.CanBuild canBuildNew = (BuildableObject.CanBuild)PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.canBuildAt);

            if (!canBuildNew.isEquivalent(___canBuild) || forceUpdate)
            {

                PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.destroyBuildIndicator);

                if (!canBuildNew.result)
                {
                    PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.instantiateNoBuildIndicator);
                }
                else
                {
                    PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.instantiateBuildIndicator);
                }

                // Clear and redo materials
                PatchUtils.InvokeParamless(typeof(Builder), b, BuilderFunctions.clearGhostMaterial);
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

                for (int i = 0; i < ___actualBuiltObjects.Count; i++)
                {
                    BuildableObject buildableObject = ___actualBuiltObjects[i];
                    BuilderFunctions.turnIntoGhostMaterial.Invoke(b, new object[] { buildableObject.transform, material });
                }

                ___canBuild = canBuildNew;

            }
        }
    }
}
