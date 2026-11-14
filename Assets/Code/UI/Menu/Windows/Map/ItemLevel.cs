using System;
using TMPro;
using UnityEngine;

namespace Code.UI.Menu.Windows.Map
{
    public class ItemLevel : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image _mainImage;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private UnityEngine.UI.Button _button;
        [Space(10)]
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _currentColor = Color.yellow;
        [SerializeField] private Color _unlockedNonCompletedColor = Color.white;

        private LevelViewData _data;

        public event Action<int, int> LoadLevelEvent;

        public void Render(LevelViewData data)
        {
            _data = data;
            _text.text = data.Label;
            _mainImage.color = data.Status switch
            {
                LevelViewStatus.Current => _currentColor,
                LevelViewStatus.Completed => _completedColor,
                LevelViewStatus.Unlocked => _unlockedNonCompletedColor,
                _ => _lockedColor
            };
            _button.interactable = data.Status != LevelViewStatus.Locked;
            _button.onClick.RemoveListener(OnLoadLevel);
            _button.onClick.AddListener(OnLoadLevel);
        }

        private void OnDestroy() => _button.onClick.RemoveListener(OnLoadLevel);

        private void OnLoadLevel() => LoadLevelEvent?.Invoke(_data.LevelNumber, _data.ChapterNumber);
    }
}
