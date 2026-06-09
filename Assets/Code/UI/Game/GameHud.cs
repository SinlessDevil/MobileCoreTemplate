using System.Collections.Generic;
using Code.Services.StaticData;
using UnityEngine;
using Zenject;

namespace Code.UI.Game
{
    public class GameHud : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _debugObjects;
        
        private IStaticDataService _staticDataService; 
        
        [Inject]
        public void Constructor(IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
        }
        
        public void Initialize()
        {
            InitDebugObjects();
        }

        private void InitDebugObjects()
        {
            if (_staticDataService.GameConfig.DebugMode)
            {
                foreach (var debugObject in _debugObjects)
                {
                    debugObject.SetActive(true);
                }
            }
        }
    }
}