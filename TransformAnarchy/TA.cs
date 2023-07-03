﻿using System;
using System.Collections.Generic;
using System.Linq;
using Parkitect;
using UnityEngine;
using HarmonyLib;

namespace TransformAnarchy
{
    public class TA : AbstractMod
    {

        public const string VERSION_NUMBER = "1.1";
        public override string getIdentifier() => "com.parkitectCommunity.TA";
        public override string getName() => "Transform Anarchy";
        public override string getDescription() => @"Adds an advanced building gizmo for select building types.
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

        public void RegisterHotkeys()
        {
            _keys = new KeybindManager("TA_KEYS", "Transform Anarchy");

            _keys.AddKeybind("cancelPivotEdit", "Reset Pivot", "Will reset the pivot to the default for the object.", UnityEngine.KeyCode.Alpha5);
            _keys.AddKeybind("togglePivotEdit", "Toggle Pivot Offset", "Toggles whether the pivot or the object will move.", UnityEngine.KeyCode.Alpha6);
            _keys.AddKeybind("toggleGizmoSpace", "Toggle Gizmo Space", "Toggles the space the gizmo operates in, either local or global.", UnityEngine.KeyCode.Alpha7);
            _keys.AddKeybind("toggleGizmoTool", "Toggle Gizmo Tool", "Toggles the gizmo, either positional or rotational.", UnityEngine.KeyCode.Alpha8);
            _keys.AddKeybind("toggleGizmoOn", "Toggle Placement Mode", "Toggles whether to use the advanced gizmos or just the normal game logic.", UnityEngine.KeyCode.Alpha9);
            _keys.RegisterAll();

        }

        public void UnregisterHotkeys()
        {
            _keys.UnregisterAll();
        }
    }
}
