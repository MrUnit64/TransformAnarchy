namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Reflection;

    public class RADebug : ModComponent
    {
        public List<Material> allMaterials = new List<Material>();

        public override void OnApplied()
        {
            var fields = typeof(AssetManager).GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if(field.FieldType == typeof(Material))
                {
                    allMaterials.Add(field.GetValue(AssetManager.Instance) as Material);
                }
            }
        }

        public override void OnReverted()
        {
            allMaterials.Clear();
        }
    }


}