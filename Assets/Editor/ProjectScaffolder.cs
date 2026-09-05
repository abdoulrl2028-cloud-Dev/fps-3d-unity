#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using FPS.Player;

namespace FPS.EditorTools
{
    /// <summary>
    /// Organizes the professional project scaffolding:
    /// creates scenes (MainMenu, Level01..Level07, Test/TestScene), registers them in
    /// the Build Settings (Level01 first = main), builds the basic Player prefab and
    /// populates the test scene. Idempotent: safe to run many times.
    ///
    /// Auto-run is intentionally disabled: the level pipeline (FpsLevelBuilder) owns
    /// the scene list and build settings now. Run via menu FPS > Setup Project Structure.
    /// </summary>
    public static class ProjectScaffolder
    {
        private const string Level01Path = "Assets/Scenes/Level01.unity";
        private const string BasicPlayerPrefab = "Assets/Prefabs/Player/BasicPlayer.prefab";

        [MenuItem("FPS/Setup Project Structure")]
        public static void SetupProjectStructure()
        {
            try
            {
                PromoteLegacyScene();

                CreateScene("Assets/Scenes/MainMenu.unity");
                CreateScene("Assets/Scenes/Level01.unity");
                CreateScene("Assets/Scenes/Level02.unity");
                CreateScene("Assets/Scenes/Level03.unity");
                CreateScene("Assets/Scenes/Level04.unity");
                CreateScene("Assets/Scenes/Level05.unity");
                CreateScene("Assets/Scenes/Level06.unity");
                CreateScene("Assets/Scenes/Level07.unity");
                CreateScene("Assets/Scenes/Test/TestScene.unity");

                SetupBuildSettings();

                CreateBasicPlayerPrefab();
                PopulateTestScene();

                OpenLevel01();
                AssetDatabase.SaveAssets();

                Debug.Log("[FPS] Project structure ready: scenes, build settings and basic Player prefab.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[FPS] Project structure setup failed: " + e.Message + "\n" + e.StackTrace);
            }
        }

        private static void PromoteLegacyScene()
        {
            if (System.IO.File.Exists(Level01Path))
                return;

            string legacy = "Assets/Scenes/FpsGame.unity";
            if (System.IO.File.Exists(legacy))
            {
                AssetDatabase.MoveAsset(legacy, Level01Path);
                if (System.IO.File.Exists("Assets/Scenes/FpsGame/NavMesh.asset"))
                    AssetDatabase.MoveAsset("Assets/Scenes/FpsGame", "Assets/Scenes/Level01");
            }
        }

        private static void CreateScene(string path)
        {
            if (System.IO.File.Exists(path))
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(Level01Path, true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Test/TestScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level02.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level03.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level04.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level05.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level06.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level07.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        private static void CreateBasicPlayerPrefab()
        {
            if (System.IO.File.Exists(BasicPlayerPrefab))
                return;

            var root = new GameObject("Player");

            var cc = root.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);

            root.AddComponent<PlayerMovement>();
            var look = root.AddComponent<PlayerLook>();
            var healthConfig = root.AddComponent<PlayerHealth>();

            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(root.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";

            var so = new SerializedObject(look);
            so.FindProperty("playerCamera").objectReferenceValue = camGo.GetComponent<Camera>();
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, BasicPlayerPrefab);
            Object.DestroyImmediate(root);
        }

        private static void PopulateTestScene()
        {
            string testPath = "Assets/Scenes/Test/TestScene.unity";
            Scene test = EditorSceneManager.OpenScene(testPath, OpenSceneMode.Additive);

            if (GameObject.Find("Player") == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasicPlayerPrefab);
                if (prefab != null)
                {
                    var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, test);
                    player.transform.position = new Vector3(0f, 1f, 0f);
                }

                var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "Ground";
                ground.transform.position = new Vector3(0f, -1f, 0f);
                ground.transform.localScale = new Vector3(30f, 1f, 30f);
                SceneManager.MoveGameObjectToScene(ground, test);
            }

            EditorSceneManager.MarkSceneDirty(test);
            EditorSceneManager.SaveScene(test, testPath);
            EditorSceneManager.CloseScene(test, true);
        }

        private static void OpenLevel01()
        {
            if (System.IO.File.Exists(Level01Path))
                EditorSceneManager.OpenScene(Level01Path, OpenSceneMode.Single);
        }
    }
}
#endif