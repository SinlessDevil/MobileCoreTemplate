using System;
using System.Collections.Generic;

namespace Code.Editor.Tests
{
    [Serializable]
    public class GroupedTestGroup
    {
        public string AssemblyName;
        public List<TestCaseConfig> Tests = new();

        public GroupedTestGroup(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}
