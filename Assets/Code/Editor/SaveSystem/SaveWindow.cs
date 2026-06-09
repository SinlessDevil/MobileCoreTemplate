#if UNITY_EDITOR
using System;
using System.IO;
using Code.Services.PersistenceProgress.Player;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Editor.Save
{
    public class SaveWindow : EditorWindow
    {
        private const string PlayerPrefsKey = "PlayerData";
        private const string JsonFileName = "player_data.json";

        private string SavePath => Application.persistentDataPath;
        private string JsonFilePath => Path.Combine(SavePath, JsonFileName);

        // --- Cached UI elements ---
        private HelpBox _prefsPathBox;
        private ScrollView _prefsScroll;
        private Label _prefsDataLabel;
        private HelpBox _prefsMsgBox;

        private HelpBox _jsonPathBox;
        private ScrollView _jsonScroll;
        private Label _jsonDataLabel;
        private HelpBox _jsonMsgBox;

        [MenuItem("Tools/Save Window/All Saves Window", false, 2001)]
        private static void OpenWindow()
        {
            var window = GetWindow<SaveWindow>();
            window.titleContent = new GUIContent("All Saves Window");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        // CreateGUI() — вызывается один раз при открытии окна.
        // Здесь строим дерево UI и вешаем колбэки.
        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Code/Editor/SaveSystem/SaveWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Code/Editor/SaveSystem/SaveWindow.uss");

            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            BindSection(
                pathBoxName:    "helpbox-prefs-path",
                scrollName:     "scroll-prefs",
                dataLabelName:  "label-prefs-data",
                msgBoxName:     "helpbox-prefs-msg",
                refreshBtnName: "btn-refresh-prefs",
                deleteBtnName:  "btn-delete-prefs",
                out _prefsPathBox, out _prefsScroll, out _prefsDataLabel, out _prefsMsgBox,
                RefreshPlayerPrefs, DeletePlayerPrefs);

            BindSection(
                pathBoxName:    "helpbox-json-path",
                scrollName:     "scroll-json",
                dataLabelName:  "label-json-data",
                msgBoxName:     "helpbox-json-msg",
                refreshBtnName: "btn-refresh-json",
                deleteBtnName:  "btn-delete-json",
                out _jsonPathBox, out _jsonScroll, out _jsonDataLabel, out _jsonMsgBox,
                RefreshJsonFile, DeleteJson);

            _prefsPathBox.text = GetPlayerPrefsPath();
            _jsonPathBox.text  = JsonFilePath;

            Refresh();
        }

        // Q<T>(name) — ищет элемент по имени и типу в дереве UXML.
        private void BindSection(
            string pathBoxName, string scrollName, string dataLabelName, string msgBoxName,
            string refreshBtnName, string deleteBtnName,
            out HelpBox pathBox, out ScrollView scroll, out Label dataLabel, out HelpBox msgBox,
            Action onRefresh, Action onDelete)
        {
            pathBox   = rootVisualElement.Q<HelpBox>(pathBoxName);
            scroll    = rootVisualElement.Q<ScrollView>(scrollName);
            dataLabel = rootVisualElement.Q<Label>(dataLabelName);
            msgBox    = rootVisualElement.Q<HelpBox>(msgBoxName);

            rootVisualElement.Q<Button>(refreshBtnName).clicked += onRefresh;
            rootVisualElement.Q<Button>(deleteBtnName).clicked  += onDelete;
        }

        private void Refresh()
        {
            RefreshPlayerPrefs();
            RefreshJsonFile();
        }

        private void RefreshPlayerPrefs()
        {
            string data = string.Empty;
            string fallback = "No PlayerPrefs data found.";

            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                try
                {
                    string raw = PlayerPrefs.GetString(PlayerPrefsKey);
                    var obj  = JsonConvert.DeserializeObject<PlayerData>(raw);
                    data     = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                catch (Exception e)
                {
                    data = $"Failed to decode PlayerPrefs:\n{e.Message}";
                }
            }

            UpdateSection(_prefsScroll, _prefsDataLabel, _prefsMsgBox, data, fallback);
        }

        private void RefreshJsonFile()
        {
            string data = string.Empty;
            string fallback = "No JSON file found.";

            if (File.Exists(JsonFilePath))
            {
                try { data = File.ReadAllText(JsonFilePath); }
                catch (Exception e) { data = $"Failed to read JSON:\n{e.Message}"; }
            }

            UpdateSection(_jsonScroll, _jsonDataLabel, _jsonMsgBox, data, fallback);
        }

        // Показывает либо данные, либо предупреждение — через CSS-класс "hidden".
        private static void UpdateSection(ScrollView scroll, Label dataLabel, HelpBox msgBox,
            string data, string fallbackMessage)
        {
            bool hasData = !string.IsNullOrEmpty(data);

            dataLabel.text = data;
            scroll.EnableInClassList("hidden", !hasData);
            msgBox.EnableInClassList("hidden", hasData);

            if (!hasData)
                msgBox.text = fallbackMessage;
        }

        private void DeletePlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return;
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs deleted.");
            RefreshPlayerPrefs();
        }

        private void DeleteJson()
        {
            if (!File.Exists(JsonFilePath)) return;
            File.Delete(JsonFilePath);
            Debug.Log("JSON file deleted.");
            RefreshJsonFile();
        }

        private string GetPlayerPrefsPath()
        {
#if UNITY_EDITOR_WIN
            return $@"Windows Registry: HKEY_CURRENT_USER\Software\Unity\UnityEditor\{Application.companyName}\{Application.productName}";
#elif UNITY_EDITOR_OSX
            return $"~/Library/Preferences/unity.{Application.companyName}.{Application.productName}.plist";
#else
            return "Platform not supported for PlayerPrefs path preview.";
#endif
        }
    }
}
#endif
