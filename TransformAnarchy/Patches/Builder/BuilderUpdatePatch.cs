using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved
{

    [HarmonyPatch]
    public class BuilderUpdatePatch
    {

        // Get protected method and make it public so we can patch
        static MethodBase TargetMethod() => AccessTools.Method(typeof(Builder), "Update");

        [HarmonyPrefix]
        public static bool Prefix(ref GameObject ___ghost, ref Vector3 ___ghostPos, ref Quaternion ___rotation, ref Vector3 ___forward, ref List<BuildableObject> ___actualBuiltObjects, ref Material ___ghostMaterial)
        {

            // Builder is actually editable?
            if (TA.MainController.CurrentBuilder == null)
            {
                return true;
            }

            // INJECT TA CODE
            TA.MainController.OnBuilderUpdate();

            // This init's the special ghost glowing stuff when an object is placed
            if (!___ghost.activeSelf)
            {

                MethodInfo method = AccessTools.Method(typeof(Builder), "turnIntoGhostMaterial");

                for (int i = 0; i < ___actualBuiltObjects.Count; i++)
                {
                    BuildableObject buildableObject = ___actualBuiltObjects[i];

                    method.Invoke(TA.MainController.CurrentBuilder, new object[] { buildableObject.transform, ___ghostMaterial });
                }

                ___ghost.SetActive(true);

            }

            // Just turn off the

            if (TA.MainController.CurrentBuilder != null && TA.MainController.GizmoEnabled)
            {

                Builder b = TA.MainController.CurrentBuilder;

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
                    PatchUtils.InvokeParamless(typeof(Builder), b, "updatePriceTag");
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
                    PatchUtils.InvokeParamless(typeof(Builder), b, "buildObjects");
                }

                return false;

            }

            return true;

        }
    }
}
