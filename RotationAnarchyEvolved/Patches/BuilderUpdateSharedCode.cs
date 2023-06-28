using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved {

    public static class BuilderUpdateSharedCode
    {

        static object InvokeParamless(System.Type type, object instance, string methodName)
        {
            return AccessTools.Method(type, methodName).Invoke(instance, new object[] { });
        }

        public static bool Update<T>(ref GameObject ___ghost, ref Vector3 ___ghostPos, ref Quaternion ___rotation, ref List<BuildableObject> ___actualBuiltObjects, ref Material ___ghostMaterial) where T : Builder
        {
            // INJECT RAE CODE
            RAE.MainController.OnBuilderUpdate();

            // This init's the special ghost glowing stuff when an object is placed
            if (!___ghost.activeSelf)
            {

                MethodInfo method = AccessTools.Method(typeof(T), "turnIntoGhostMaterial");

                for (int i = 0; i < ___actualBuiltObjects.Count; i++)
                {
                    BuildableObject buildableObject = ___actualBuiltObjects[i];

                    method.Invoke(RAE.MainController.CurrentBuilder, new object[] { buildableObject.transform, ___ghostMaterial });
                }

                ___ghost.SetActive(true);

            }

            if (RAE.MainController.CurrentBuilder != null && RAE.MainController.GizmoEnabled)
            {

                T b = (T)RAE.MainController.CurrentBuilder;

                // Do first init of gizmo
                if (!RAE.MainController.GizmoCurrentState)
                {
                    RAE.MainController.InitGizmoTransform(___ghost, ___ghostPos, ___rotation);
                    RAE.MainController.GizmoCurrentState = RAE.MainController.GizmoEnabled;
                }

                Vector3 curPos;
                Quaternion curRot;

                RAE.MainController.GetGizmoTransform(out curPos, out curRot);

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
                    InvokeParamless(typeof(T), b, "updatePriceTag");
                }

                // Update visual ghost position
                ___ghost.transform.position = curPos;
                ___ghost.transform.rotation = curRot;

                // Update place at ghost position
                ___ghostPos = curPos;
                ___rotation = curRot;

                // Only place if mouse clicked, gizmos aren't in use and mouse isn't over a UI element
                if (Input.GetMouseButtonUp(0) && !RAE.MainController.GizmoControlsBeingUsed && !UIUtility.isMouseOverUIElement())
                {
                    InvokeParamless(typeof(T), b, "buildObjects");
                }

                return false;

            }

            return true;

        }
    }
}
