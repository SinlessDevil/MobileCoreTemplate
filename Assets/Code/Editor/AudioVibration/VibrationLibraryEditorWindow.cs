using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Services.AudioVibrationFX.Vibration;
using Code.StaticData.AudioVibration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Editor.AudioVibration
{
    public class VibrationLibraryEditorWindow : EditorWindow
    {
        private const string VibrationDataPath = "StaticData/Vibration/VibrationsData";
        private const string EnumOutPutPath    = "Assets/Code/Services/AudioVibrationFX/Vibration/VibrationType.cs";

        private VibrationsData _vibrationsData;
        private SerializedObject _serializedData;
        private TextField _enumPreview;

        [MenuItem("Tools/Audio Vibration Window/Vibration Library", false, 2002)]
        private static void OpenWindow() => GetWindow<VibrationLibraryEditorWindow>().Show();

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Code/Editor/AudioVibration/VibrationLibraryEditorWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Code/Editor/AudioVibration/VibrationLibraryEditorWindow.uss");

            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _enumPreview           = rootVisualElement.Q<TextField>("tf-vibration-enum");
            _enumPreview.isReadOnly = true;

            rootVisualElement.Q<Button>("btn-generate").clicked += GenerateEnum;

            LoadData();
        }

        private void LoadData()
        {
            _vibrationsData = Resources.Load<VibrationsData>(VibrationDataPath);

            if (_vibrationsData == null)
            {
                Debug.LogError($"VibrationsData not found at Resources/{VibrationDataPath}.asset");
                return;
            }

            _serializedData = new SerializedObject(_vibrationsData);

            var container = rootVisualElement.Q<VisualElement>("container-vibrations");
            container.Clear();
            var field = new PropertyField(_serializedData.FindProperty("Vibrations"), "Vibrations");
            field.Bind(_serializedData);
            container.Add(field);

            UpdateEnumPreview();
        }

        private void OnDisable()
        {
            if (_vibrationsData != null)
                EditorUtility.SetDirty(_vibrationsData);
        }

        private void UpdateEnumPreview()
        {
            if (_enumPreview == null) return;
            _enumPreview.value = string.Join(", ", Enum.GetNames(typeof(VibrationType)));
        }

        public void UpdateVibrationTypesAfterReload()
        {
            if (_vibrationsData == null) return;

            foreach (var v in _vibrationsData.Vibrations)
            {
                var sanitized = Sanitize(v.Name);
                v.VibrationType = Enum.TryParse(sanitized, out VibrationType parsed)
                    ? parsed
                    : VibrationType.Unknown;
            }

            EditorUtility.SetDirty(_vibrationsData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateEnum()
        {
            if (_vibrationsData == null)
            {
                Debug.LogError("[VibrationLibraryEditorWindow] VibrationsData not assigned or not loaded.");
                return;
            }

            _serializedData.ApplyModifiedProperties();
            GenerateEnumFile(_vibrationsData.Vibrations);
            AssignEnumTypes(_vibrationsData.Vibrations);
            EditorUtility.SetDirty(_vibrationsData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UpdateEnumPreview();
        }

        private void GenerateEnumFile(List<VibrationData> list)
        {
            var names = list
                .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                .Select(v => Sanitize(v.Name))
                .Distinct()
                .ToList();

            using (var writer = new StreamWriter(EnumOutPutPath))
            {
                writer.WriteLine("using System;");
                writer.WriteLine();
                writer.WriteLine("namespace Code.Services.AudioVibrationFX.Vibration");
                writer.WriteLine("{");
                writer.WriteLine("    [Serializable]");
                writer.WriteLine("    public enum VibrationType");
                writer.WriteLine("    {");
                writer.WriteLine("        Unknown = -1,");
                for (int i = 0; i < names.Count; i++)
                    writer.WriteLine($"        {names[i]} = {i},");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Debug.Log($"[VibrationLibraryEditorWindow] VibrationType generated at {EnumOutPutPath}");
        }

        private void AssignEnumTypes(List<VibrationData> list)
        {
            foreach (var v in list)
            {
                var sanitized = Sanitize(v.Name);
                v.VibrationType = Enum.TryParse(sanitized, out VibrationType parsed)
                    ? parsed
                    : VibrationType.Unknown;
            }
        }

        private static string Sanitize(string name) =>
            name.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Trim();
    }
}
