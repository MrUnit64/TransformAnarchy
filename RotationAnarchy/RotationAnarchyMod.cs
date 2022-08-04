namespace RotationAnarchy
{
    using UnityEngine;
    using RotationAnarchy.Internal;

    public class RotationAnarchyMod : ModBase<RotationAnarchyMod>, IModSettings
    {
        public override string AUTHOR => "parkitectCommunity";
        public override string MODKEY => "rotationAnarchy";
        public override string getName() => "Rotation Anarchy";
        public override string getDescription() => "You know what it is.";
        protected override string SettingsInfoString => "Howdy, pardner.";

        // Prefs values -------------------------------------------------------
        public static PrefsFloat RotationAngle { get; private set; }
        // Hotkeys      -------------------------------------------------------
        public static BaseHotkey Direction { get; private set; }
        public static BaseHotkey DoLocalRotation { get; private set; }

        protected override void OnModEnabled()
        {
            base.OnModEnabled();

            // Version registration      -------------------------------------------------------
            NewVersion("0.1", "Baseline");

            // Prefs values registration -------------------------------------------------------
            //TODO: replace with functional snap angle ui
            RegisterAndLoadPrefsValue(RotationAngle = new PrefsFloat("rotationAngle", 90, 0, 360, "Rotation Angle"));

            // Hotkeys registration      -------------------------------------------------------
            Direction = NewHotkey("direction", "Rotation direction", "Change the rotation axes from horizontal to vertical", KeyCode.LeftShift);
            DoLocalRotation = NewHotkey("localSpace", "Local space", "Change the rotation axes from local space (object axes) to global space (world axes)", KeyCode.CapsLock);

            // Mod changes registration --------------------------------------------------------
            RegisterChange(new HUDBackgroundGraphicResizer());
            RegisterChange(new HUDGizmoButtonCreator());
        }
    }
}
