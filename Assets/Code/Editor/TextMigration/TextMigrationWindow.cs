using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Code.Editor.TextMigration
{
    public class TextMigrationWindow : EditorWindow
    {
        private enum TextType
        {
            LegacyText,
            TextMeshPro
        }

        private static readonly List<string> TypeOptions = new() { "Legacy Text", "TextMeshPro" };

        [MenuItem("Tools/Text Migration Window", false, 2010)]
        private static void OpenWindow()
        {
            var window = GetWindow<TextMigrationWindow>("Text Migration");
            window.minSize = new Vector2(420, 380);
            window.Show();
        }

        private ObjectField _fieldPrefab;
        private DropdownField _dropdownFrom;
        private DropdownField _dropdownTo;
        private ObjectField _fieldTmpFont;
        private ObjectField _fieldLegacyFont;
        private HelpBox _helpboxInfo;
        private Button _btnApply;

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Code/Editor/TextMigration/TextMigrationWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Code/Editor/TextMigration/TextMigrationWindow.uss");

            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _fieldPrefab = rootVisualElement.Q<ObjectField>("field-prefab");
            _dropdownFrom = rootVisualElement.Q<DropdownField>("dropdown-from");
            _dropdownTo = rootVisualElement.Q<DropdownField>("dropdown-to");
            _fieldTmpFont = rootVisualElement.Q<ObjectField>("field-tmp-font");
            _fieldLegacyFont = rootVisualElement.Q<ObjectField>("field-legacy-font");
            _helpboxInfo = rootVisualElement.Q<HelpBox>("helpbox-info");
            _btnApply = rootVisualElement.Q<Button>("btn-apply");

            _fieldPrefab.objectType = typeof(GameObject);
            _fieldTmpFont.objectType = typeof(TMP_FontAsset);
            _fieldLegacyFont.objectType = typeof(Font);

            _dropdownFrom.choices = TypeOptions;
            _dropdownTo.choices = TypeOptions;
            _dropdownFrom.index = 0; // LegacyText by default
            _dropdownTo.index = 1; // TextMeshPro by default

            _fieldPrefab.RegisterValueChangedCallback(_ => Refresh());
            _dropdownFrom.RegisterValueChangedCallback(_ => Refresh());
            _dropdownTo.RegisterValueChangedCallback(_ => Refresh());

            _btnApply.clicked += ApplyMigration;

            Refresh();
        }

        // ──────────────────────────── UI state ────────────────────────────

        private void Refresh()
        {
            var fromType = (TextType)_dropdownFrom.index;
            var toType = (TextType)_dropdownTo.index;
            bool same = fromType == toType;

            _fieldTmpFont.EnableInClassList("hidden", toType != TextType.TextMeshPro);
            _fieldLegacyFont.EnableInClassList("hidden", toType != TextType.LegacyText);

            _btnApply.SetEnabled(!same);
            UpdateInfoBox(fromType, toType, same);
        }

        private void UpdateInfoBox(TextType from, TextType to, bool same)
        {
            if (same)
            {
                _helpboxInfo.messageType = HelpBoxMessageType.Warning;
                _helpboxInfo.text = "From and To must be different types.";
                return;
            }

            var prefab = _fieldPrefab.value as GameObject;
            if (prefab == null)
            {
                _helpboxInfo.messageType = HelpBoxMessageType.Info;
                _helpboxInfo.text = "Assign a prefab to preview what will be migrated.";
                return;
            }

            int count = CountComponents(prefab, from);
            _helpboxInfo.messageType = count > 0 ? HelpBoxMessageType.Info : HelpBoxMessageType.Warning;
            _helpboxInfo.text = count > 0
                ? $"Found {count} {from} component(s) → will convert to {to}."
                : $"No {from} components found in this prefab.";
        }

        // ──────────────────────────── Migration ───────────────────────────

        private void ApplyMigration()
        {
            var prefab = _fieldPrefab.value as GameObject;
            if (prefab == null)
            {
                Warn("Assign a prefab first.");
                return;
            }

            if (_dropdownFrom.index == _dropdownTo.index) 
                return;

            var toType = (TextType)_dropdownTo.index;

            if (toType == TextType.TextMeshPro && _fieldTmpFont.value == null)
            {
                Warn("Assign a TMP Font Asset before migrating to TextMeshPro.");
                return;
            }

            if (toType == TextType.LegacyText && _fieldLegacyFont.value == null)
            {
                Warn("Assign a Legacy Font before migrating to Legacy Text.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Warn("Could not resolve prefab asset path.");
                return;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var root = scope.prefabContentsRoot;
                if (toType == TextType.TextMeshPro)
                    MigrateToTMP(root, (TMP_FontAsset)_fieldTmpFont.value);
                else
                    MigrateToLegacy(root, (Font)_fieldLegacyFont.value);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[TextMigration] Done: {assetPath}");
            Refresh();
        }

        // ─────────────────────────── Converters ───────────────────────────

        private static void MigrateToTMP(GameObject root, TMP_FontAsset font)
        {
            foreach (var src in root.GetComponentsInChildren<Text>(true).ToList())
                ConvertTextToTMP(src, font);
        }

        private static void MigrateToLegacy(GameObject root, Font font)
        {
            foreach (var src in root.GetComponentsInChildren<TextMeshProUGUI>(true).ToList())
                ConvertTMPToText(src, font);
        }

        private static void ConvertTextToTMP(Text src, TMP_FontAsset font)
        {
            var go = src.gameObject;

            string content = src.text;
            int fontSize = src.fontSize;
            Color color = src.color;
            bool raycast = src.raycastTarget;
            bool isEnabled = src.enabled;
            var alignment = src.alignment;
            var fontStyle = src.fontStyle;
            float lineSpace = src.lineSpacing;
            bool richText = src.supportRichText;

            Object.DestroyImmediate(src);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.raycastTarget = raycast;
            tmp.enabled = isEnabled;
            tmp.font = font;
            tmp.alignment = ToTMPAlignment(alignment);
            tmp.fontStyle = ToTMPFontStyle(fontStyle);
            tmp.lineSpacing = lineSpace;
            tmp.richText = richText;
        }

        private static void ConvertTMPToText(TextMeshProUGUI src, Font font)
        {
            var go = src.gameObject;

            string content = src.text;
            float fontSize = src.fontSize;
            Color color = src.color;
            bool raycast = src.raycastTarget;
            bool isEnabled = src.enabled;
            var alignment = src.alignment;
            var fontStyle = src.fontStyle;
            float lineSpace = src.lineSpacing;
            bool richText = src.richText;

            Object.DestroyImmediate(src);

            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = Mathf.RoundToInt(fontSize);
            txt.color = color;
            txt.raycastTarget = raycast;
            txt.enabled = isEnabled;
            txt.font = font;
            txt.alignment = ToLegacyAlignment(alignment);
            txt.fontStyle = ToLegacyFontStyle(fontStyle);
            txt.lineSpacing = lineSpace;
            txt.supportRichText = richText;
        }

        // ──────────────────────────── Mapping ─────────────────────────────

        private static TextAlignmentOptions ToTMPAlignment(TextAnchor a) => a switch
        {
            TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
            TextAnchor.UpperCenter => TextAlignmentOptions.Top,
            TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
            TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
            TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
            TextAnchor.MiddleRight => TextAlignmentOptions.Right,
            TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
            TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
            TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
            _ => TextAlignmentOptions.Center
        };

        private static TextAnchor ToLegacyAlignment(TextAlignmentOptions a) => a switch
        {
            TextAlignmentOptions.TopLeft => TextAnchor.UpperLeft,
            TextAlignmentOptions.Top => TextAnchor.UpperCenter,
            TextAlignmentOptions.TopRight => TextAnchor.UpperRight,
            TextAlignmentOptions.Left => TextAnchor.MiddleLeft,
            TextAlignmentOptions.Center => TextAnchor.MiddleCenter,
            TextAlignmentOptions.Right => TextAnchor.MiddleRight,
            TextAlignmentOptions.BottomLeft => TextAnchor.LowerLeft,
            TextAlignmentOptions.Bottom => TextAnchor.LowerCenter,
            TextAlignmentOptions.BottomRight => TextAnchor.LowerRight,
            _ => TextAnchor.MiddleCenter
        };

        private static FontStyles ToTMPFontStyle(UnityEngine.FontStyle s) => s switch
        {
            UnityEngine.FontStyle.Bold => FontStyles.Bold,
            UnityEngine.FontStyle.Italic => FontStyles.Italic,
            UnityEngine.FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
            _ => FontStyles.Normal
        };

        private static UnityEngine.FontStyle ToLegacyFontStyle(FontStyles s)
        {
            bool bold = (s & FontStyles.Bold) != 0;
            bool italic = (s & FontStyles.Italic) != 0;
            return (bold, italic) switch
            {
                (true, true) => UnityEngine.FontStyle.BoldAndItalic,
                (true, false) => UnityEngine.FontStyle.Bold,
                (false, true) => UnityEngine.FontStyle.Italic,
                _ => UnityEngine.FontStyle.Normal
            };
        }

        // ───────────────────────────── Helpers ────────────────────────────

        private static int CountComponents(GameObject prefab, TextType type) =>
            type == TextType.LegacyText
                ? prefab.GetComponentsInChildren<Text>(true).Length
                : prefab.GetComponentsInChildren<TextMeshProUGUI>(true).Length;

        private static void Warn(string msg) =>
            EditorUtility.DisplayDialog("Text Migration", msg, "OK");
    }
}