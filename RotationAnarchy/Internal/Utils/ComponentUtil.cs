namespace RotationAnarchy.Internal
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Reflection;

    public static class ComponentUtil
    {
        public enum CopyPropertiesType
        {
            FieldsPublic,
            FieldsPrivate,
            FieldsPublicAndPrivate
        }

        public static void DeleteAllComponents(this GameObject go, Type compType)
        {
            var arr = go.GetComponents(compType);
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                GameObject.Destroy(arr[i]);
            }
        }

        public static void TraverseTransformsForeach(Transform root, Action<Transform> forEach)
        {
            forEach(root);
            int cc = root.childCount;
            for (int i = 0; i < cc; i++)
            {
                var c = root.GetChild(i);
                TraverseTransformsForeach(c, forEach);
            }
        }

        public static void TraverseComponentForeach<T>(Transform root, Action<T> forEach) where T : Component
        {
            var comp = root.GetComponent<T>();
            if (comp)
            {
                forEach(comp);
            }
            int cc = root.childCount;
            for (int i = 0; i < cc; i++)
            {
                var c = root.GetChild(i);

                TraverseComponentForeach(c, forEach);
            }
        }

        public static void CopyComponentProperties(CopyPropertiesType config, Component lhs, Component rhs, bool onlyTargetClass = true, string[] ignore = null)
        {
            if (lhs == null || rhs == null)
            {
                throw new ArgumentNullException();
            }

            switch (config)
            {
                case CopyPropertiesType.FieldsPublic:
                    {
                        CopyFieldsPublic(lhs, rhs, ignore);
                    }
                    break;
                case CopyPropertiesType.FieldsPrivate:
                    break;
                case CopyPropertiesType.FieldsPublicAndPrivate:
                    break;
                default:
                    break;
            }
        }

        private static Dictionary<string, FieldInfo> cacheFields = new Dictionary<string, FieldInfo>();
        private static List<FieldInfo> tempFieldInfoList = new List<FieldInfo>();
        private static HashSet<string> tempStringHashSet = new HashSet<string>();

        private static void CopyFieldsPublic(Component lhs, Component rhs, string[] ignore)
        {
            var lhs_f = lhs.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var rhs_f = rhs.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (ignore != null)
            {
                tempStringHashSet.Clear();
                for (int i = 0; i < ignore.Length; i++)
                {
                    tempStringHashSet.Add(ignore[i]);
                }
            }

            cacheFields.Clear();
            foreach (var item in rhs_f)
            {
                cacheFields.Add(item.Name, item);
            }

            int count = 0;
            foreach (var item in lhs_f)
            {
                if (tempStringHashSet.Contains(item.Name))
                    continue;

                if (cacheFields.TryGetValue(item.Name, out var found))
                {
                    found.SetValue(rhs, item.GetValue(lhs));
                    count++;
                }
            }
        }
    }
}