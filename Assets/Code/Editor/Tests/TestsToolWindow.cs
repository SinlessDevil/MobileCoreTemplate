using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace Code.Editor.Tests
{
    public class TestsToolWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Tests Tool Window", false, 2000)]
        private static void OpenWindow()
        {
            TestsToolWindow window = GetWindow<TestsToolWindow>("TestsToolWindow");
            window.position = new Rect(100, 100, 1600, 700);
            window.Show();
        }

        [FoldoutGroup("Grouped Edit Mode", expanded: false)] [TableList]
        public List<GroupedTestGroup> GroupedEditMode = new();

        [FoldoutGroup("Grouped Play Mode", expanded: false)] [TableList]
        public List<GroupedTestGroup> GroupedPlayMode = new();

        public void OnEnable() => RefreshTests();

        [Button("🔍 Find All Tests", ButtonSizes.Large), GUIColor(0.3f, 0.6f, 1f)]
        private void RefreshTests()
        {
            GroupedEditMode.Clear();
            GroupedPlayMode.Clear();

            TestRunnerApi api = new TestRunnerApi();
            api.RetrieveTestList(TestMode.EditMode, root => CollectTestsRecursive(root, TestPlatform.EditMode));
            api.RetrieveTestList(TestMode.PlayMode, root => CollectTestsRecursive(root, TestPlatform.PlayMode));
        }

        [Button("➕ Add [Ignore] Attribute Tests", ButtonSizes.Large), GUIColor(1f, 0.6f, 0.2f)]
        private void AddIgnoreAttributeTests()
        {
            IEnumerable<TestCaseConfig> testsToIgnore = GroupedEditMode.Concat(GroupedPlayMode)
                .SelectMany(g => g.Tests)
                .Where(t => !t.Enabled && t.IsChanged);
            AddIgnoreAttributes(testsToIgnore);
        }

        [Button("➖ Remove [Ignore] Attribute Tests", ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
        private void RemoveIgnoreAttributeTests()
        {
            IEnumerable<TestCaseConfig> testsToRestore = GroupedEditMode.Concat(GroupedPlayMode)
                .SelectMany(g => g.Tests)
                .Where(t => t.Enabled && t.IsChanged);
            RemoveIgnoreAttributes(testsToRestore);
        }

        [Button("📋 Open Test Runner", ButtonSizes.Large), GUIColor(0.8f, 0.8f, 1f)]
        private void OpenTestRunner()
        {
            Type testRunnerType =
                Type.GetType("UnityEditor.TestTools.TestRunner.TestRunnerWindow,UnityEditor.TestRunner");
            if (testRunnerType != null)
                GetWindow(testRunnerType, false, "Test Runner");
            else
                Debug.LogWarning("Test Runner Window not found. Make sure 'Test Framework' package is installed.");
        }

        private void AddIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            Dictionary<string, List<string>> testsByFile = new Dictionary<string, List<string>>();

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
                string file = kvp.Key;
                List<string> methodNames = kvp.Value;
                List<string> lines = File.ReadAllLines(file).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    foreach (var methodName in methodNames)
                    {
                        if (lines[i].Contains($" {methodName}("))
                        {
                            int attrStart = i - 1;
                            while (attrStart >= 0 && lines[attrStart].Trim().StartsWith("["))
                            {
                                attrStart--;
                            }

                            attrStart++;
                            
                            bool alreadyHasIgnore = false;
                            for (int j = attrStart; j < i; j++)
                            {
                                if (!lines[j].Contains("Ignore(")) 
                                    continue;
                                
                                alreadyHasIgnore = true;
                                break;
                            }

                            if (alreadyHasIgnore)
                                continue;
                            
                            for (int j = attrStart; j < i; j++)
                            {
                                if (!lines[j].Contains("[Test") && !lines[j].Contains("[UnityTest")) 
                                    continue;
                                
                                if (!lines[j].Contains("]")) 
                                    continue;
                                
                                lines[j] = lines[j].Replace("]", ", Ignore(\"Disabled via TestsToolWindow\")]");
                                modified = true;
                                Debug.Log($"[TestsTool] Added Ignore in method: {methodName}");
                                break;
                            }
                        }
                    }
                }

                if (modified) 
                    File.WriteAllLines(file, lines);
            }

            AssetDatabase.Refresh();
        }

        private void RemoveIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            Dictionary<string,List<string>> testsByFile = new Dictionary<string, List<string>>();

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
                string file = kvp.Key;
                List<string> methodNames = kvp.Value;
                List<string> lines = File.ReadAllLines(file).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    foreach (var methodName in methodNames)
                    {
                        if (lines[i].Contains($" {methodName}("))
                        {
                            int attrStart = i - 1;
                            while (attrStart >= 0 && lines[attrStart].Trim().StartsWith("["))
                            {
                                attrStart--;
                            }

                            attrStart++;

                            for (int j = attrStart; j < i; j++)
                            {
                                if (!lines[j].Contains("Ignore(\"Disabled via TestsToolWindow\")"))
                                    continue;
                                
                                lines[j] = lines[j].Replace(", Ignore(\"Disabled via TestsToolWindow\")", "");
                                modified = true;
                                Debug.Log($"[TestsTool] Deletes Ignore fot method: {methodName}");
                            }
                        }
                    }
                }

                if (modified) 
                    File.WriteAllLines(file, lines);
            }

            AssetDatabase.Refresh();
        }

        private void CollectTestsRecursive(ITestAdaptor adaptor, TestPlatform platform)
        {
            if (!adaptor.HasChildren && !string.IsNullOrEmpty(adaptor.FullName))
            {
                TestCaseConfig config = new TestCaseConfig(adaptor.FullName, adaptor.RunState != RunState.Ignored);
                string assemblyName = adaptor.TypeInfo?.Assembly?.GetName()?.Name ?? "Unknown";

                List<GroupedTestGroup> groupList = platform == TestPlatform.EditMode ? GroupedEditMode : GroupedPlayMode;
                GroupedTestGroup group = groupList.FirstOrDefault(g => g.AssemblyName == assemblyName);
                if (group == null)
                {
                    group = new GroupedTestGroup(assemblyName);
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