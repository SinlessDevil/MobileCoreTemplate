using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Code.UI.Menu.ButtonsNavigation
{
    public class ButtonNavigationHolder : MonoBehaviour
    {
        private readonly string ShopState = "Shop_Game_State";
        private readonly string MapState = "Play_Game_State";
        private readonly string MetaState = "Meta_Game_State";
        
        private const float Transition = 0.1f;
        
        [SerializeField] private List<ButtonNavigation> _buttonNavigations = new();
        [SerializeField] private Animator _animator;

        private MenuNavigationViewModel _viewModel;
        
        [Inject]
        private void Constructor(MenuNavigationViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        public event Action<TypeWindow> OpenedWindowEvent;
        
        private void OnValidate()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            
            if(_buttonNavigations.Count == 0)
            {
                _buttonNavigations.AddRange(GetComponentsInChildren<ButtonNavigation>());
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        public void Initialize(TypeWindow typeWindow = TypeWindow.Map)
        {
            InitButtonNavigation();
            UnsubscribeEvents();
            SubscribeEvents();
            _viewModel.Initialize(typeWindow);
            SetUpButtonState(_viewModel.SelectedWindow);
        }
        
        private void InitButtonNavigation()
        {
            foreach (ButtonNavigation buttonNavigation in _buttonNavigations)
            {
                buttonNavigation.Initialize();
            }
        }
        
        private void SubscribeEvents()
        {
            foreach (ButtonNavigation buttonNavigation in _buttonNavigations)
            {
                buttonNavigation.OpenedWindowEvent += OnOpenedWindow;
            }
        }
        
        private void UnsubscribeEvents()
        {
            foreach (ButtonNavigation buttonNavigation in _buttonNavigations)
            {
                buttonNavigation.OpenedWindowEvent -= OnOpenedWindow;
            }
        }
        
        private void OnOpenedWindow(TypeWindow typeWindow)
        {
            _viewModel.SelectWindow(typeWindow);
            
            SetUpButtonState(_viewModel.SelectedWindow);
            
            OpenedWindowEvent?.Invoke(_viewModel.SelectedWindow);
        }

        private void SetUpButtonState(TypeWindow typeWindow)
        {
            switch (typeWindow)
            {
                case TypeWindow.Map:
                    MapStateButton();
                    break;
                case TypeWindow.Shop:
                    ShopStateButton();
                    break;
                case TypeWindow.Meta:
                    MetaStateButton();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeWindow), typeWindow, null);
            }
        }
        
        private void ShopStateButton() => _animator.CrossFade(ShopState, Transition);

        private void MapStateButton() => _animator.CrossFade(MapState, Transition);
        
        private void MetaStateButton() => _animator.CrossFade(MetaState, Transition);
    }
}