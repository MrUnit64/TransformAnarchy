namespace RotationAnarchy.Internal
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.UI;

    public static class UIUtil
    {
        public static GUIStyle STYLE_BUTTON;
        public static GUIStyle STYLE_LABEL_INFO;
        public static GUIStyle STYLE_LABEL;
        public static GUIStyle STYLE_LABEL_LEFT;
        public static GUIStyle STYLE_SLIDER;
        public static GUIStyle STYLE_TOGGLE;
        public static GUIStyle STYLE_SLIDER_THUMB;
        public static GUIStyle STYLE_VAlUE;

        private static bool _stylesInitialized;

        private static Stack<Color> _colorStack = new Stack<Color>();

        public static void InitStyles()
        {
            if (!_stylesInitialized)
            {
                STYLE_BUTTON = new GUIStyle(GUI.skin.button);

                STYLE_LABEL_INFO = new GUIStyle(GUI.skin.label);
                STYLE_LABEL_INFO.alignment = TextAnchor.MiddleLeft;
                STYLE_LABEL_INFO.fontSize = 18;
                STYLE_LABEL_INFO.contentOffset += new Vector2(0, -20);
                STYLE_LABEL_INFO.padding = new RectOffset(10, 10, 10, 10);

                STYLE_LABEL = new GUIStyle(GUI.skin.label);
                STYLE_LABEL.alignment = TextAnchor.MiddleRight;
                STYLE_LABEL.fontSize = 12;
                STYLE_LABEL.contentOffset += new Vector2(0, -20);

                STYLE_LABEL_LEFT = new GUIStyle(GUI.skin.label);
                STYLE_LABEL_LEFT.alignment = TextAnchor.MiddleLeft;
                STYLE_LABEL_LEFT.fontSize = 12;
                STYLE_LABEL_LEFT.contentOffset += new Vector2(0, -20);

                STYLE_SLIDER = new GUIStyle(GUI.skin.horizontalSlider);
                STYLE_SLIDER_THUMB = new GUIStyle(GUI.skin.horizontalSliderThumb);

                STYLE_VAlUE = new GUIStyle(GUI.skin.label);
                STYLE_VAlUE.alignment = TextAnchor.MiddleLeft;
                STYLE_VAlUE.normal.textColor = new Color32(181, 230, 29, 255);
                STYLE_VAlUE.fontSize = 12;
                STYLE_VAlUE.contentOffset += new Vector2(0, -20);

                STYLE_TOGGLE = new GUIStyle(GUI.skin.toggle);
                STYLE_TOGGLE.padding = new RectOffset(0, 0, 0, 0);
                STYLE_TOGGLE.margin = new RectOffset(0, 0, 0, 0);

                _stylesInitialized = true;
            }
        }

        public static void PushColor(Color color)
        {
            _colorStack.Push(color);
            GUI.color = color;
        }

        public static void PopColor()
        {
            if(_colorStack.Count > 0)
                _colorStack.Pop();
         
            GUI.color = _colorStack.Count > 0 ? _colorStack.Peek() : Color.white;
        }

        public static void RemoveListeners(Toggle tgl)
        {
            tgl.onValueChanged.RemoveAllListeners();
            int persitent = tgl.onValueChanged.GetPersistentEventCount();
            for (int i = 0; i < persitent; i++)
            {
                tgl.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }
        }

        public static void RemoveListeners(Slider slider)
        {
            slider.onValueChanged.RemoveAllListeners();
            int persitent = slider.onValueChanged.GetPersistentEventCount();
            for (int i = 0; i < persitent; i++)
            {
                slider.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }
        }

        public static void RemoveListeners(Button button)
        {
            button.onClick.RemoveAllListeners();
            int persitent = button.onClick.GetPersistentEventCount();
            for (int i = 0; i < persitent; i++)
            {
                button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }
        }

        public static Texture2D LoadTexture(string modpath, string name, string extension)
        {
            Texture2D tex = new Texture2D(1, 1);
            string imgPath = modpath + name + extension;
            if (File.Exists(imgPath))
            {
                tex.LoadImage(File.ReadAllBytes(imgPath));
                return tex;
            }
            return null;
        }

        public static Sprite LoadSprite(string modpath, string name, string extension)
        {
            var tex = LoadTexture(modpath, name, extension);
            if (tex == null)
                return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public static void ClearLayoutElement(LayoutElement le)
        {
            le.flexibleWidth = le.flexibleHeight = -1;
            le.preferredWidth = le.preferredHeight = 0;
            le.minHeight = le.minWidth = 0;
        }
    }
}