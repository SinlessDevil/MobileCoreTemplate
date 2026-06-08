#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Editor.Save
{
    public class SaveConfigExporterWindow : EditorWindow
    {
        private const string FolderName = "ExportedData";

        private string SaveFolderPath => Path.Combine(Application.dataPath, FolderName);

        private ScriptableObject _targetAsset;
        private int              _selectedFileIndex;
        private List<string>     _jsonFiles = new();

        // Cached UI refs
        private ObjectField    _fieldAsset;
        private HelpBox        _helpboxNoAsset;
        private VisualElement  _containerInspector;
        private VisualElement  _containerOverwrite;
        private VisualElement  _containerFiles;
        private ToolbarMenu    _dropdownFiles;

        [MenuItem("Tools/Save Window/Save Config Exporter Window", false, 2001)]
        private static void OpenWindow()
        {
            var window = GetWindow<SaveConfigExporterWindow>();
            window.titleContent = new GUIContent("Save Config Exporter Window");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Code/Editor/SaveSystem/SaveConfigExporterWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Code/Editor/SaveSystem/SaveConfigExporterWindow.uss");

            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _fieldAsset         = rootVisualElement.Q<ObjectField>("field-asset");
            _helpboxNoAsset     = rootVisualElement.Q<HelpBox>("helpbox-no-asset");
            _containerInspector = rootVisualElement.Q<VisualElement>("container-inspector");
            _containerOverwrite = rootVisualElement.Q<VisualElement>("container-overwrite");
            _containerFiles     = rootVisualElement.Q<VisualElement>("container-files");
            _dropdownFiles      = rootVisualElement.Q<ToolbarMenu>("dropdown-files");

            _fieldAsset.objectType = typeof(ScriptableObject);
            _fieldAsset.RegisterValueChangedCallback(evt =>
            {
                _targetAsset = evt.newValue as ScriptableObject;
                OnAssetChanged();
            });

            rootVisualElement.Q<Button>("btn-save-new").clicked     += SaveNewJson;
            rootVisualElement.Q<Button>("btn-overwrite").clicked    += OverwriteSelectedJson;

            Directory.CreateDirectory(SaveFolderPath);
            RefreshAll();
        }

        private void OnAssetChanged()
        {
            _containerInspector.Clear();

            bool hasAsset = _targetAsset != null;
            _helpboxNoAsset.EnableInClassList("hidden", hasAsset);

            if (hasAsset)
            {
                var inspector = new InspectorElement(_targetAsset);
                _containerInspector.Add(inspector);
            }

            RefreshJsonFiles();
            RebuildFileListUI();
        }

        private void RefreshAll()
        {
            bool hasAsset = _targetAsset != null;
            _helpboxNoAsset.EnableInClassList("hidden", hasAsset);
            RefreshJsonFiles();
            RebuildFileListUI();
        }

        private void RefreshJsonFiles()
        {
            _jsonFiles = Directory.Exists(SaveFolderPath)
                ? Directory.GetFiles(SaveFolderPath, "*.json")
                    .Where(p => _targetAsset != null && Path.GetFileName(p).StartsWith(_targetAsset.name))
                    .ToList()
                : new List<string>();

            _selectedFileIndex = 0;
            UpdateDropdown();

            bool hasFiles = _jsonFiles.Count > 0;
            _containerOverwrite.EnableInClassList("hidden", !hasFiles || _targetAsset == null);
        }

        private void UpdateDropdown()
        {
            _dropdownFiles.menu.MenuItems().Clear();
            for (int i = 0; i < _jsonFiles.Count; i++)
            {
                int idx = i;
                string label = Path.GetFileName(_jsonFiles[i]);
                _dropdownFiles.menu.AppendAction(label, _ =>
                {
                    _selectedFileIndex   = idx;
                    _dropdownFiles.text  = label;
                });
            }

            _dropdownFiles.text = _jsonFiles.Count > 0
                ? Path.GetFileName(_jsonFiles[0])
                : "Select file…";
        }

        private void RebuildFileListUI()
        {
            _containerFiles.Clear();
            foreach (var filePath in _jsonFiles)
                _containerFiles.Add(BuildFileRow(filePath));
        }

        private VisualElement BuildFileRow(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            var row = new VisualElement();
            row.AddToClassList("file-row");

            var btnLoad = new Button(() => LoadJsonToTarget(filePath)) { text = "Load" };
            btnLoad.AddToClassList("btn-load");

            var label = new Label(fileName);
            label.AddToClassList("file-label");

            var btnDelete = new Button(() =>
            {
                DeleteJsonFile(filePath);
                RefreshJsonFiles();
                RebuildFileListUI();
            }) { text = "Delete" };
            btnDelete.AddToClassList("btn-delete");

            row.Add(btnLoad);
            row.Add(label);
            row.Add(btnDelete);
            return row;
        }

        private void SaveNewJson()
        {
            if (_targetAsset == null) return;
            try
            {
                string path = Path.Combine(SaveFolderPath,
                    $"{_targetAsset.name}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                File.WriteAllText(path, JsonUtility.ToJson(_targetAsset, true));
                Debug.Log($"Saved JSON to: {path}");
                RefreshJsonFiles();
                RebuildFileListUI();
            }
            catch (Exception e) { Debug.LogError($"Failed to save JSON: {e}"); }
        }

        private void OverwriteSelectedJson()
        {
            if (_targetAsset == null || _jsonFiles.Count == 0) return;
            try
            {
                string path = _jsonFiles[_selectedFileIndex];
                File.WriteAllText(path, JsonUtility.ToJson(_targetAsset, true));
                Debug.Log($"Overwrote JSON: {path}");
                RefreshJsonFiles();
                RebuildFileListUI();
            }
            catch (Exception e) { Debug.LogError($"Failed to overwrite JSON: {e}"); }
        }

        private void LoadJsonToTarget(string path)
        {
            if (_targetAsset == null) return;
            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), _targetAsset);
                EditorUtility.SetDirty(_targetAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Loaded JSON from: {Path.GetFileName(path)}");
            }
            catch (Exception e) { Debug.LogError($"Failed to load JSON: {e}"); }
        }

        private void DeleteJsonFile(string path)
        {
            try
            {
                File.Delete(path);
                Debug.Log($"Deleted: {Path.GetFileName(path)}");
            }
            catch (Exception e) { Debug.LogError($"Failed to delete JSON: {e}"); }
        }
    }
}
#endif
