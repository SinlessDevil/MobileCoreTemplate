using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Code.Editor.Tests
{
    public class TestsToolWindow : EditorWindow
    {
        [MenuItem("Tools/Tests Tool Window", false, 2000)]
        private static void OpenWindow()
        {
            var window = GetWindow<TestsToolWindow>("TestsToolWindow");
            window.position = new Rect(100, 100, 1600, 700);
            window.Show();
        }

        private readonly List<GroupedTestGroup> _editModeGroups = new();
        private readonly List<GroupedTestGroup> _playModeGroups = new();

        private Foldout _foldoutEdit;
        private Foldout _foldoutPlay;

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Code/Editor/Tests/TestsToolWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Code/Editor/Tests/TestsToolWindow.uss");

            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _foldoutEdit = rootVisualElement.Q<Foldout>("foldout-edit");
            _foldoutPlay = rootVisualElement.Q<Foldout>("foldout-play");

            rootVisualElement.Q<Button>("btn-find").clicked          += RefreshTests;
            rootVisualElement.Q<Button>("btn-add-ignore").clicked    += AddIgnoreAttributeTests;
            rootVisualElement.Q<Button>("btn-remove-ignore").clicked += RemoveIgnoreAttributeTests;
            rootVisualElement.Q<Button>("btn-test-runner").clicked   += OpenTestRunner;

            RefreshTests();
        }

        private void RefreshTests()
        {
            _editModeGroups.Clear();
            _playModeGroups.Clear();
            _foldoutEdit?.Clear();
            _foldoutPlay?.Clear();

            var api = new TestRunnerApi();
            api.RetrieveTestList(TestMode.EditMode, root =>
            {
                CollectTestsRecursive(root, TestPlatform.EditMode);
                RebuildGroupUI(_foldoutEdit, _editModeGroups);
            });
            api.RetrieveTestList(TestMode.PlayMode, root =>
            {
                CollectTestsRecursive(root, TestPlatform.PlayMode);
                RebuildGroupUI(_foldoutPlay, _playModeGroups);
            });
        }

        private void RebuildGroupUI(Foldout foldout, List<GroupedTestGroup> groups)
        {
            if (foldout == null) return;
            foldout.Clear();

            foreach (var group in groups)
            {
                var groupFoldout = new Foldout { text = group.AssemblyName, value = false };
                groupFoldout.AddToClassList("group-foldout");

                foreach (var test in group.Tests)
                    groupFoldout.Add(BuildTestRow(test));

                foldout.Add(groupFoldout);
            }
        }

        private VisualElement BuildTestRow(TestCaseConfig test)
        {
            var row = new VisualElement();
            row.AddToClassList("test-row");

            var nameLabel = new Label(test.FullName);
            nameLabel.AddToClassList("test-name");

            var toggle = new Toggle { value = test.Enabled };
            toggle.AddToClassList("test-toggle");
            ApplyToggleStyle(toggle, test.Enabled);

            var changedLabel = new Label("Changed");
            changedLabel.AddToClassList("label-changed");
            changedLabel.EnableInClassList("hidden", !test.IsChanged);

            toggle.RegisterValueChangedCallback(evt =>
            {
                test.Enabled = evt.newValue;
                ApplyToggleStyle(toggle, evt.newValue);
                changedLabel.EnableInClassList("hidden", !test.IsChanged);
            });

            row.Add(nameLabel);
            row.Add(toggle);
            row.Add(changedLabel);
            return row;
        }

        private static void ApplyToggleStyle(Toggle toggle, bool enabled)
        {
            toggle.EnableInClassList("toggle-enabled",  enabled);
            toggle.EnableInClassList("toggle-disabled", !enabled);
        }

        private void AddIgnoreAttributeTests()
        {
            var tests = _editModeGroups.Concat(_playModeGroups)
                .SelectMany(g => g.Tests)
                .Where(t => !t.Enabled && t.IsChanged);
            AddIgnoreAttributes(tests);
        }

        private void RemoveIgnoreAttributeTests()
        {
            var tests = _editModeGroups.Concat(_playModeGroups)
                .SelectMany(g => g.Tests)
                .Where(t => t.Enabled && t.IsChanged);
            RemoveIgnoreAttributes(tests);
        }

        private void OpenTestRunner()
        {
            var type = Type.GetType("UnityEditor.TestTools.TestRunner.TestRunnerWindow,UnityEditor.TestRunner");
            if (type != null)
                GetWindow(type, false, "Test Runner");
            else
                Debug.LogWarning("Test Runner Window not found. Make sure 'Test Framework' package is installed.");
        }

        private void AddIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            var testsByFile = new Dictionary<string, List<string>>();

            foreach (var test in tests)
            {
                string methodName = test.FullName.Split('.').Last();
                string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (!File.ReadAllText(file).Contains($" {methodName}("))
                        continue;

                    if (!testsByFile.ContainsKey(file))
                        testsByFile[file] = new List<string>();

                    testsByFile[file].Add(methodName);
                    break;
                }
            }

            foreach (var kvp in testsByFile)
            {
                var lines    = File.ReadAllLines(kvp.Key).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    foreach (var methodName in kvp.Value)
                    {
                        if (!lines[i].Contains($" {methodName}(")) continue;

                        int attrStart = i - 1;
                        while (attrStart >= 0 && lines[attrStart].Trim().StartsWith("["))
                            attrStart--;
                        attrStart++;

                        bool alreadyIgnored = false;
                        for (int j = attrStart; j < i; j++)
                        {
                            if (!lines[j].Contains("Ignore(")) continue;
                            alreadyIgnored = true;
                            break;
                        }

                        if (alreadyIgnored) continue;

                        for (int j = attrStart; j < i; j++)
                        {
                            if ((!lines[j].Contains("[Test") && !lines[j].Contains("[UnityTest"))
                                || !lines[j].Contains("]")) continue;

                            lines[j]  = lines[j].Replace("]", ", Ignore(\"Disabled via TestsToolWindow\")]");
                            modified  = true;
                            Debug.Log($"[TestsTool] Added Ignore in method: {methodName}");
                            break;
                        }
                    }
                }

                if (modified) File.WriteAllLines(kvp.Key, lines);
            }

            AssetDatabase.Refresh();
        }

        private void RemoveIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            var testsByFile = new Dictionary<string, List<string>>();

            foreach (var test in tests)
            {
                string methodName = test.FullName.Split('.').Last();
                string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (!File.ReadAllText(file).Contains($" {methodName}("))
                        continue;

                    if (!testsByFile.ContainsKey(file))
                        testsByFile[file] = new List<string>();

                    testsByFile[file].Add(methodName);
                    break;
                }
            }

            foreach (var kvp in testsByFile)
            {
                var lines    = File.ReadAllLines(kvp.Key).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    foreach (var methodName in kvp.Value)
                    {
                        if (!lines[i].Contains($" {methodName}(")) continue;

                        int attrStart = i - 1;
                        while (attrStart >= 0 && lines[attrStart].Trim().StartsWith("["))
                            attrStart--;
                        attrStart++;

                        for (int j = attrStart; j < i; j++)
                        {
                            if (!lines[j].Contains("Ignore(\"Disabled via TestsToolWindow\")")) continue;

                            lines[j]  = lines[j].Replace(", Ignore(\"Disabled via TestsToolWindow\")", "");
                            modified  = true;
                            Debug.Log($"[TestsTool] Removed Ignore for method: {methodName}");
                        }
                    }
                }

                if (modified) File.WriteAllLines(kvp.Key, lines);
            }

            AssetDatabase.Refresh();
        }

        private void CollectTestsRecursive(ITestAdaptor adaptor, TestPlatform platform)
        {
            if (!adaptor.HasChildren && !string.IsNullOrEmpty(adaptor.FullName))
            {
                var config       = new TestCaseConfig(adaptor.FullName, adaptor.RunState != RunState.Ignored);
                string assembly  = adaptor.TypeInfo?.Assembly?.GetName()?.Name ?? "Unknown";
                var groupList    = platform == TestPlatform.EditMode ? _editModeGroups : _playModeGroups;
                var group        = groupList.FirstOrDefault(g => g.AssemblyName == assembly);

                if (group == null)
                {
                    group = new GroupedTestGroup(assembly);
                    groupList.Add(group);
                }

                group.Tests.Add(config);
            }
            else
            {
                foreach (var child in adaptor.Children)
                    CollectTestsRecursive(child, platform);
            }
        }
    }
}
