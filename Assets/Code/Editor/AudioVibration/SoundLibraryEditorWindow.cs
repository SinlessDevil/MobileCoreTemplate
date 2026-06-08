using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Services.AudioVibrationFX.Music;
using Code.Services.AudioVibrationFX.Sound;
using Code.StaticData.AudioVibration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Editor.AudioVibration
{
    public class SoundLibraryEditorWindow : EditorWindow
    {
        private const string Sound2DPath            = "Assets/Code/Services/AudioVibrationFX/Sound/Sound2DType.cs";
        private const string Sound3DPath            = "Assets/Code/Services/AudioVibrationFX/Sound/Sound3DType.cs";
        private const string MusicPath              = "Assets/Code/Services/AudioVibrationFX/Music/MusicType.cs";
        private const string SoundsDataResourcePath = "StaticData/Sounds/Sounds";

        private SoundsData _soundsData;
        private SerializedObject _serializedData;

        private TextField _sound2DPreview;
        private TextField _sound3DPreview;
        private TextField _musicPreview;

        [MenuItem("Tools/Audio Vibration Window/Sound Library", false, 2002)]
        private static void OpenWindow() => GetWindow<SoundLibraryEditorWindow>().Show();

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Code/Editor/AudioVibration/SoundLibraryEditorWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Code/Editor/AudioVibration/SoundLibraryEditorWindow.uss");

            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _sound2DPreview = rootVisualElement.Q<TextField>("tf-sound2d");
            _sound3DPreview = rootVisualElement.Q<TextField>("tf-sound3d");
            _musicPreview   = rootVisualElement.Q<TextField>("tf-music");

            _sound2DPreview.isReadOnly = true;
            _sound3DPreview.isReadOnly = true;
            _musicPreview.isReadOnly   = true;

            rootVisualElement.Q<Button>("btn-generate").clicked += GenerateEnums;

            LoadData();
        }

        private void LoadData()
        {
            _soundsData = Resources.Load<SoundsData>(SoundsDataResourcePath);

            if (_soundsData == null)
            {
                Debug.LogError("SoundsData not found at Resources/StaticData/Sounds/Sounds.asset");
                return;
            }

            _serializedData = new SerializedObject(_soundsData);

            BindList("container-2d",    "Sounds2DData", "2D Sounds");
            BindList("container-3d",    "Sounds3DData", "3D Sounds");
            BindList("container-music", "MusicData",    "Music");

            UpdateEnumPreviews();
        }

        private void BindList(string containerName, string propertyName, string label)
        {
            var container = rootVisualElement.Q<VisualElement>(containerName);
            container.Clear();
            var field = new PropertyField(_serializedData.FindProperty(propertyName), label);
            field.Bind(_serializedData);
            container.Add(field);
        }

        private void OnDisable()
        {
            if (_soundsData != null)
                EditorUtility.SetDirty(_soundsData);
        }

        private void UpdateEnumPreviews()
        {
            if (_sound2DPreview == null) return;
            _sound2DPreview.value = string.Join(", ", Enum.GetNames(typeof(Sound2DType)));
            _sound3DPreview.value = string.Join(", ", Enum.GetNames(typeof(Sound3DType)));
            _musicPreview.value   = string.Join(", ", Enum.GetNames(typeof(MusicType)));
        }

        public void UpdateSoundTypesAfterReload()
        {
            if (_soundsData == null) return;

            foreach (var s in _soundsData.Sounds2DData)
                s.Sound2DType = Enum.TryParse(s.Name, out Sound2DType t) ? t : Sound2DType.Unknown;
            foreach (var s in _soundsData.Sounds3DData)
                s.Sound3DType = Enum.TryParse(s.Name, out Sound3DType t) ? t : Sound3DType.Unknown;
            foreach (var m in _soundsData.MusicData)
                m.MusicType = Enum.TryParse(m.Name, out MusicType t) ? t : MusicType.Unknown;

            EditorUtility.SetDirty(_soundsData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateEnums()
        {
            if (_soundsData == null) return;

            _serializedData.ApplyModifiedProperties();

            GenerateEnumFileBase(Sound2DPath, "Sound2DType", "Sound",
                _soundsData.Sounds2DData, TypeSound.Sound2D);
            GenerateEnumFileBase(Sound3DPath, "Sound3DType", "Sound",
                _soundsData.Sounds3DData.Cast<SoundData>().ToList(), TypeSound.Sound3D);
            GenerateEnumFileBase(MusicPath, "MusicType", "Music",
                _soundsData.MusicData, TypeSound.Music);

            EditorUtility.SetDirty(_soundsData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateEnumFileBase(string enumPath, string enumName, string nameFolder,
            List<SoundData> soundList, TypeSound typeSound)
        {
            var names = soundList
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .Select(s => Sanitize(s.Name))
                .Distinct()
                .ToList();

            using (var writer = new StreamWriter(enumPath))
            {
                writer.WriteLine("using System;");
                writer.WriteLine();
                writer.WriteLine($"namespace Code.Services.AudioVibrationFX.{nameFolder}");
                writer.WriteLine("{");
                writer.WriteLine("    [Serializable]");
                writer.WriteLine($"    public enum {enumName}");
                writer.WriteLine("    {");
                writer.WriteLine("        Unknown = -1,");
                for (int i = 0; i < names.Count; i++)
                    writer.WriteLine($"        {names[i]} = {i},");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            foreach (var sound in soundList)
            {
                var sanitized = Sanitize(sound.Name);
                switch (typeSound)
                {
                    case TypeSound.Sound2D when Enum.TryParse(sanitized, out Sound2DType t): sound.Sound2DType = t; break;
                    case TypeSound.Sound3D when Enum.TryParse(sanitized, out Sound3DType t): sound.Sound3DType = t; break;
                    case TypeSound.Music   when Enum.TryParse(sanitized, out MusicType t):   sound.MusicType   = t; break;
                    default: throw new ArgumentOutOfRangeException(nameof(typeSound), typeSound, null);
                }
            }

            EditorUtility.SetDirty(_soundsData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{enumName} enum generated successfully!");
        }

        private static string Sanitize(string name) =>
            name.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Trim();

        private enum TypeSound { Sound2D, Sound3D, Music }
    }
}
