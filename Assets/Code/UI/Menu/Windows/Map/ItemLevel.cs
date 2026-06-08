using System;
using Code.Services.AudioVibrationFX.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Menu.Windows.Map
{
    public class ItemLevel : MonoBehaviour
    {
        [SerializeField] private Image _mainImage;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _button;
        [Space(10)]
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _currentColor = Color.yellow;
        [SerializeField] private Color _unlockedNonCompletedColor = Color.white;
        
        private int _currentLevel;
        private int _currentChapter;
        
        private ISoundService _soundService;
        
        [Inject]
        private void Constructor(ISoundService soundService)
        {
            _soundService = soundService;
        }
        
        public event Action<int, int> LoadLevelEvent; 
        
        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        
        public void Initialize(int CurrentLevel, int CurrentChapter)
        {
            _currentLevel = CurrentLevel;
            _currentChapter = CurrentChapter;
         
            SetText(_currentLevel.ToString());
            
            UnsubscribeEvents();
            SubscribeEvents();
        }
        
        public void SetCurrent()
        {
            _mainImage.color = _currentColor;
            _button.interactable = true;
        }
        
        public void SetCompleted()
        {
            _mainImage.color = _completedColor;
            _button.interactable = true;
        }

        public void SetUnlockedNonCompleted()
        {
            _mainImage.color = _unlockedNonCompletedColor;
            _button.interactable = true;
        }
        
        public void SetLocked()
        {
            _mainImage.color = _lockedColor;
            _button.interactable = false;
        }
        
        private void SetText(string text)
        {
            _text.text = text;
        }
        
        private void SubscribeEvents()
        {
            _button.onClick.AddListener(OnLoadLevel);
        }
        
        private void UnsubscribeEvents()
        {
            _button.onClick.RemoveListener(OnLoadLevel);
        }
        
        private void OnLoadLevel()
        {
            LoadLevelEvent?.Invoke(_currentLevel, _currentChapter);
        }
    }
}