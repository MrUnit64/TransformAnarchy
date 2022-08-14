namespace RotationAnarchy.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DebugGUI : MonoBehaviour
    {
        public static bool IsActive => instance;

        private static string prefsKey = "banana.debugGui.rect.";

        [System.Serializable]
        private struct SavedRect
        {
            public float x, y, width, height;
            public SavedRect(Rect rect)
            {
                x = rect.x; y = rect.y; width = rect.width; height = rect.height;
            }
            public Rect ToRect() => new Rect(x, y, width, height);
        }

        public static void SetActive(bool state)
        {
            if (state)
            {
                if (PlayerPrefs.HasKey(prefsKey))
                    rect = JsonUtility.FromJson<SavedRect>(PlayerPrefs.GetString(prefsKey)).ToRect();
                instance = new GameObject("DebugGUI").AddComponent<DebugGUI>();
                resizing = false;
            }
            else
            {
                PlayerPrefs.SetString(prefsKey, JsonUtility.ToJson(new SavedRect(rect)));
                PlayerPrefs.Save();
                GameObject.Destroy(instance.gameObject);
                instance = null;
            }
        }

        public static void DrawValue(string name, object value, Color? color = null)
        {
            if (!IsActive)
                return;

            if (name == null || value == null)
                return;

            commands.Add(new DrawCommand()
            {
                name = GetContent(name),
                value = GetContent(value.ToString()),
                color = color.HasValue ? color.Value : new Color(1, 1, 1, 1)
            });
        }

        private static DebugGUI instance;
        private static Rect rect = new Rect(0, 0, 300, 600);
        private static List<DrawCommand> commands = new List<DrawCommand>();
        private static Texture2D lineTex;
        private static Texture2D rowTex;
        private static GUIStyle labelStyle;
        private static GUIStyle valueStyle;
        private static bool initialized;
        private static Stack<GUIContent> guiContentPool = new Stack<GUIContent>();

        private static void Initialize()
        {
            if (initialized)
                return;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.LowerRight;

            valueStyle = new GUIStyle(GUI.skin.label);
            valueStyle.alignment = TextAnchor.LowerLeft;

            lineTex = new Texture2D(1, 1);
            lineTex.SetPixel(0, 0, new Color(1, 1, 1, 0.5f));
            lineTex.Apply();

            rowTex = new Texture2D(1, 1);
            rowTex.SetPixel(0, 0, new Color(1, 1, 1, 0.05f));
            rowTex.Apply();

            initialized = true;
        }

        private void OnDisable()
        {

            GameObject.Destroy(instance.gameObject);
        }

        private struct DrawCommand
        {
            public GUIContent name;
            public GUIContent value;
            public Color color;
        }

        private void OnGUI()
        {
            Initialize();
            GUI.color = Color.white;
            rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);
            rect.width = Mathf.Clamp(rect.width, 200, Screen.width);
            rect.height = Mathf.Clamp(rect.height, 200, Screen.height);
            rect = GUI.Window(GetInstanceID(), rect, WindowFunction, "debug values");

        }

        private static bool resizing;
        private static Rect rectOnResize;
        private static Vector2 mouseClickPos;

        private void WindowFunction(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            float resizeCornerSize = 10;
            Rect resizeCornerRect = new Rect(rect.width - resizeCornerSize - 2, rect.height - resizeCornerSize - 2, resizeCornerSize, resizeCornerSize);

            if (!resizing && Event.current.type == EventType.MouseDown && resizeCornerRect.Contains(Event.current.mousePosition))
            {
                resizing = true;
                rectOnResize = rect;
                mouseClickPos = GetMousePos();
            }

            if (resizing && Event.current.type == EventType.MouseUp)
            {
                resizing = false;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            if (resizing)
            {
                Vector2 delta = mouseClickPos - GetMousePos();
                rect.width = rectOnResize.width - delta.x;
                rect.height = rectOnResize.height - delta.y;
            }

            rect.width = Mathf.Clamp(rect.width, 200, Screen.width);
            rect.height = Mathf.Clamp(rect.height, 200, Screen.height);

            GUI.DrawTexture(resizeCornerRect, lineTex);

            float rowHeight = 18;
            float padding = 5;
            Rect rowRect = new Rect(0, 25, rect.width, rowHeight);

            float maxLabelWidth = -1;
            foreach (var cmd in commands)
                maxLabelWidth = Mathf.Max(maxLabelWidth, labelStyle.CalcSize(cmd.name).x);

            maxLabelWidth += padding * 2;
            GUI.DrawTexture(new Rect(maxLabelWidth + 1, 20, 1, rect.height - 25), lineTex);

            int row = 0;
            foreach (var command in commands)
            {
                Rect labelRect = new Rect(rowRect.x + padding, rowRect.y, maxLabelWidth - (padding * 2), rowHeight);

                GUI.color = command.color;
                if (row % 2 == 0)
                    GUI.DrawTexture(rowRect, rowTex);

                GUI.Label(labelRect, command.name, labelStyle);
                GUI.color = Color.white;

                Rect valueRect = new Rect(labelRect.width + padding * 4, rowRect.y, rowRect.width, rowHeight);
                GUI.Label(valueRect, command.value, valueStyle);

                rowRect.y += rowHeight;
                row++;
            }

            foreach (var item in commands)
            {
                guiContentPool.Push(item.name);
                guiContentPool.Push(item.value);
            }

            commands.Clear();
        }

        private static Vector2 GetMousePos()
        {
            Vector2 mp = Input.mousePosition;
            return new Vector2(mp.x, Screen.height - mp.y);
        }

        private static GUIContent GetContent(string value)
        {
            GUIContent content = null;
            if (guiContentPool.Count > 0)
                content = guiContentPool.Pop();
            else
                content = new GUIContent();
            content.text = value;
            return content;
        }
    }

}