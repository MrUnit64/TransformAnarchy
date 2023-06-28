using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace RotationAnarchyEvolved {

    public static class PatchUtils
    {

        public static object InvokeParamless(System.Type type, object instance, string methodName)
        {
            return AccessTools.Method(type, methodName).Invoke(instance, new object[] { });
        }
    }
}
