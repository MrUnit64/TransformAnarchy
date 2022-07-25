using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy
{
    internal static class Settings
    {
        internal static string rotationAngleString = "90";
        internal static float rotationAngle = 90;

        internal static void checkValues()
        {
            rotationAngleString = rotationAngleString.Replace(",", ".");
            if (float.TryParse(rotationAngleString, out rotationAngle) && rotationAngle < 360)
            {
                //Save in file for next time maybe?
            }
            else
            {
                rotationAngle = 90f;
            }
            rotationAngleString = rotationAngle.ToString();
        }
    }

}
