namespace RotationAnarchy.Internal
{
    using System;
    using UnityEngine;

    public abstract class PrefsValue
    {
        public ModBase ModBase { get; private set; }
        public virtual void Reset() { }
        public abstract void Save();
        public abstract void Load(bool invokeEvent);
        public abstract void OnParkitectDefaultSettingsGUI();
        public virtual void Clear() { }
        public virtual void SetModBase(ModBase modbase)
        {
            this.ModBase = modbase;
        }
    }

    public abstract class PrefsValue<T> : PrefsValue where T : struct
    {
        public event Action<T> OnValueChanged;

        protected T _value;
        public T Value { 
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(value);
            }
        }

        public T Min { get; set; }
        public T Max { get; set; }
        public T DefaultValue { get; private set; }
        public string Key { get; private set; }
        public string PrefsKey { get; private set; }
        public string GuiName { get; private set; }

        public PrefsValue(string key, T defaultValue, T min, T max, string guiname = null, Action<T> onValueChanged = null)
        {
            this.Key = key;
            this.DefaultValue = defaultValue;
            this.Value = defaultValue;
            this.Min = min;
            this.Max = max;
            this.GuiName = guiname;
            this.OnValueChanged = onValueChanged;
        }

        public override void Reset() => Value = DefaultValue;

        public override void Clear()
        {
            base.Clear();
            OnValueChanged = null;
        }

        public override void SetModBase(ModBase modbase)
        {
            base.SetModBase(modbase);
            PrefsKey = modbase.PREFSKEY + Key;
        }
    }

    public class PrefsFloat : PrefsValue<float>
    {
        public PrefsFloat(string key, float defaultValue, float min, float max, string guiname = null, Action<float> onValueChanged = null, string formatting = null) 
            : base(key, defaultValue, min, max, guiname, onValueChanged)
        {
            this.Formatting = formatting;
        }

        public string Formatting { get; set; }

        public override void Save()
        {
            PlayerPrefs.SetFloat(PrefsKey, Value);
            ModBase.LOG("Saving: " + PrefsKey + " value: " + Value);
        }

        public override void Load(bool invokeEvent)
        {
            float value = 0;
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                value = PlayerPrefs.GetFloat(PrefsKey);
                ModBase.LOG("Found key: " + PrefsKey + " value: " + value);
            }
            else
            {
                ModBase.LOG("Key not found: " + PrefsKey);
                value = DefaultValue;
            }

            if (invokeEvent)
                Value = value;
            else
                _value = value;
        }

        public override void OnParkitectDefaultSettingsGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(GuiName, UIUtil.STYLE_LABEL, GUILayout.Width(150));
                float value  = GUILayout.HorizontalSlider(Value, Min, Max, UIUtil.STYLE_SLIDER, UIUtil.STYLE_SLIDER_THUMB);
                if (value != Value)
                    Value = value;

                GUILayout.Label(Value.ToString(Formatting == null ? "0.0" : Formatting), UIUtil.STYLE_VAlUE, GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }
        }
    }

    public class PrefsBool : PrefsValue<bool>
    {

        public PrefsBool(string key, bool defaultValue, string guiname = null, Action<bool> onValueChanged = null) 
            : base(key, defaultValue, false, false, guiname, onValueChanged)
        {

        }

        public override void Save()
        {
            PlayerPrefs.SetInt(PrefsKey, Value ? 1 : 0);
            ModBase.LOG("Saving: " + PrefsKey + " value: " + Value);
        }

        public override void Load(bool invokeEvent)
        {
            bool value = false;
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                value = PlayerPrefs.GetInt(PrefsKey) == 1;
                ModBase.LOG("Found key: " + PrefsKey + " value: " + value);
            }
            else
            {
                ModBase.LOG("Key not found: " + PrefsKey);
                value = DefaultValue;
            }

            if (invokeEvent)
                Value = value;
            else
                _value = value;
        }

        public override void OnParkitectDefaultSettingsGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(GuiName, UIUtil.STYLE_LABEL, GUILayout.Width(150));
                bool value = GUILayout.Toggle(Value, "", UIUtil.STYLE_TOGGLE);
                if(value != Value)
                    Value = value;
                GUILayout.Label(Value.ToString(), UIUtil.STYLE_VAlUE, GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }
        }
    }
}