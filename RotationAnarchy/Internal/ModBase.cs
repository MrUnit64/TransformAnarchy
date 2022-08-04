namespace RotationAnarchy.Internal
{
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Generic version of the base mod, with singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ModBase<T> : ModBase where T : ModBase<T>
    {
        public static T Instance { get; private set; }

        protected sealed override void OnEarlyInit()
        {
            base.OnEarlyInit();
            Instance = this as T;
        }
    }

    /// <summary>
    /// Purpose of the base mod is to abstract common work, like saving preferences, patching/unpatching harmony, etc.
    /// 
    /// Full feature list:
    /// 
    /// - Version - simple version tracking.
    /// - Log - similar to Debug.Log, but appends the mod name at the front.
    /// - Automatic settings drawing, loading/saving and events, through PrefsValue class.
    /// - Automatic root gameObject creation
    /// - Simple loading of textures
    /// - 
    /// 
    /// </summary>
    public abstract class ModBase : AbstractMod, IModSettings
    {
        /// <summary>
        /// Represents one game version.
        /// List of these is displayed in settings.
        /// </summary>
        public class Version
        {
            /// <summary>
            /// Version number
            /// </summary>
            public string number { get; set; }
            /// <summary>
            /// Version description
            /// </summary>
            public string info { get; set; }

            public Version(string number, string info)
            {
                this.number = number;
                this.info = info;
            }
        }

        /// <summary>
        /// Mod "key" used everywhere as path i.e. "{AUTHOR}.{MODKEY}" 
        /// $"{AUTHOR}.{MODKEY}.enableRotation" ---> "baboon.mod.enableRotation"
        /// </summary>
        public abstract string AUTHOR { get; }
        /// <summary>
        /// Mod "key" used everywhere as path i.e. "{AUTHOR}.{MODKEY}" 
        /// $"{AUTHOR}.{MODKEY}.enableRotation" ---> "baboon.mod.enableRotation"
        /// </summary>
        public abstract string MODKEY { get; }
        public abstract override string getDescription();
        /// <summary>
        /// The string that is displayed in settings window, for additional mod instructions or info.
        /// </summary>
        protected abstract string SettingsInfoString { get; }

        /// <summary>
        /// Key for saving into playerprefs
        /// </summary>
        public string PREFSKEY { get; private set; }
        /// <summary>
        /// Key for identifying harmony instance
        /// </summary>
        public string HARMONYKEY { get; private set; }
        /// <summary>
        /// Key for printing into log
        /// </summary>
        public string DEBUGKEY { get; private set; }
        /// <summary>
        /// Absolute mod path on runtime. Used for loading resources.
        /// </summary>
        public string MODPATH { get; private set; }
        /// <summary>
        /// Key appended to Hotkeys to identify them
        /// </summary>
        public string HOTKEY { get; private set; }

        /// <summary>
        /// When disabled the mod wont print any logs.
        /// </summary>
        public PrefsBool EnableLogging { get; private set; } = new PrefsBool("enableLogging", false, "Enable Logging");
        /// <summary>
        /// When enabled the logs in console will be colored. Useful when you are using BepinEx + UnityEdit
        /// </summary>
        public PrefsBool ColoredLogs { get; private set; } = new PrefsBool("coloredLogs", false, "Use colors in logs");
        /// <summary>
        /// The root game object for the mod
        /// </summary>
        public GameObject GameObject { get; private set; }
        /// <summary>
        /// List of versions
        /// </summary>
        public List<Version> Versions { get; private set; } = new List<Version>();
        /// <summary>
        /// Indicates that the mod is in the process of being disabled
        /// </summary>
        public bool DisablingMod { get; private set; }

        public override string getName() => AUTHOR + "@" + MODKEY;
        public override string getIdentifier() => AUTHOR + "::" + MODKEY;
        public override sealed string getVersionNumber() => GetLatestVersion().number;

        /// <summary>
        /// Default version used when no other versions are registered
        /// </summary>
        private Version noVersion = new Version("0.0", "No version.");
        /// <summary>
        /// All registered prefsValues
        /// </summary>
        private List<PrefsValue> prefsValues = new List<PrefsValue>();
        /// <summary>
        /// All registered hotkeys
        /// </summary>
        private List<Hotkey> hotkeys = new List<Hotkey>();
        /// <summary>
        /// All registered textures
        /// </summary>
        private List<Texture2D> createdTextures = new List<Texture2D>();
        /// <summary>
        /// All registered sprites
        /// </summary>
        private List<Sprite> createdSprites = new List<Sprite>();
        /// <summary>
        /// All registered sprites
        /// </summary>
        private Dictionary<Type, ModChange> changes = new Dictionary<Type, ModChange>();
        /// <summary>
        /// Mod hotkey group
        /// </summary>
        private KeyGroup keyGroup;
        /// <summary>
        /// Mod harmony instance
        /// </summary>
        protected Harmony harmony;

        /// <summary>
        /// Are we drawing version list in the settings menu?
        /// </summary>
        private bool gui_drawingVersions;
        /// <summary>
        /// Scroll position for settings window
        /// </summary>
        private Vector2 gui_scrollViewPos;

        /// <summary>
        /// This method is a replacement for onEnabled()
        /// </summary>
        protected virtual void OnModEnabled()
        {
        }

        /// <summary>
        /// This method is a replacement for onDisabled()
        /// </summary>
        protected virtual void OnModDisabled()
        {
        }

        /// <summary>
        /// Called when the mod is unloading textures/sprites and other "heavy" resources.
        /// </summary>
        protected virtual void OnUnloadResources()
        {
        }

        /// <summary>
        /// Substitute for Debug.Log
        /// - Only prints when EnableLogging is true
        /// - Prints with mod name before it
        /// </summary>
        /// <param name="msg">actual log message</param>
        /// <param name="from">the object name where this log originated from</param>
        public void LOG(string msg, string from = null)
        {
            if (EnableLogging.Value)
            {
                if (ColoredLogs.Value)
                {
                    if (from != null)
                        UnityEngine.Debug.Log("<color=yellow>" + DEBUGKEY + $"< {from} > " + msg + "</color>");
                    else
                        UnityEngine.Debug.Log("<color=yellow>" + DEBUGKEY + msg + "</color>");
                }
                else
                {
                    if (from != null)
                        UnityEngine.Debug.Log(DEBUGKEY + $"< {from} > " + msg);
                    else
                        UnityEngine.Debug.Log(DEBUGKEY + msg);
                }

            }
        }

        /// <summary>
        /// Substitute for Debug.LogError
        /// - Only prints when EnableLogging is true
        /// - Prints with mod name before it
        /// </summary>
        /// <param name="msg">actual log message</param>
        /// <param name="from">the object name where this log originated from</param>
        public void ERROR(string msg, string from = null)
        {
            if (EnableLogging.Value)
            {
                if (ColoredLogs.Value)
                {
                    if (from != null)
                        UnityEngine.Debug.LogError("<color=red>" + DEBUGKEY + $"< {from} > " + msg + "</color>");
                    else
                        UnityEngine.Debug.LogError("<color=red>" + DEBUGKEY + msg + "</color>");
                }
                else
                {
                    if (from != null)
                        UnityEngine.Debug.LogError(DEBUGKEY + $"< {from} > " + msg);
                    else
                        UnityEngine.Debug.LogError(DEBUGKEY + msg);
                }
            }
        }

        /// <summary>
        /// Use this to register a created PrefsValue with the mod base.
        /// </summary>
        /// <param name="value"></param>
        protected void RegisterAndLoadPrefsValue(PrefsValue value)
        {
            value.Initialize(this);
            prefsValues.Add(value);
            value.Load(false);
        }

        protected void RegisterChange(ModChange change)
        {
            try
            {
                change.OnChangeApplied();
            }
            catch(Exception e)
            {
                LogInternalError("Failed to apply change: " + change.GetType());
                throw e;
            }

            changes.Add(change.GetType(), change);
        }

        /// <summary>
        /// Use this to create a new hotkey
        /// </summary>
        /// <param name="value"></param>
        protected BaseHotkey NewHotkey(string id, string niceName, string description, KeyCode key, Hotkey.OnKeyDownHandler onKeyDown = null)
        {
            var km = NewKeyMapping(id, niceName, description, key);
            InputManager.Instance.registerKeyMapping(km);
            var hk = new BaseHotkey(km);
            HotkeyManager.Instance.registerHotkey(hk);
            hotkeys.Add(hk);
            if (onKeyDown != null)
                hk.onKeyDown += onKeyDown;
            return hk;
        }

        /// <summary>
        /// Use this to create a new version
        /// </summary>
        /// <param name="value"></param>
        protected void NewVersion(string number, string info)
        {
            Versions.Add(new Version(number, info));
        }

        public Version GetLatestVersion()
        {
            if (Versions.Count > 0)
                return Versions[Versions.Count - 1];
            else
                return noVersion;
        }

        /// <summary>
        /// Loads PNG from the mod folder by the name.
        /// Can be a relative path 
        /// Example:
        /// "img/icon"
        /// "icon_brush"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Texture2D LoadPNG(string name)
        {
            LOG("Trying to load png: " + name);
            var tex = UIUtil.LoadTexture(MODPATH, name, ".png");
            if (tex == null)
                ERROR("FAILED to load png: " + name);
            else
                createdTextures.Add(tex);
            return tex;
        }

        /// <summary>
        /// Loads PNG from the mod folder by the name and create a Sprite object out of it.
        /// Can be a relative path 
        /// Example:
        /// "img/icon"
        /// "icon_brush"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite LoadSpritePNG(string name)
        {
            LOG("Trying to load sprite png: " + name);
            var tex = UIUtil.LoadSprite(MODPATH, name, ".png");
            if (tex == null)
                ERROR("FAILED to load sprite png: " + name);
            else
                createdSprites.Add(tex);
            return tex;
        }

        public T GetChange<T>() where T : ModChange
        {
            if (changes.TryGetValue(typeof(T), out var change))
                return change as T;
            return null;
        }

        public ModChange GetChange(Type type) 
        {
            if (changes.TryGetValue(type, out var change))
                return change;
            return null;
        }

        /*  ========================================================================================================================
         *  ========================================================================================================================
         * 
         *  Internal
         * 
         * 
         *  ========================================================================================================================
         *  ========================================================================================================================
         */

        /// <summary>
        /// Used internally, before the mod is fully initialized
        /// </summary>
        protected virtual void OnEarlyInit()
        {

        }

        public sealed override void onEnabled()
        {
            base.onEnabled();
            DisablingMod = false;
            try // Create KeyGroup
            {
                keyGroup = new KeyGroup(getName()) { keyGroupName = getName() };
                InputManager.Instance.registerKeyGroup(keyGroup);
            }
            catch (Exception ex)
            {
                LogInternalError("Failed to create and register key group");
                throw ex;
            }

            try // Register default PrefValues
            {
                RegisterAndLoadPrefsValue(EnableLogging);
                RegisterAndLoadPrefsValue(ColoredLogs);
            }
            catch (Exception ex)
            {
                LogInternalError("Failed to register default prefs");
                throw ex;
            }

            try // Set various mod KEYS and path
            {
                SetInternalData(MODKEY, ModManager.Instance.getMod(this.getIdentifier()).path);
            }
            catch (Exception ex)
            {
                LogInternalError("Failed to set internal data");
                throw ex;
            }

            // Construct root game object
            GameObject = new GameObject(AUTHOR + "@" + MODKEY);

            try
            {
                OnEarlyInit();
            }
            catch (Exception ex)
            {
                LogInternalError("Failed OnEarlyInit()");
                throw ex;
            }

            try // Enable the actual mod content
            {
                OnModEnabled();
            }
            catch (Exception ex)
            {
                LogInternalError("<b>Your code</b> caused the mod to FAIL TO ENABLE!");
                throw ex;
            }

            try // Apply harmony patches
            {
                var assembly = Assembly.GetExecutingAssembly();
                LOG(" Beginning Harmony patching for: " + assembly.FullName);
                harmony = new Harmony(HARMONYKEY);
                harmony.PatchAll(assembly);
                foreach (var m in harmony.GetPatchedMethods())
                {
                    LOG(" Harmony patched method: " + m.Name + "()");
                }
            }
            catch (Exception ex)
            {
                LogInternalError("Harmony failed to apply patches");
                throw ex;
            }
        }

        public sealed override void onDisabled()
        {
            base.onDisabled();
            LOG("Trying to disable mod.");
            DisablingMod = true;

            // Instead of allowing the mod to throw exceptions on the spot, 
            // we first attempt to unload as much of mods resources as possible,
            // collecting the exceptions into a list which we will rethrow later.
            List<Exception> exceptions = null;
            try // Call actual mods OnDisabled
            {
                OnModDisabled();
            }
            catch (Exception ex)
            {
                exceptions = new List<Exception>();
                exceptions.Add(new Exception("Failed to cleanly disable mod, attempting cleanup", ex));
                LogInternalError("<b>Your code</b> caused the mod to FAIL TO DISABLE!");
            }

            ModChange change = null;
            try
            {
                var values = changes.Values.ToArray();
                for (int i = values.Length - 1; i >= 0; i--)
                {
                    change = values[i];
                    change.OnChangeReverted();
                }

                changes.Clear();
            }
            catch(Exception ex)
            {
                exceptions.Add(new Exception("Failed to revert change:" + change?.GetType().Name, ex));
            }

            try // unregister key group
            {
                InputManager.Instance.unregisterKeyGroup(keyGroup);
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception("Failed to unregister key group", ex));
            }

            try // unregister prefs values
            {
                foreach (var pref in prefsValues)
                    pref.Clear();

                prefsValues.Clear();
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception("Failed to clear PrefValues", ex));
            }

            try // unregister hotkeys
            {
                foreach (var hk in hotkeys)
                    UnregisterHotkey(hk);

                hotkeys.Clear();
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception("Failed to unregister hotkeys", ex));
                throw ex;
            }

            try // harmony unpatch
            {
                harmony.UnpatchAll();
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception("Failed to Unpatch Harmony!", ex));
            }

            try // destroy root game object
            {
                GameObject.Destroy(GameObject);
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception("Failed to destroy GameObject ", ex));
            }

            try // attempt to unload heavy resources
            {
                UnloadResources();
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception("Failed unload resources ", ex));
            }

            // "Throw" exceptions into the unity console and throw one simple exception to notify Parkitect that we failed.
            if (exceptions != null && exceptions.Count > 0)
            {
                foreach (var ex in exceptions)
                {
                    UnityEngine.Debug.LogException(ex);
                }

                throw new Exception("MOD FAILED TO DISABLE CLEANLY");
            }
        }

        /// <summary>
        /// Release heavy resources
        /// </summary>
        private void UnloadResources()
        {
            for (int i = 0; i < createdSprites.Count; i++)
            {
                if (createdSprites[i])
                    GameObject.Destroy(createdSprites[i]);
            }
            createdSprites.Clear();
            for (int i = 0; i < createdTextures.Count; i++)
            {
                if (createdTextures[i])
                    GameObject.Destroy(createdTextures[i]);
            }
            createdTextures.Clear();

            OnUnloadResources();
        }

        public virtual void onDrawSettingsUI()
        {
            UIUtil.InitStyles();

            // Begin settings window top bar
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(150));
                {
                    if (prefsValues.Count > 0)
                    {
                        if (GUILayout.Button("Reset settings"))
                        {
                            OnResetSettings();
                        }
                    }

                    UIUtil.PushColor(gui_drawingVersions ? Color.green : Color.white);
                    if (GUILayout.Button("Versions"))
                    {
                        gui_drawingVersions = !gui_drawingVersions;
                    }
                    UIUtil.PopColor();
                    GUILayout.EndVertical();
                }

                if (SettingsInfoString != null)
                    GUILayout.Label(SettingsInfoString, UIUtil.STYLE_LABEL_INFO);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(40);

            if (gui_drawingVersions)
            {
                // Draw versions
                gui_scrollViewPos = GUILayout.BeginScrollView(gui_scrollViewPos);
                {
                    for (int i = Versions.Count - 1; i >= 0; i--)
                    {
                        var v = Versions[i];
                        GUILayout.BeginHorizontal();
                        {
                            UIUtil.PushColor(Color.green);
                            GUILayout.Label(v.number, UIUtil.STYLE_LABEL, GUILayout.Width(50));
                            UIUtil.PopColor();
                            GUILayout.Label(v.info, UIUtil.STYLE_LABEL_LEFT);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Space(10);
                    }
                    GUILayout.EndScrollView();
                }
            }
            else
            {
                // Draw prefs values
                for (int i = 0; i < prefsValues.Count; i++)
                {
                    if (prefsValues[i] != null)
                        prefsValues[i].OnParkitectDefaultSettingsGUI();
                }
            }
        }

        /// <summary>
        /// Reset all settings to them default
        /// </summary>
        protected virtual void OnResetSettings()
        {
            for (int i = 0; i < prefsValues.Count; i++)
            {
                if (prefsValues[i] != null)
                    prefsValues[i].Reset();
            }
        }

        public virtual void onSettingsOpened()
        {
            gui_drawingVersions = false;
        }

        /// <summary>
        /// When closing settings window - save all pref values
        /// </summary>
        public virtual void onSettingsClosed()
        {
            for (int i = 0; i < prefsValues.Count; i++)
            {
                if (prefsValues[i] != null)
                    prefsValues[i].Save();
            }
            PlayerPrefs.Save();
        }

        private KeyMapping NewKeyMapping(string id, string niceName, string description, KeyCode key)
        {
            return new KeyMapping(HOTKEY + id, key, key)
            {
                canRebind = true,
                keyDescription = description,
                keyName = niceName,
                keyGroupIdentifier = getName()
            };
        }

        private void UnregisterHotkey(Hotkey hotkey)
        {
            HotkeyManager.Instance.unregisterHotkey(hotkey);
            InputManager.Instance.unregisterKeyMapping(hotkey.keyMapping);
            hotkey.onKeyDown = null;
        }

        private void SetInternalData(string modkey, string path)
        {
            PREFSKEY = $"{AUTHOR}.prefs.{modkey}.";
            HARMONYKEY = $"parkitect.{AUTHOR}.{modkey}";
            DEBUGKEY = $"[{AUTHOR}.{modkey}] ";
            MODPATH = path + "/";
            HOTKEY = $"{AUTHOR}.{modkey}.";
        }

        private void LogInternalError(string message)
        {
            UnityEngine.Debug.LogError("<color=red> MOD LOADING ERROR! -> " + AUTHOR + "." + MODKEY + " >  " + message + "</color>");
        }
    }

}