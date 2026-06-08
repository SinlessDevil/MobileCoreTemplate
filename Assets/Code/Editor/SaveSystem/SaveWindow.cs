#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using Code.Services.PersistenceProgress.Player;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace Code.Editor.Save
{
    public class SaveWindow : EditorWindow
    {
        private const string PlayerPrefsKey = "PlayerData";
        private const string JsonFileName = "player_data.json";
        private const string XmlFileName = "player_data.xml";

        private string SavePath => Application.persistentDataPath;
        private string JsonFilePath => Path.Combine(SavePath, JsonFileName);
        private string XmlFilePath => Path.Combine(SavePath, XmlFileName);

        // --- Cached UI elements ---
        private HelpBox _prefsPathBox;
        private ScrollView _prefsScroll;
        private Label _prefsDataLabel;
        private HelpBox _prefsMsgBox;

        private HelpBox _jsonPathBox;
        private ScrollView _jsonScroll;
        private Label _jsonDataLabel;
        private HelpBox _jsonMsgBox;

        private HelpBox _xmlPathBox;
        private ScrollView _xmlScroll;
        private Label _xmlDataLabel;
        private HelpBox _xmlMsgBox;

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

            BindSection(
                pathBoxName:    "helpbox-xml-path",
                scrollName:     "scroll-xml",
                dataLabelName:  "label-xml-data",
                msgBoxName:     "helpbox-xml-msg",
                refreshBtnName: "btn-refresh-xml",
                deleteBtnName:  "btn-delete-xml",
                out _xmlPathBox, out _xmlScroll, out _xmlDataLabel, out _xmlMsgBox,
                RefreshXmlFile, DeleteXml);

            _prefsPathBox.text = GetPlayerPrefsPath();
            _jsonPathBox.text  = JsonFilePath;
            _xmlPathBox.text   = XmlFilePath;

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
            RefreshXmlFile();
        }

        private void RefreshPlayerPrefs()
        {
            string data = string.Empty;
            string fallback = "No PlayerPrefs data found.";

            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                try
                {
                    string base64 = PlayerPrefs.GetString(PlayerPrefsKey);
                    byte[] bytes = Convert.FromBase64String(base64);
                    var deserialized = SerializationUtility.DeserializeValue<PlayerData>(bytes, DataFormat.JSON);
                    data = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(deserialized, DataFormat.JSON));
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

        private void RefreshXmlFile()
        {
            string data = string.Empty;
            string fallback = "No XML file found.";

            if (File.Exists(XmlFilePath))
            {
                try { data = File.ReadAllText(XmlFilePath); }
                catch (Exception e) { data = $"Failed to read XML:\n{e.Message}"; }
            }

            UpdateSection(_xmlScroll, _xmlDataLabel, _xmlMsgBox, data, fallback);
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

        private void DeleteXml()
        {
            if (!File.Exists(XmlFilePath)) return;
            File.Delete(XmlFilePath);
            Debug.Log("XML file deleted.");
            RefreshXmlFile();
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
