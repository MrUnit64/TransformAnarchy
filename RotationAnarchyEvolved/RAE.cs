using System;
using System.Collections.Generic;
using System.Linq;
using Parkitect;
using UnityEngine;

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

        public override void onEnabled()
        {

            Debug.LogWarning("Loading RAE");

            RegisterHotkeys();
            _modPath = ModManager.Instance.getMod(this.getIdentifier()).path;

            GameObject go = new GameObject();
            go.name = "RAE Main";
            MainController = go.AddComponent<RAEController>();

            var loadedAB = AssetBundle.LoadFromFile(_modPath + "\\Res\\rae_mesh");

            Debug.Log("All AssetBundle objects:");

            foreach (string obj in loadedAB.GetAllAssetNames())
            {
                Debug.Log(" * " + obj);
            }

            Debug.Log("We are go for launch! Loading prefabs");

            MainController.ArrowGO = loadedAB.LoadAsset<GameObject>("assets/arrowgizmo.prefab");
            Debug.Log($"Loaded prefab ArrowGO = {MainController.ArrowGO}");

            MainController.RingGO = loadedAB.LoadAsset<GameObject>("assets/ringgizmo.prefab");
            Debug.Log($"Loaded prefab RingGO = {MainController.RingGO}");

            loadedAB.Unload(false);

        }

        public override void onDisabled()
        {
            UnregisterHotkeys();
            UnityEngine.Object.Destroy(MainController.gameObject);
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
