using System;
using System.Collections.Generic;
using System.Linq;
using Parkitect;
using UnityEngine;
using HarmonyLib;
using System.Windows.Markup;
using System.IO;
using MiniJSON;
using Mono.Security.Authenticode;

namespace TransformAnarchy
{
    public class TA : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "1.2";
        public override string getIdentifier() => "com.parkitectCommunity.TA";
        public override string getName() => "Transform Anarchy Experimental";
        public override string getDescription() => @"Adds an advanced building gizmo for select building types.
EXPERIMENTAL VERSION

Features:
    Positional Gizmo
    Rotational Gizmo
    Support for:
        Deco
        Blueprints*
        Flatrides**
    Local and Global modes for both gizmos
    Gizmos can always be clicked, even through any objects.

Notes:
* (Z axis is not supported. Rotate on global Y for best results)
** (will snap to grid, rotate on global Y for best results)
        ";

        public override string getVersionNumber() => VERSION_NUMBER;
        public override bool isMultiplayerModeCompatible() => true;
        public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        private KeybindManager _keys;
        private string _modPath;
        public string _settingsFilePath;

        // TA Settings, these are loaded in when OnSettingsOpened is called
        private string gizmoSizeString;

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

        public override void onEnabled()
        {

            Instance = this;

            Debug.LogWarning("Loading TA");

            _harmony = new Harmony(getIdentifier());

            RegisterHotkeys();
            _modPath = ModManager.Instance.getMod(this.getIdentifier()).path;

            var loadedAB = AssetBundle.LoadFromFile(_modPath + "\\Res\\ta_assets");

            // Load from Asset Bundles
            Debug.Log("Loading assetbundle stuff:");

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
            Debug.Log("Loaded assetbundle!");

            Debug.Log("Initing Main RA handler");
            GameObject go = new GameObject();
            go.name = "TA Main";
            MainController = go.AddComponent<TAController>();

            Debug.Log("Harmony patch coming!");
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
            GUILayout.Label("Version", guistyleTextLeft, GUILayout.Width(200));
            GUILayout.Label(VERSION_NUMBER, guistyleTextMiddle);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Gizmo style based on", guistyleTextLeft, GUILayout.Width(200));
            string[] gizmoStyleStrings = { "Fixed size", "Screen size", "Object size" };
            TASettings.gizmoStyle = GUILayout.SelectionGrid(TASettings.gizmoStyle, gizmoStyleStrings, 3, guistyleButton);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Gizmo scale", guistyleTextLeft, GUILayout.Width(200));
            gizmoSizeString = GUILayout.TextField(gizmoSizeString, 5, guistyleField);
            GUILayout.EndHorizontal();


            // Check the values when enter is pressed
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                // Try to convert the input text to a float
                if (float.TryParse(gizmoSizeString, out float result))
                {
                    // If successful and size > 0.01 update the value in the TASettingsData class
                    if (result > 0.1 && result < 5)
                    {
                        TASettings.gizmoSize = result;
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
            _settingsFilePath = System.IO.Path.Combine(ModManager.Instance.getMod(this.getIdentifier()).path, "ta_settings.json");

            // Load TA settings
            LoadTASettingsFromFile();
        }

        public void onSettingsClosed()
        {
            // Save TA settings
            SaveTASettingsToFile();

            // Update the gizmo size value in the settings UI
            GUI.FocusControl(null);
        }

        public void RegisterHotkeys()
        {
            _keys = new KeybindManager("TA_KEYS", "Transform Anarchy");

            _keys.AddKeybind("cancelPivotEdit", "Reset Pivot", "Will reset the pivot to the default for the object.", UnityEngine.KeyCode.Alpha5);
            _keys.AddKeybind("togglePivotEdit", "Toggle Pivot Offset", "Toggles whether the pivot or the object will move.", UnityEngine.KeyCode.Alpha6);
            _keys.AddKeybind("toggleGizmoSpace", "Toggle Gizmo Space", "Toggles the space the gizmo operates in, either local or global.", UnityEngine.KeyCode.Alpha7);
            _keys.AddKeybind("toggleGizmoTool", "Toggle Gizmo Tool", "Toggles the gizmo, either positional or rotational.", UnityEngine.KeyCode.Alpha8);
            _keys.AddKeybind("toggleGizmoOn", "Toggle Placement Mode", "Toggles whether to use the advanced gizmos or just the normal game logic.", UnityEngine.KeyCode.Alpha9);
            _keys.AddKeybind("usePipetteGizmo", "Pipette Gizmo", "Enables gizmo automatically when pipette is used and this button is held.", UnityEngine.KeyCode.LeftAlt);
            _keys.RegisterAll();

        }

        public void UnregisterHotkeys()
        {
            _keys.UnregisterAll();
        }

        // Load or create TA settings file
        private void LoadTASettingsFromFile()
        {
            if (File.Exists(_settingsFilePath))
            {
                // Load existing settings from JSON file
                Debug.Log("Loading TA settings from file");
                string json = File.ReadAllText(_settingsFilePath);
                TASettings = JsonUtility.FromJson<TASettingsData>(json);
            }
            else
            {
                // Create new settings with default values
                Debug.Log("TA settings file not found");
                Debug.Log("Creating TA settings file");
                TASettings = new TASettingsData();

                // Load default values from TA Settings class
                SaveTASettingsToFile();
            }

            // Load settings from TaSettings class to this script
            gizmoSizeString = TASettings.gizmoSize.ToString();
        }

        // Save values to TA settings file
        private void SaveTASettingsToFile()
        {
            Debug.Log("Saving TA settings");
            // Convert TA settings data to JSON format
            string json = JsonUtility.ToJson(TASettings, true);

            // Save JSON data to the TA settings file
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
