using System;
using System.Collections.Generic;
using System.Linq;
using Parkitect;
using UnityEngine;
using HarmonyLib;

namespace RotationAnarchyEvolved
{
    public class RAE : AbstractMod
    {

        public const string VERSION_NUMBER = "1.0";
        public override string getIdentifier() => "com.parkitectCommunity.RAE";
        public override string getName() => "Rotation Anarchy Evolved";
        public override string getDescription() => "";
        public override string getVersionNumber() => VERSION_NUMBER;
        public override bool isMultiplayerModeCompatible() => true;

        private KeybindManager _keys;
        private string _modPath;

        public static RAEController MainController;
        public static RAE Instance;
        private Harmony _harmony;

        public static GameObject ArrowGO;
        public static GameObject RingGO;

        public override void onEnabled()
        {

            Instance = this;

            Debug.LogWarning("Loading RAE");

            _harmony = new Harmony(getIdentifier());

            RegisterHotkeys();
            _modPath = ModManager.Instance.getMod(this.getIdentifier()).path;

            var loadedAB = AssetBundle.LoadFromFile(_modPath + "\\Res\\rae_mesh");

            Debug.Log("All AssetBundle objects:");

            foreach (string obj in loadedAB.GetAllAssetNames())
            {
                Debug.Log(" * " + obj);
            }

            Debug.Log("We are go for launch! Loading prefabs");

            ArrowGO = loadedAB.LoadAsset<GameObject>("assets/arrowgizmo.prefab");
            Debug.Log($"Loaded prefab ArrowGO = {ArrowGO}");

            RingGO = loadedAB.LoadAsset<GameObject>("assets/ringgizmo.prefab");
            Debug.Log($"Loaded prefab RingGO = {RingGO}");

            loadedAB.Unload(false);

            GameObject go = new GameObject();
            go.name = "RAE Main";
            MainController = go.AddComponent<RAEController>();

            Debug.Log("Harmony patch coming!");
            foreach (string str in AccessTools.GetMethodNames(typeof(Builder)))
            {
                Debug.Log(str);
            }

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
            _keys = new KeybindManager("RAE_KEYS", "Rotation Anarchy Evolved");

            _keys.AddKeybind("toggleGizmoSpace", "Toggle Gizmo Space", "Toggles the space the gizmo operates in, either local or global", UnityEngine.KeyCode.X);
            _keys.AddKeybind("toggleGizmoTool", "Toggle Gizmo Tool", "Toggles the gizmo, either positional or rotational", UnityEngine.KeyCode.Z);
            _keys.AddKeybind("toggleGizmoOn", "Toggle Placement Mode", "Toggles whether to use the advanced gizmos or just the normal game logic", UnityEngine.KeyCode.M);

            _keys.RegisterAll();

        }

        public void UnregisterHotkeys()
        {
            _keys.UnregisterAll();
        }
    }
}
