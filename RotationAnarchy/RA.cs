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
        }
    }
}
