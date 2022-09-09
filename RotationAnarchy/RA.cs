namespace RotationAnarchy
{
    using UnityEngine;
    using RotationAnarchy.Internal;
    using System.Reflection;

    public class RA : ModBase<RA>, IModSettings
    {
        public override string AUTHOR => "parkitectCommunity";
        public override string MODKEY => "rotationAnarchy";
        public override string getName() => "Rotation Anarchy";
        public override string getDescription() => "Adds a more advanced rotation gizmo for Deco and Flatrides.";
        protected override string SettingsInfoString => "Settings";
        protected override Assembly AssemblyGetter => Assembly.GetExecutingAssembly();

        // Prefs values -------------------------------------------------------
        public static PrefsBool ActiveOnLoad { get; private set; }
        public static PrefsFloatSnapped RotationAngle { get; private set; }
        public static PrefsFloat GizmoWidthMin { get; private set; }
        public static PrefsFloat GizmoWidthMax { get; private set; }

        // Hotkeys      -------------------------------------------------------

        /// <summary>
        /// When set to false mod should not interfere in any way with rotation,
        /// if deactivated mid rotation, should also reset the rotated deco to default, if in placement mode.
        /// </summary>
        public static BaseHotkey SelectObjectHotkey { get; private set; }
        public static BaseHotkey RAActiveHotkey { get; private set; }
        public static BaseHotkey DirectionHotkey { get; private set; }
        public static BaseHotkey LocalRotationHotkey { get; private set; }
        public static BaseHotkey DragRotationHotkey { get; private set; }
        public static BaseHotkey AngleSnapHotkey { get; private set; }

        // Features    -------------------------------------------------------
        public static RAController Controller { get; private set; }
        public static TrackballController TrackballController { get; private set; }

        // Params      -------------------------------------------------------
        public static Color SelectedBuildableHighlightColor { get; private set; } = new Color32(0, 162, 232, 255);

        public static GizmoColors GizmoColors { get; private set; } = new GizmoColors()
        {
            X = new GizmoAxisColorBlock() { color = new Color32(214, 72, 56, 255), outlineColor = new Color32(113, 32, 23, 255) },
            Y = new GizmoAxisColorBlock() { color = new Color32(55, 213, 47, 255), outlineColor = new Color32(0, 89, 0, 255) },
            Z = new GizmoAxisColorBlock() { color = new Color32(75, 156, 205, 255), outlineColor = new Color32(29, 75, 103, 255) }
        };

        public static GizmoOffsets GizmoOffsets { get; private set; } = new GizmoOffsets()
        {
            X = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = new Quaternion() },
            Y = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0.1f, 0), rotationOffset = Quaternion.LookRotation(Vector3.up) },
            Z = new GizmoOffsetsBlock() { positionOffset = new Vector3(0, 0, 0), rotationOffset = Quaternion.LookRotation(Vector3.forward) },
        };

        public static float GizmoParamLerp => 20f;


        protected override void OnModEnabled()
        {
            base.OnModEnabled();

            // Version registration      -------------------------------------------------------
            NewVersion("1.0", "Phase 1 of Rotation Anarchy");

            // Prefs values registration -------------------------------------------------------
            RegisterAndLoadPrefsValue(ActiveOnLoad = new PrefsBool("activeOnLoad", true, "Active On Load"));
            RegisterAndLoadPrefsValue(RotationAngle = new PrefsFloatSnapped("rotationAngle", 45f, 0f, 90f, 22.5f, "Rotation Snap Angle"));
            RegisterAndLoadPrefsValue(GizmoWidthMin = new PrefsFloat("gizmoWidthMin", 0.05f, 0.025f, 0.1f, "Gizmo Width Min",formatting: "0.000"));
            RegisterAndLoadPrefsValue(GizmoWidthMax = new PrefsFloat("gizmoWidthMax", 0.25f, 0.5f, 0.15f, "Gizmo Width Max", formatting: "0.000"));

            // Hotkeys registration      -------------------------------------------------------
            SelectObjectHotkey = NewHotkey("selectObject", "Select Object", "Selects an object to edit", KeyCode.R);
            RAActiveHotkey = NewHotkey("active", "Toggle RA", "Toggle Rotation Anarchy active, without disabling it.", KeyCode.Y);
            DirectionHotkey = NewHotkey("direction", "Rotation direction", "Change the rotation axes from horizontal to vertical", KeyCode.LeftControl);
            LocalRotationHotkey = NewHotkey("localSpace", "Local space", "Change the rotation axes from local space (object axes) to global space (world axes)", KeyCode.CapsLock);
            DragRotationHotkey = NewHotkey("dragRotation", "Drag Rotation", "Toggle to a Trackball-Style dragging mode, use Height-Change Key to lock Axis", KeyCode.Keypad3);
            AngleSnapHotkey = NewHotkey("angleSnap", "Angle Snapping", "Toggle angle snapping for drag rotation", KeyCode.Keypad1);

            // Mod changes registration --------------------------------------------------------
            RegisterComponent(Controller = new RAController());
            RegisterComponent(TrackballController = new TrackballController());
            RegisterComponent(new ChangeUIBackgroundGraphics());
            RegisterComponent(new ConstructWindowToggle());
            RegisterComponent(new RADebug());
            RegisterComponent(new GizmoController());
            RegisterComponent(new RAWorldSpaceText());
        }

        protected override void OnModUpdate()
        {
            base.OnModUpdate();

            if (Input.GetKeyDown(KeyCode.KeypadPeriod))
                DebugGUI.SetActive(!DebugGUI.IsActive);
        }
    }
}
