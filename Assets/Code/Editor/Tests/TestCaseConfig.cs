using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Editor.Tests
{
    [Serializable]
    public class TestCaseConfig
    {
        [TableColumnWidth(250, Resizable = true)] [ReadOnly]
        public string FullName;

        [GUIColor(0.2f, 0.8f, 0.2f)]
        [BoxGroup("Enabled", showLabel: false)]
        [TableColumnWidth(150, Resizable = false)]
        [ShowInInspector]
        [field: SerializeField]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    IsChanged = true;
                }
            }
        }

        [GUIColor(1f, 0f, 0)]
        [BoxGroup("IsChanged", showLabel: false)]
        [TableColumnWidth(150, Resizable = false)]
        [ShowInInspector, ReadOnly, ShowIf(nameof(IsChanged))]
        public bool IsChanged = false;

        private bool _enabled;

        public TestCaseConfig(string fullName, bool enabled)
        {
            FullName = fullName;
            _enabled = enabled;
        }
    }
}