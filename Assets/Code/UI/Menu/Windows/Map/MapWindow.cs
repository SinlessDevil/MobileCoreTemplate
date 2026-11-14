using System.Collections.Generic;
using System.Threading;
using Code.Services.Factories.UIFactory;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Menu.Windows.Map
{
    public class MapWindow : BaseWindow
    {
        private const float ChapterFadeDuration = 0.2f;

        [SerializeField] private TMP_Text _chapterName;
        [SerializeField] private UnityEngine.UI.GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private List<ButtonSwipeChapter> _buttonsSwipeChapter;
        [SerializeField] private List<CanvasGroup> _canvasGroups;

        private readonly List<ItemLevel> _levelPool = new();
        private MapViewModel _viewModel;
        private IUIFactory _uiFactory;
        private Sequence _fade;
        private bool _initialized;
        private bool _rendering;

        [Inject]
        public void Constructor(MapViewModel viewModel, IUIFactory uiFactory)
        {
            _viewModel = viewModel;
            _uiFactory = uiFactory;
        }

        public override void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            foreach (ButtonSwipeChapter button in _buttonsSwipeChapter)
            {
                button.Initialize();
                button.SwipedChapterEvent += OnSwipeChapter;
            }

            RenderAsync(_viewModel.Initialize(), false, destroyCancellationToken).Forget();
        }

        private void OnDestroy()
        {
            _fade?.Kill();
            foreach (ButtonSwipeChapter button in _buttonsSwipeChapter)
                button.SwipedChapterEvent -= OnSwipeChapter;

            foreach (ItemLevel item in _levelPool)
                item.LoadLevelEvent -= StartLevel;
        }

        private void OnSwipeChapter(TypeSwipe direction)
        {
            if (!_rendering && _viewModel.TryMoveChapter(direction, out MapViewData data))
                RenderAsync(data, true, destroyCancellationToken).Forget();
        }

        private async UniTask RenderAsync(MapViewData data, bool animate, CancellationToken cancellationToken)
        {
            _rendering = true;
            foreach (ButtonSwipeChapter button in _buttonsSwipeChapter)
                button.SetInteractable(false);

            try
            {
                if (animate)
                    await FadeToAsync(0f, cancellationToken);

                while (_levelPool.Count < data.Levels.Count)
                {
                    ItemLevel item = await _uiFactory.CreateItemLevel(_gridLayoutGroup.transform, cancellationToken);
                    item.LoadLevelEvent += StartLevel;
                    item.gameObject.SetActive(false);
                    _levelPool.Add(item);
                }

                _chapterName.text = data.ChapterTitle;
                for (int index = 0; index < data.Levels.Count; index++)
                {
                    ItemLevel item = _levelPool[index];
                    item.Render(data.Levels[index]);
                    item.gameObject.SetActive(true);
                }

                for (int index = data.Levels.Count; index < _levelPool.Count; index++)
                    _levelPool[index].gameObject.SetActive(false);

                foreach (ButtonSwipeChapter button in _buttonsSwipeChapter)
                    button.gameObject.SetActive(button.Direction == TypeSwipe.Left ? data.CanSwipeLeft : data.CanSwipeRight);

                if (animate)
                    await FadeToAsync(1f, cancellationToken);
            }
            finally
            {
                _rendering = false;
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (animate)
                    {
                        foreach (CanvasGroup group in _canvasGroups)
                            group.alpha = 1f;
                    }
                    foreach (ButtonSwipeChapter button in _buttonsSwipeChapter)
                        button.SetInteractable(true);
                }
            }
        }

        private async UniTask FadeToAsync(float alpha, CancellationToken cancellationToken)
        {
            if (_canvasGroups.Count == 0)
                return;

            Sequence fade = DOTween.Sequence();
            _fade = fade;
            foreach (CanvasGroup group in _canvasGroups)
                fade.Join(group.DOFade(alpha, ChapterFadeDuration).SetEase(Ease.Linear));

            await UniTask.WaitUntil(() => !fade.IsActive(), cancellationToken: cancellationToken);
            _fade = null;
        }

        private void StartLevel(int levelNumber, int chapterNumber)
        {
            if (!_rendering)
                _viewModel.StartLevel(levelNumber, chapterNumber);
        }
    }
}
