﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Windows.Markup;
using System.IO;
using MiniJSON;
using Mono.Security.Authenticode;
using System.Net.Sockets;

namespace TransformAnarchy
{
    public class TA : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "1.3";
        public override string getIdentifier() => "com.parkitectCommunity.TA";
        public override string getName() => "Transform Anarchy";
        public override string getDescription() => @"Adds an advanced building gizmo for select building types.";

        public override string getVersionNumber() => VERSION_NUMBER;
        public override bool isMultiplayerModeCompatible() => true;
        public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        private KeybindManager _keys;
        private string _modPath;
        public string _taSettingsFilePath;

        // TA Settings, these are loaded in when OnSettingsOpened is called
        private string gizmoSizeString;
        private string rotationAngleString;

        public static TASettingsData TASettings;
        public static TAController MainController;
        public static TA Instance;
        private Harmony _harmony;

        public static GameObject ArrowGO;
        public static GameObject RingGO;
        public static GameObject UiHolder;

        public static Sprite MoveSprite;
        public static Sprite RotateSprite;
        public static Sprite LocalSprite;
        public static Sprite GlobalSprite;
        public static Sprite OriginMoveSprite;
        public static Sprite TickSprite;

        public TA()
        {
            // Register hotkeys in advance so you can edit them from the main menu
            RegisterHotkeys();
        }

        public override void onEnabled()
        {
            Debug.LogWarning("TA: Loading Transform Anarchy");

            Instance = this;

            _modPath = ModManager.Instance.getMod(this.getIdentifier()).path;
            _taSettingsFilePath = System.IO.Path.Combine(ModManager.Instance.getMod(this.getIdentifier()).path, "ta_settings.json");

            // Load TA settings
            LoadTASettingsFromFile();

            _harmony = new Harmony(getIdentifier());

            // Load Asset Bundles
            var loadedAB = AssetBundle.LoadFromFile(_modPath + "\\Res\\ta_assets");

            // Load from Asset Bundles
            Debug.Log("TA: Loading assetbundle stuff:");

            // Gizmo meshes
            ArrowGO = loadedAB.LoadAsset<GameObject>("assets/arrowgizmo.prefab");
            RingGO = loadedAB.LoadAsset<GameObject>("assets/ringgizmo.prefab");

            // Button prefab
            UiHolder = loadedAB.LoadAsset<GameObject>("assets/ta_uiholder.prefab");

            // Sprites
            MoveSprite = loadedAB.LoadAsset<Sprite>("assets/ui_icon_move.png");
            RotateSprite = loadedAB.LoadAsset<Sprite>("assets/ui_icon_rotation.png");
            LocalSprite = loadedAB.LoadAsset<Sprite>("assets/ui_icon_local.png");
            GlobalSprite = loadedAB.LoadAsset<Sprite>("assets/ui_icon_global.png");
            OriginMoveSprite = loadedAB.LoadAsset<Sprite>("assets/ui_icon_pivot.png");
            TickSprite = loadedAB.LoadAsset<Sprite>("assets/ui_icon_build.png");

            loadedAB.Unload(false);
            Debug.Log("TA: Loaded assetbundle!");

            // Actually loading TA
            Debug.Log("TA: Initing Main Transform Anarchy handler");
            GameObject go = new GameObject();
            go.name = "TA Main";
            MainController = go.AddComponent<TAController>();

            Debug.Log("TA: Harmony patch coming");
            _harmony.PatchAll();

        }

        public override void onDisabled()
        {
            UnregisterHotkeys();
            UnityEngine.Object.Destroy(MainController.gameObject);

            if (_harmony != null)
            {
                _harmony.UnpatchAll(getIdentifier());
            }
        }

        public void onDrawSettingsUI()
        {

            // GUI settings style
            GUIStyle guistyleTextLeft = new GUIStyle(GUI.skin.label);
            guistyleTextLeft.margin = new RectOffset(10, 10, 10, 0);
            guistyleTextLeft.alignment = TextAnchor.MiddleLeft;

            GUIStyle guistyleTextMiddle = new GUIStyle(GUI.skin.label);
            guistyleTextMiddle.margin = new RectOffset(0, 10, 10, 0);
            guistyleTextMiddle.alignment = TextAnchor.MiddleCenter;

            GUIStyle guistyleField = new GUIStyle(GUI.skin.textField);
            guistyleField.margin = new RectOffset(0, 10, 10, 0);
            guistyleField.alignment = TextAnchor.MiddleCenter;

            GUIStyle guistyleButton = new GUIStyle(GUI.skin.button);
            guistyleButton.margin = new RectOffset(0, 10, 10, 0);
            guistyleButton.alignment = TextAnchor.MiddleCenter;

            // GUI settings layout
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Version", guistyleTextLeft, GUILayout.Width(175));
            GUILayout.Label(VERSION_NUMBER, guistyleTextMiddle);
            if (GUILayout.Button("Advanced settings", guistyleButton, GUILayout.Width(125)))
            {
                TASettings.showAdvancedSettings = !TASettings.showAdvancedSettings;
                onDrawSettingsUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Default rotation angle", guistyleTextLeft, GUILayout.Width(175));
            rotationAngleString = GUILayout.TextField(rotationAngleString, 7, guistyleField);
            GUILayout.EndHorizontal();

            if (TASettings.showAdvancedSettings == true)
            {
                GUILayout.Space(30);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gizmo scale based on", guistyleTextLeft, GUILayout.Width(175));
                string[] gizmoStyleString = { "Fixed size", "Screen size", "Object size" };
                TASettings.gizmoStyle = GUILayout.SelectionGrid(TASettings.gizmoStyle, gizmoStyleString, 3, guistyleButton);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gizmo scale", guistyleTextLeft, GUILayout.Width(175));
                gizmoSizeString = GUILayout.TextField(gizmoSizeString, 7, guistyleField);
                GUILayout.EndHorizontal();

                GUILayout.Space(30);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Pipette tool behaviour", guistyleTextLeft, GUILayout.Width(175));
                string[] holdPipetteButtonString = { "Always show gimzo", "Show gizmo if key is held" };
                TASettings.useButtonForPipette = GUILayout.SelectionGrid(TASettings.useButtonForPipette, holdPipetteButtonString, 2, guistyleButton);
                GUILayout.EndHorizontal();

                GUILayout.Space(30);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gizmo render behaviour", guistyleTextLeft, GUILayout.Width(175));
                string[] gizmoRenderBehaviourString = { "Render gizmo behind object", "Don't render behind object" };
                TASettings.gizmoRenderBehaviourString = GUILayout.SelectionGrid(TASettings.gizmoRenderBehaviourString, gizmoRenderBehaviourString, 2, guistyleButton);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(50);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Vertical rotation on blueprints and flatrides is not supported", guistyleTextMiddle);
            GUILayout.EndHorizontal();

            // Check the values when enter is pressed
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                // Try to convert the input text to a float
                if (float.TryParse(gizmoSizeString, out float result))
                {
                    // If successful and size > 0.01 update the value in the TASettingsData class
                    if (result >= 0.1 && result <= 5)
                    {
                        TASettings.gizmoSize = result;
                    }
                }
                if (float.TryParse(rotationAngleString, out float result2))
                {
                    // If successful and size > 0.01 update the value in the TASettingsData class
                    if (result >= 0.01 && result <= 180)
                    {
                        TASettings.rotationAngle = result2;
                    }
                }

                // Clear the focus from the TextField
                GUI.FocusControl(null);
            }

            GUILayout.EndVertical();
        }
        public void onSettingsOpened()
        {
            // Get settings file path
            _taSettingsFilePath = System.IO.Path.Combine(ModManager.Instance.getMod(this.getIdentifier()).path, "ta_settings.json");

            // Load TA settings
            LoadTASettingsFromFile();
        }

        public void onSettingsClosed()
        {
            // Save TA settings
            SaveTASettingsToFile();

            // Update the gizmo size value in the settings UI
            GUI.FocusControl(null);

            //Reload the gizmos
            MainController.SetGizmoCamera();
        }

        public void RegisterHotkeys()
        {
            _keys = new KeybindManager("TA_KEYS", "Transform Anarchy");

            _keys.AddKeybind("togglePivotEdit", "Toggle Pivot Offset", "Toggles whether the pivot or the object will move.", UnityEngine.KeyCode.U);
            _keys.AddKeybind("cancelPivotEdit", "Reset Pivot", "Will reset the pivot to the default for the object.", UnityEngine.KeyCode.L);
            _keys.AddKeybind("resetGizmoTool", "Reset Rotation", "Will reset the rotation to the default for the object.", UnityEngine.KeyCode.L);
            _keys.AddKeybind("toggleGizmoSpace", "Toggle Gizmo Space", "Toggles the space the gizmo operates in, either local or global.", UnityEngine.KeyCode.Y);
            _keys.AddKeybind("toggleGizmoTool", "Toggle Gizmo Tool", "Toggles the gizmo, either positional or rotational.", UnityEngine.KeyCode.R);
            _keys.AddKeybind("toggleGizmoOn", "Toggle Placement Mode", "Toggles whether to use the gizmo or just the normal game logic.", UnityEngine.KeyCode.Z);
            _keys.AddKeybind("usePipetteGizmo", "Pipette Gizmo", "Enables gizmo automatically when pipette is used and this button is held.", UnityEngine.KeyCode.LeftAlt);
            _keys.AddKeybind("buildObject", "Build object", "Press this key to place down the object when using the gizmo.", UnityEngine.KeyCode.T);
            _keys.RegisterAll();

        }

        public void UnregisterHotkeys()
        {
            _keys.UnregisterAll();
        }

        // Load or create TA settings file
        private void LoadTASettingsFromFile()
        {
            if (File.Exists(_taSettingsFilePath))
            {
                // Load existing settings from JSON file
                Debug.Log("TA: Loading Transform Anarchy settings from file");
                string json = File.ReadAllText(_taSettingsFilePath);
                TASettings = JsonUtility.FromJson<TASettingsData>(json);
            }
            else
            {
                // Create new settings with default values
                Debug.Log("TA: Transform Anarchy settings file not found, creating new one");
                TASettings = new TASettingsData();

                // Load default values from TA Settings class
                SaveTASettingsToFile();
            }

            // Load settings from TaSettings class to this script
            gizmoSizeString = TASettings.gizmoSize.ToString();
            rotationAngleString = TASettings.rotationAngle.ToString();
        }

        // Save values to TA settings file
        private void SaveTASettingsToFile()
        {
            Debug.Log("TA: Saving Transform Anarchy settings to file");
            // Convert TA settings data to JSON format
            string json = JsonUtility.ToJson(TASettings, true);

            // Save JSON data to the TA settings file
            File.WriteAllText(_taSettingsFilePath, json);
        }
    }
}
