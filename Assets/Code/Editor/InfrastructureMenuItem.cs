using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Code.Editor
{
    public class InfrastructureMenuItem
    {
        private const string GameRunnerPath = "Infrastructure/GameRunner";

        [MenuItem("GameObject/Infrastructure/GameRunner", false, 10)]
        public static void CreateSceneContext(MenuCommand menuCommand)
        {
            Object gameRunnerPrefab = Resources.Load(GameRunnerPath);
            GameObject root = PrefabUtility.InstantiatePrefab(gameRunnerPrefab) as GameObject;
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}