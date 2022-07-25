using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using Parkitect;

namespace RotationAnarchy
{
    public class Main : AbstractMod, IModSettings
    {

        // Constructor
        public Main()
        {
            setupControls();
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }
        // Mod data
        public override string getName()
        {
            return "Rotation Anarchy";
        }
        public override string getIdentifier()
        {
            return "RotationAnarchy";
        }
        public override string getDescription()
        {
            return "Rotates objects hopefully";
        }
        public override string getVersionNumber()
        {
            return "0.1";
        }

        // When mod is enabled/disabled
        public override void onEnabled()
        {
            base.onEnabled();
            this.rotationHarmony = new Harmony("RotationAnarchy");
            rotationHarmony.PatchAll(typeof(RotationAnarchy.Main).Assembly);
        }
        public override void onDisabled()
        {
            base.onDisabled();
            rotationHarmony.UnpatchAll("RotationAnarchy");
        }

        // When modsettings is opened/closed
        public void onDrawSettingsUI()
        {
            // GUI settings style
            GUIStyle guistyleTextLeft = new GUIStyle(GUI.skin.label);
            guistyleTextLeft.margin = new RectOffset(10, 10, 10, 0);
            guistyleTextLeft.alignment = TextAnchor.MiddleLeft;
            GUIStyle guistyleText = new GUIStyle(GUI.skin.label);
            guistyleText.margin = new RectOffset(0, 10, 10, 0);
            guistyleText.alignment = TextAnchor.MiddleCenter;

            GUIStyle guistyleField = new GUIStyle(GUI.skin.textField);
            guistyleField.margin = new RectOffset(0, 10, 10, 0);
            guistyleField.alignment = TextAnchor.MiddleCenter;

            GUIStyle guistyleButton = new GUIStyle(GUI.skin.button);
            guistyleButton.margin = new RectOffset(0, 10, 10, 0);
            guistyleButton.alignment = TextAnchor.MiddleCenter;

            // GUI settings layout
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            GUILayout.Label("Version", guistyleTextLeft, new GUILayoutOption[] { GUILayout.Height(30f), GUILayout.Width(300) });
            GUILayout.Label("Rotation angle", guistyleTextLeft);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            GUILayout.Label("0.1", guistyleText, new GUILayoutOption[] { GUILayout.Height(30f), GUILayout.Width(200) });
            Settings.rotationAngleString = GUILayout.TextField(Settings.rotationAngleString, guistyleField);
            if (GUILayout.Button("Confirm changes", guistyleButton))
            {
                Settings.checkValues();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        public void onSettingsOpened()
        {

        }
        public void onSettingsClosed()
        {

        }
        // Control settings menu
        void setupControls()
        {
            KeyGroup keyGroup = new KeyGroup(this.getIdentifier());
            keyGroup.keyGroupName = this.getName();
            ScriptableSingleton<InputManager>.Instance.registerKeyGroup(keyGroup);
            this.RegisterKey("orientationKey", KeyCode.None, "Change rotation direction", "Change the rotation direction from horizontal to vertical");
        }

        // Keymapping
        KeyMapping RegisterKey(string identifier, KeyCode keyCode, string Name, string Description = "")
		{
			KeyMapping keyMapping = new KeyMapping(identifier, keyCode, KeyCode.None);
			keyMapping.keyGroupIdentifier = this.getIdentifier();
			keyMapping.keyName = Name;
			keyMapping.keyDescription = Description;
			ScriptableSingleton<InputManager>.Instance.registerKeyMapping(keyMapping);
			return keyMapping;
        }

        // Private variables
        Harmony rotationHarmony;
    }
}
