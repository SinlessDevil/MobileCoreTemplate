using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Code.Editor.Tests
{
    [Serializable]
    public class GroupedTestGroup
    {
        [TableColumnWidth(175, Resizable = false)] [ReadOnly]
        public string AssemblyName;

        [TableList] public List<TestCaseConfig> Tests = new();

        public GroupedTestGroup(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}