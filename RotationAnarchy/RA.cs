namespace RotationAnarchy
{
    using UnityEngine;
    using RotationAnarchy.Internal;

    public class RA : ModBase<RA>, IModSettings
    {
        public override string AUTHOR => "parkitectCommunity";
        public override string MODKEY => "rotationAnarchy";
        public override string getName() => "Rotation Anarchy";
        public override string getDescription() => "You know what it is.";
        protected override string SettingsInfoString => "Howdy, pardner.";

        // Prefs values -------------------------------------------------------
        public static PrefsBool ActiveOnLoad { get; private set; }
        public static PrefsFloatSnapped RotationAngle { get; private set; }
        public static PrefsFloat PlacementGizmoWidth { get; private set; }
        public static PrefsFloat GizmoWidth { get; private set; }

        // Hotkeys      -------------------------------------------------------

        /// <summary>
        /// When set to false mod should not interfere in any way with rotation,
        /// if deactivated mid rotation, should also reset the rotated deco to default, if in placement mode.
        /// </summary>
        public static BaseHotkey RAActiveHotkey { get; private set; }
        public static BaseHotkey DirectionHotkey { get; private set; }
        public static BaseHotkey LocalRotationHotkey { get; private set; }

        // Features    -------------------------------------------------------
        public static RAController Controller { get; private set; }

        // Params      -------------------------------------------------------

        public static GizmoColors GizmoColors { get; private set; } = new GizmoColors()
        {
            X = new GizmoAxisColorBlock() { color = new Color32(214, 72, 56, 255), outlineColor = new Color32(113, 32, 23, 255) },
            Y = new GizmoAxisColorBlock() { color = new Color32(55, 213, 47, 255), outlineColor = new Color32(0, 89, 0, 255) },
            Z = new GizmoAxisColorBlock() { color = new Color32(75, 156, 205, 255), outlineColor = new Color32(29, 75, 103, 255) }
        };

        public static GizmoOffsets GizmoOffsets { get; private set; } = new GizmoOffsets()
        {
            X = new GizmoOffsetsBlock() { localPositionOffset = new Vector3(0, 0, 0), localRotationOffset = new Quaternion() },
            Y = new GizmoOffsetsBlock() { localPositionOffset = new Vector3(0, 0.1f, 0), localRotationOffset = Quaternion.LookRotation(Vector3.up) },
            Z = new GizmoOffsetsBlock() { localPositionOffset = new Vector3(0, 0, 0), localRotationOffset = Quaternion.LookRotation(Vector3.forward) },
        };

        public static float GizmoParamLerp => 15f;

        protected override void OnModEnabled()
        {
            base.OnModEnabled();

            // Version registration      -------------------------------------------------------
            NewVersion("0.1", "Baseline");

            // Prefs values registration -------------------------------------------------------
            RegisterAndLoadPrefsValue(ActiveOnLoad = new PrefsBool("activeOnLoad", true, "Active On Load"));
            RegisterAndLoadPrefsValue(RotationAngle = new PrefsFloatSnapped("rotationAngle", 45f, 0f, 90f, 22.5f, "Rotation Angle"));
            RegisterAndLoadPrefsValue(PlacementGizmoWidth = new PrefsFloat("placementGizmoWidth", 1f, 0.1f, 2f, "Builder Gizmo Width"));
            RegisterAndLoadPrefsValue(GizmoWidth = new PrefsFloat("gizmoWidth", 1f, 0.1f, 2f, "Gizmo Width"));

            // Hotkeys registration      -------------------------------------------------------
            RAActiveHotkey = NewHotkey("active", "Toggle RA", "Toggle Rotation Anarchy active, without disabling it.", KeyCode.Y);
            DirectionHotkey = NewHotkey("direction", "Rotation direction", "Change the rotation axes from horizontal to vertical", KeyCode.LeftShift);
            LocalRotationHotkey = NewHotkey("localSpace", "Local space", "Change the rotation axes from local space (object axes) to global space (world axes)", KeyCode.CapsLock);

            // Mod changes registration --------------------------------------------------------
            RegisterComponent(Controller = new RAController());
            RegisterComponent(new ChangeUIBackgroundGraphics());
            RegisterComponent(new ConstructWindowToggle());
            RegisterComponent(new RADebug());
            RegisterComponent(new GizmoController());
            RegisterComponent(new RAWorldSpaceText());
        }
    }
}
