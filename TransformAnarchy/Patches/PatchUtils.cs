using System;
using System.Collections.Generic;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TA {

    public static class PatchUtils
    {

        public static object InvokeParamless(System.Type type, object instance, string methodName)
        {
            return AccessTools.Method(type, methodName, new Type[] { }).Invoke(instance, new object[] { });
        }
    }
}
