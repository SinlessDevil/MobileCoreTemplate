using System;

namespace Code.Editor.Tests
{
    [Serializable]
    public class TestCaseConfig
    {
        public string FullName;
        public bool IsChanged;

        private bool _enabled;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled  = value;
                IsChanged = true;
            }
        }

        public TestCaseConfig(string fullName, bool enabled)
        {
            FullName = fullName;
            _enabled = enabled;
        }
    }
}
