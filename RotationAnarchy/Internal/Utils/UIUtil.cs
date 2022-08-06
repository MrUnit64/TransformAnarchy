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
        public static GUIStyle STYLE_LABEL_MICRO;
        public static GUIStyle STYLE_LABEL_CENTER;
        public static GUIStyle STYLE_LABEL_LEFT;
        public static GUIStyle STYLE_SLIDER;
        public static GUIStyle STYLE_TOGGLE;
        public static GUIStyle STYLE_SLIDER_THUMB;
        public static GUIStyle STYLE_VAlUE;
        public static GUIStyle STYLE_TEXTFIELD;
        private static RectOffset defaultMargin;
        private static RectOffset defaultPadding;

        private static bool _stylesInitialized;

        private static Stack<Color> _colorStack = new Stack<Color>();

        private static Dictionary<string, string> idStringMap = new Dictionary<string, string>();
        private static Dictionary<string, float> idFloatMap = new Dictionary<string, float>();
        private static Dictionary<string, bool> idBoolMap = new Dictionary<string, bool>();
        private static Dictionary<string, Rect> idRectMap = new Dictionary<string, Rect>();

        private static Dictionary<string, GUIContent> labelMicroGUIContent = new Dictionary<string, GUIContent>();

        private static Texture2D textureWhite;

        public static void InitStyles()
        {
            if (!_stylesInitialized)
            {
                textureWhite = CreateSolidColorTexture(1, 1, new Color(1,1,1,1));

                defaultMargin = new RectOffset(5, 5, 0, 0);
                defaultPadding = new RectOffset(5, 5, 2, 2);

                STYLE_BUTTON = new GUIStyle(GUI.skin.button);

                STYLE_LABEL_INFO = new GUIStyle(GUI.skin.label);
                STYLE_LABEL_INFO.alignment = TextAnchor.MiddleLeft;
                STYLE_LABEL_INFO.fontSize = 18;
                STYLE_LABEL_INFO.padding = defaultPadding;
                STYLE_LABEL_INFO.margin = defaultMargin;

                STYLE_LABEL = new GUIStyle(GUI.skin.label);
                STYLE_LABEL.alignment = TextAnchor.MiddleRight;
                STYLE_LABEL.fontSize = 12;
                STYLE_LABEL.padding = defaultPadding;
                STYLE_LABEL.margin = defaultMargin;

                STYLE_LABEL_MICRO = new GUIStyle(STYLE_LABEL);
                STYLE_LABEL_MICRO.alignment = TextAnchor.MiddleCenter;
                STYLE_LABEL_MICRO.fontSize = 7;

                STYLE_LABEL_CENTER = new GUIStyle(STYLE_LABEL);
                STYLE_LABEL_CENTER.alignment = TextAnchor.MiddleCenter;

                STYLE_LABEL_LEFT = new GUIStyle(GUI.skin.label);
                STYLE_LABEL_LEFT.alignment = TextAnchor.MiddleLeft;
                STYLE_LABEL_LEFT.fontSize = 12;
                STYLE_LABEL_LEFT.padding = defaultPadding;
                STYLE_LABEL_LEFT.margin = defaultMargin;

                STYLE_SLIDER = new GUIStyle(GUI.skin.horizontalSlider);
                STYLE_SLIDER_THUMB = new GUIStyle(GUI.skin.horizontalSliderThumb);

                STYLE_VAlUE = new GUIStyle(GUI.skin.label);
                STYLE_VAlUE.alignment = TextAnchor.MiddleLeft;
                STYLE_VAlUE.normal.textColor = new Color32(181, 230, 29, 255);
                STYLE_VAlUE.fontSize = 12;
                STYLE_VAlUE.padding = defaultPadding;
                STYLE_VAlUE.margin = defaultMargin;

                STYLE_TEXTFIELD = new GUIStyle(GUI.skin.textField);
                STYLE_TEXTFIELD.alignment = TextAnchor.MiddleLeft;
                STYLE_TEXTFIELD.fontSize = 12;
                STYLE_TEXTFIELD.padding = defaultPadding;
                STYLE_TEXTFIELD.margin = defaultMargin;

                STYLE_TOGGLE = new GUIStyle(GUI.skin.toggle);
                STYLE_TOGGLE.padding = defaultPadding;
                STYLE_TOGGLE.margin = defaultMargin;

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
            if (_colorStack.Count > 0)
                _colorStack.Pop();

            GUI.color = _colorStack.Count > 0 ? _colorStack.Peek() : Color.white;
        }

        public static bool GUILayoutSnappedSliderFloat(string id, float value, float min, float max, float snap, out float result)
        {
            result = value;

            if (!idFloatMap.TryGetValue(id, out float currentFloat))
                idFloatMap[id] = currentFloat = value;

            if (!idBoolMap.TryGetValue(id, out bool pressed))
                idBoolMap[id] = false;

            if (!idRectMap.TryGetValue(id, out Rect rect))
                idRectMap[id] = new Rect();

            //if value was changed externally
            if (!pressed && value != currentFloat)
            {
                currentFloat = value;
            }

            // Draw background graphics
            // divisions 
            if(snap > 0)
            {
                float divisions = ((max - min) / snap);
                float padding = 6;
                float step = (rect.width - padding * 2) / divisions;
                Vector2 start = new Vector2(rect.x + step + padding, rect.y-2);
                for (int i = 0; i < divisions - 1; i++)
                {
                    var pos = new Rect(start.x + (step * i), start.y, 1, rect.height - 5);
                    GUI.DrawTexture(pos, textureWhite);

                    if(snap > 19)
                    {
                        string label = (snap * (i + 1)).ToString("0.0");
                        Vector2 size = STYLE_LABEL_MICRO.CalcSize(GetLabelMicroGuiContent(label));
                        GUI.Label(new Rect(pos.x - (size.x / 2f), pos.y - size.y, size.x, size.y), GetLabelMicroGuiContent(label), STYLE_LABEL_MICRO);
                    }
                }
            }
            
            if (pressed)
                PushColor(Color.blue);
            float outputFloat = GUILayout.HorizontalSlider(currentFloat, min, max, UIUtil.STYLE_SLIDER, UIUtil.STYLE_SLIDER_THUMB);
            if (pressed)
                PopColor();

            idFloatMap[id] = outputFloat;

            float snapped = MathUtility.roundToNearest(currentFloat, snap);

            // handle input
            Event e = Event.current;
            if(e.type == EventType.Repaint)
                idRectMap[id] = rect = GUILayoutUtility.GetLastRect();

            if (rect.Contains(e.mousePosition))
            {
                if(!pressed)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    {
                        idBoolMap[id] = pressed = true;
                    }
                }
            }

            if(pressed)
            {
                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    idBoolMap[id] = pressed = false;
                    idFloatMap[id] = snapped;
                }
            }

            if (snapped != value)
            {
                result = snapped;
                return true;
            }

            return false;
        }

        /// <summary>
        /// While editing the text field we need to allow user to make edits without overwriting their input.
        /// For this reason this method uses hash lookup to store temporary value of the field while its being edited.
        /// If the value inputed is correct, it returns true and can be assigned.
        /// If the value is incorrect the field shows red, and will revert itself to initial value when lost focus.
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="value">Value when we started drawing</param>
        /// <param name="result">A valid int if any was inputed</param>
        /// <returns>True if the input was made, and input was parsed as a correct float</returns>
        public static bool GUILayoutFloatField(string id, string format, float value, out float result, float? maxWidth = null)
        {
            result = value;

            GUI.SetNextControlName(id);

            string valueString = value.ToString(format ?? null);

            // check if we already have started editing this field
            if (!idStringMap.TryGetValue(id, out string currentString))
                idStringMap[id] = currentString = valueString;

            // is the current field string value a correct float?
            bool isValidInput = float.TryParse(currentString, out float currentValid);

            if (isValidInput)
            {
                if (value != currentValid)
                {
                    //happens when PROVIDED value changes, externally, indirectly (something other than user)
                    idStringMap[id] = currentString = valueString;
                }
            }

            if (!isValidInput)
                PushColor(Color.red);

            string fieldOutputString = null;
            if (maxWidth.HasValue)
            {
                fieldOutputString = GUILayout.TextField(currentString, UIUtil.STYLE_TEXTFIELD, GUILayout.MaxWidth(maxWidth.Value));
            }
            else
            {
                fieldOutputString = GUILayout.TextField(currentString, UIUtil.STYLE_TEXTFIELD);
            }

            if (!isValidInput)
            {
                PopColor();
                // field is not valid and we are not focusing it anymore, revert the value
                if (GUI.GetNameOfFocusedControl() != id)
                {
                    fieldOutputString = valueString;
                }
            }

            // store current field output, disregarding its correctness
            idStringMap[id] = fieldOutputString;

            // test field output if its correct
            if (float.TryParse(fieldOutputString, out float validFloat))
            {
                // if correct and not equals to value provided, we got a hit
                if (validFloat != value)
                {
                    result = validFloat;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// While editing the text field we need to allow user to make edits without overwriting their input.
        /// For this reason this method uses hash lookup to store temporary value of the field while its being edited.
        /// If the value inputed is correct, it returns true and can be assigned.
        /// If the value is incorrect the field shows red, and will revert itself to initial value when lost focus.
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="value">Value when we started drawing</param>
        /// <param name="result">A valid float if any was inputed</param>
        /// <returns>True if the input was made, and input was parsed as a correct float</returns>
        public static bool GUILayoutIntField(string id, int value, out int result, float? maxWidth = null)
        {
            result = value;

            GUI.SetNextControlName(id);

            string valueString = value.ToString();

            // check if we already have started editing this field
            if (!idStringMap.TryGetValue(id, out string currentString))
            {
                idStringMap[id] = currentString = valueString;
            }

            // is the current field string value a correct float?
            bool isValidInput = int.TryParse(currentString, out int currentValid);

            if (isValidInput)
            {
                if (value != currentValid)
                {
                    //happens when PROVIDED value changes, externally, indirectly (something other than user)
                    idStringMap[id] = currentString = valueString;
                }
            }

            if (!isValidInput)
                PushColor(Color.red);

            string fieldOutputString = null;
            if (maxWidth.HasValue)
            {
                fieldOutputString = GUILayout.TextField(currentString, UIUtil.STYLE_TEXTFIELD, GUILayout.MaxWidth(maxWidth.Value));
            }
            else
            {
                fieldOutputString = GUILayout.TextField(currentString, UIUtil.STYLE_TEXTFIELD);
            }

            if (!isValidInput)
            {
                PopColor();
                // field is not valid and we are not focusing it anymore, revert the value
                if (GUI.GetNameOfFocusedControl() != id)
                {
                    fieldOutputString = valueString;
                }
            }

            // store current field output, disregarding its correctness
            idStringMap[id] = fieldOutputString;

            // test field output if its correct
            if (int.TryParse(fieldOutputString, out int validInt))
            {
                // if correct and not equals to value provided, we got a hit
                if (validInt != value)
                {
                    result = validInt;
                    return true;
                }
            }

            return false;
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

        public static Texture2D CreateSolidColorTexture(int width, int height, Color color)
        {
            var tex = new Texture2D(width, height);
            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static GUIContent GetLabelMicroGuiContent(string value)
        {
            if(!labelMicroGUIContent.TryGetValue(value, out var content))
            {
                labelMicroGUIContent[value] = content = new GUIContent(value);
            }

            return content;
        }
    }
}