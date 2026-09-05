#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using FPS.Dialogue;
using FPS.Audio;
using FPS.AI;
using FPS.Enemies;
using FPS.Health;
using FPS.Levels;
using FPS.Managers;
using FPS.Player;
using FPS.Tools;
using FPS.UI;

namespace FPS.EditorTools
{
    /// <summary>
    /// Level workshop. Idempotent: creates the professional folder structure,
    /// placeholder prefabs (Resources/FPS/Prefabs), per-level dialogue assets,
    /// backs up Level01, then builds/augments every scene (Level01..Level09 +
    /// MainMenu) with the documented hierarchy. Existing full levels
    /// (01/02/03/08/09) are only augmented (never rebuilt); stub levels
    /// (04..07) get the complete structure. Manual Sketchfab imports are kept
    /// as marked placeholders (ModelPlaceholder) with URLs — nothing is invented.
    /// </summary>
    [InitializeOnLoad]
    public class LevelWorkshop
    {
        private static readonly string ScenesDir = "Assets/Scenes";
        private static readonly string BackupDir = ScenesDir + "/_Backups";
        private static readonly string ModelsDir = "Assets/Models";
        private static readonly string PrefabsResDir = "Assets/Resources/FPS/Prefabs";
        private static readonly string DialogueDir = "Assets/Dialogue";
        private const string MarkerName = "FPSLevelWorkshop";
        private const int LevelCount = 9;

        static LevelWorkshop()
        {
            EditorApplication.delayCall += RunOnce;
        }

        [MenuItem("FPS/Workshop/Preparar Levels 01-09 + MainMenu")]
        public static void RunFromMenu()
        {
            RunAll();
        }

        private static void RunOnce()
        {
            if (SessionState.GetBool("FPSWorkshopRanv1", false))
                return;
            SessionState.SetBool("FPSWorkshopRanv1", true);
            RunAll();
        }

        private static void RunAll()
        {
            try
            {
                Debug.Log("[FPS->Workshop] Iniciando preparação de níveis...");
                CreateFolderStructure();
                EnsureTag("Player");
                EnsureTag("Enemy");
                CreateDialogueAssets();
                CreatePlaceholderPrefabs();
                BackupLevel01();
                for (int lv = 1; lv <= LevelCount; lv++)
                    StageLevel(lv);
                StageMainMenu();
                EnsureBuildSettings();
                AssetDatabase.SaveAssets();
                Debug.Log("[FPS->Workshop] Concluído. Modelos Sketchfab aguardam importação manual (ver placeholders no inspetor).");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        // ------------------------------------------------------------- folders

        private static void CreateFolderStructure()
        {
            string[][] dirs = new string[][]
            {
                new string[] { ModelsDir, "Backgrounds" },
                new string[] { ModelsDir, "Players" },
                new string[] { ModelsDir, "Enemies" },
                new string[] { ModelsDir, "Weapons" },
                new string[] { "Assets/Prefabs", "Background" },
                new string[] { "Assets/Prefabs", "Player" },
                new string[] { "Assets/Prefabs", "Enemies" },
                new string[] { "Assets/Prefabs", "Weapons" },
                new string[] { "Assets/Prefabs", "Audio" },
                new string[] { "Assets/Audio", "Music" },
                new string[] { "Assets/Audio", "SFX" },
                new string[] { "Assets/Audio", "Ambient" },
                new string[] { "Assets/Animations", "Player" },
                new string[] { "Assets/Animations", "Enemies" },
                new string[] { "Assets/Animations", "Environment" },
                new string[] { "Assets/Dialogue" },
                new string[] { BackupDir },
                new string[] { "Assets/Scenes", "Test" },
                new string[] { PrefabsResDir }
            };
            for (int i = 0; i < dirs.Length; i++)
            {
                string path = dirs[i][0];
                for (int j = 1; j < dirs[i].Length; j++)
                    path += "/" + dirs[i][j];
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            string[] cats = { "Backgrounds", "Players", "Enemies", "Weapons" };
            for (int lv = 1; lv <= LevelCount; lv++)
            {
                for (int c = 0; c < cats.Length; c++)
                {
                    string p = ModelsDir + "/" + cats[c] + "/Level" + lv.ToString("D2");
                    if (!Directory.Exists(p))
                        Directory.CreateDirectory(p);
                }
            }
            AssetDatabase.Refresh();
        }

        // ------------------------------------------------------------- tags

        private static void EnsureTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    return;
            }
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------- assets

        private static void CreateDialogueAssets()
        {
            LoadOrCreate(DialogueDir + "/EnemyDetect.asset", "EnemyDetect",
                new DialogueData.Line[] {
                    MakeLine("Inimigo", "Você entrou no lugar errado."),
                    MakeLine("Inimigo", "Corra enquanto pode.")
                });

            LoadOrCreate(DialogueDir + "/Level01.asset", "Level01",
                new DialogueData.Line[] {
                    MakeLine("Player", "Quem está aí?"),
                    MakeLine("Inimigo", "Você entrou no lugar errado."),
                    MakeLine("Player", "Então vamos resolver isso.")
                });

            for (int lv = 2; lv <= LevelCount; lv++)
            {
                LoadOrCreate(DialogueDir + "/Level" + lv.ToString("D2") + ".asset", "Level" + lv.ToString("D2"),
                    new DialogueData.Line[] {
                        MakeLine("Player", "Missão em andamento..."),
                        MakeLine("Inimigo", "Aqui ninguém passa."),
                        MakeLine("Player", "Então vamos resolver isso.")
                    });
            }
        }

        private static DialogueData.Line MakeLine(string speaker, string text)
        {
            DialogueData.Line l = new DialogueData.Line();
            l.speaker = speaker;
            l.text = text;
            return l;
        }

        private static DialogueData LoadOrCreate(string path, string id, DialogueData.Line[] lines)
        {
            DialogueData existing = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (existing != null)
                return existing;
            DialogueData d = ScriptableObject.CreateInstance<DialogueData>();
            d.id = id;
            d.lines = lines;
            AssetDatabase.CreateAsset(d, path);
            Debug.Log("[FPS->Workshop] Diálogo criado: " + path);
            return d;
        }

        private static DialogueData LoadDialogue(string path)
        {
            return AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        }

        // ------------------------------------------------------------- prefabs

        private static void CreatePlaceholderPrefabs()
        {
            EnsurePrefab(PrefabsResDir + "/PlayerBase.prefab", BuildPlayerBase, "PlayerBase");
            EnsurePrefab(PrefabsResDir + "/EnemyBase.prefab", BuildEnemyBase, "EnemyBase");
            EnsurePrefab(PrefabsResDir + "/WeaponBase.prefab", BuildWeaponBase, "WeaponBase");
            EnsurePrefab(PrefabsResDir + "/BackgroundBase.prefab", BuildBackgroundBase, "BackgroundBase");
        }

        private static void EnsurePrefab(string path, System.Func<GameObject> builder, string label)
        {
            if (File.Exists(path))
                return;
            GameObject root = builder();
            if (root == null)
                return;
            bool ok = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();
            if (ok)
                Debug.Log("[FPS->Workshop] Placeholder criado: " + path);
            else
                Debug.LogWarning("[FPS->Workshop] Falha ao salvar " + label + " em " + path);
        }

        private static GameObject BuildPlayerBase()
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.7f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            go.AddComponent<FpsPlayerController>();
            SetProp(go, "maxHealth", 100);
            go.AddComponent<PlayerHealth>();
            go.AddComponent<PlayerWeapon>();
            go.AddComponent<PlaceholderWeapon>();

            var holder = new GameObject("WeaponHolder");
            holder.transform.SetParent(go.transform, false);
            holder.transform.localPosition = new Vector3(0f, 1.2f, 0.45f);

            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(go.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            camGo.AddComponent<AudioListener>();
            SetProp(go, "playerCamera", cam);

            AddVisualPlaceholder(go.transform, "BodyPlaceholder", "Player", "Assets/Models/Players/Level01/player_default");

            string tmpPath = PrefabsResDir + "/__tmp.prefab";
            if (File.Exists(tmpPath))
                AssetDatabase.DeleteAsset(tmpPath);
            PrefabUtility.SaveAsPrefabAsset(go, tmpPath);
            return (GameObject)AssetDatabase.LoadAssetAtPath(tmpPath, typeof(GameObject));
        }

        private static GameObject BuildEnemyBase()
        {
            var go = new GameObject("Enemy");
            go.tag = "Enemy";
            var agent = go.AddComponent<NavMeshAgent>();
            agent.radius = 0.4f;
            agent.height = 1.8f;
            agent.speed = 2.5f;
            SetProp(go, "maxHealth", 100);
            go.AddComponent<EnemyAI>();
            var ed = go.AddComponent<EnemyDialogue>();
            SetProp(ed, "detectDialogue", LoadDialogue(DialogueDir + "/EnemyDetect.asset"));
            go.AddComponent<AudioSource>();
            AddVisualPlaceholder(go.transform, "BodyPlaceholder", "Enemy", "Assets/Models/Enemies/Level01/rat_enemy");

            string tmpPath = PrefabsResDir + "/__tmp.prefab";
            if (File.Exists(tmpPath))
                AssetDatabase.DeleteAsset(tmpPath);
            PrefabUtility.SaveAsPrefabAsset(go, tmpPath);
            return (GameObject)AssetDatabase.LoadAssetAtPath(tmpPath, typeof(GameObject));
        }

        private static GameObject BuildWeaponBase()
        {
            var go = new GameObject("Weapon");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "BodyPlaceholder";
            cube.transform.SetParent(go.transform, false);
            cube.transform.localScale = new Vector3(0.65f, 0.16f, 0.14f);
            var ph = cube.AddComponent<ModelPlaceholder>();
            ph.category = "Weapon";
            ph.expectedImportPath = "Assets/Models/Weapons";
            string tmpPath = PrefabsResDir + "/__tmp.prefab";
            if (File.Exists(tmpPath))
                AssetDatabase.DeleteAsset(tmpPath);
            PrefabUtility.SaveAsPrefabAsset(go, tmpPath);
            return (GameObject)AssetDatabase.LoadAssetAtPath(tmpPath, typeof(GameObject));
        }

        private static GameObject BuildBackgroundBase()
        {
            var go = new GameObject("Background");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "BackdropMesh";
            cube.transform.SetParent(go.transform, false);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            var ph = cube.AddComponent<ModelPlaceholder>();
            ph.category = "Background";
            ph.expectedImportPath = "Assets/Models/Backgrounds";
            string tmpPath = PrefabsResDir + "/__tmp.prefab";
            if (File.Exists(tmpPath))
                AssetDatabase.DeleteAsset(tmpPath);
            PrefabUtility.SaveAsPrefabAsset(go, tmpPath);
            return (GameObject)AssetDatabase.LoadAssetAtPath(tmpPath, typeof(GameObject));
        }

        private static void AddVisualPlaceholder(Transform parent, string name, string category, string importPath)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = name;
            body.transform.SetParent(parent, false);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<MeshRenderer>().sharedMaterial =
                new Material(Shader.Find("Standard")) { color = new Color(0.5f, 0.55f, 0.6f) };

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "HeadPlaceholder";
            head.transform.SetParent(parent, false);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            head.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            Object.DestroyImmediate(head.GetComponent<Collider>());

            var ph = body.AddComponent<ModelPlaceholder>();
            ph.category = category;
            ph.expectedImportPath = importPath;
        }

        // ------------------------------------------------------------- scenes

        private static bool HasRoot(string name)
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == name)
                    return true;
            }
            return false;
        }

        private static bool HasRootWithPrefix(string prefix)
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name.StartsWith(prefix))
                    return true;
            }
            return false;
        }

        private static void AddMarker()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var obj = new GameObject(MarkerName);
            if (roots.Length > 0)
                obj.transform.SetParent(roots[0].transform, false);
        }

        private static void StageLevel(int lv)
        {
            string num = lv.ToString("D2");
            string path = ScenesDir + "/Level" + num + ".unity";
            if (!File.Exists(path))
            {
                Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(sc, path);
            }
            OpenScene(path);

            if (HasRoot(MarkerName))
            {
                Debug.Log("[FPS->Workshop] Level" + num + " já preparado.");
                return;
            }

            bool stub = SceneManager.GetActiveScene().rootCount <= 2;
            if (stub)
                BuildStubLevel(lv);
            else
                AugmentFullLevel(lv);

            AddMarker();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            Debug.Log("[FPS->Workshop] Level" + num + (stub ? " construído (estrutura completa)." : " aumentado (conteúdo preservado)."));
        }

        private static void BuildStubLevel(int lv)
        {
            CreateFloor("Environment");
            CreateBackgroundNode(ModelsDir + "/Backgrounds/Level" + lv.ToString("D2"));
            CreateAudioGroup("AudioManager");
            CreateDialogueNode();
            CreateLevelManagerNode("LevelManager");
            CreateExitZone("ExitZone", 30f);
            CreateUiNode("UI");
            CreateSpawnSystem(true);
            CreateNavigationNode("Navigation");
            CreateLight();
            CreateGameManagerNode("GameManager");
            CreateLevelSetup(lv);
        }

        private static void AugmentFullLevel(int lv)
        {
            string num = lv.ToString("D2");
            if (!HasRoot("AudioManager") && !HasRoot("GameAudio"))
                CreateAudioGroup("AudioManager");
            if (!HasRoot("DialogueManager"))
                CreateDialogueNode();
            if (!HasRoot("Enemies"))
            {
                var enemies = new GameObject("Enemies");
                SceneManager.MoveGameObjectToScene(enemies, SceneManager.GetActiveScene());
                var spawner = enemies.AddComponent<EnemySpawner>();
                SetProp(spawner, "enemyPrefab",
                    AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsResDir + "/EnemyBase.prefab"));
            }
            if (!HasRoot("EnemySpawnPoints"))
            {
                var pts = new GameObject("EnemySpawnPoints");
                SceneManager.MoveGameObjectToScene(pts, SceneManager.GetActiveScene());
                AddChild(pts, "EnemySpawn0", new Vector3(10f, 1f, 10f));
                AddChild(pts, "EnemySpawn1", new Vector3(-10f, 1f, 12f));
            }
            if (!HasRoot("LevelSetup"))
                CreateLevelSetup(lv);
            if (!HasRootWithPrefix("Background_"))
                CreateBackgroundMarker(num);
            if (!HasRoot("Navigation"))
                CreateNavigationNode("Navigation");
        }

        private static void CreateLevelSetup(int lv)
        {
            string num = lv.ToString("D2");
            var setup = new GameObject("LevelSetup");
            SceneManager.MoveGameObjectToScene(setup, SceneManager.GetActiveScene());
            var ls = setup.AddComponent<FPS.Levels.LevelSetup>();
            SetProp(ls, "levelIndex", lv);
            float diff = lv >= 6 ? 1.5f + (lv - 6) * 0.15f : 1f;
            SetProp(ls, "difficultyScale", diff);
            SetProp(ls, "introDialogue", LoadDialogue(DialogueDir + "/Level" + num + ".asset"));
        }

        private static void CreateBackgroundMarker(string num)
        {
            string path = ModelsDir + "/Backgrounds/Level" + num;
            var go = new GameObject("Background_" + "placeholder");
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            var ph = go.AddComponent<ModelPlaceholder>();
            ph.category = "Background";
            ph.expectedImportPath = path;
            if (num == "01")
                ph.sourceUrl = "https://sketchfab.com/3d-models/concrete-ready-mix-plant-4ed9ec07c52e440b8c7acfa13033c3c0";
        }

        private static void StageMainMenu()
        {
            string path = ScenesDir + "/MainMenu.unity";
            if (!File.Exists(path))
            {
                Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(sc, path);
            }
            OpenScene(path);
            if (HasRoot(MarkerName))
            {
                Debug.Log("[FPS->Workshop] MainMenu já preparado.");
                return;
            }

            if (!HasRoot("Directional Light"))
                CreateLight();

            var menu = new GameObject("Menu");
            SceneManager.MoveGameObjectToScene(menu, SceneManager.GetActiveScene());
            menu.AddComponent<MainMenuController>();
            menu.AddComponent<WeaponShopMenu>();
            menu.AddComponent<PlayerSelectionMenu>();
            menu.AddComponent<SettingsPanel>();

            if (!HasRoot("AudioManager") && !HasRoot("GameAudio"))
                CreateAudioGroup("AudioManager");
            if (!HasRoot("GameManager"))
                CreateGameManagerNode("GameManager");

            AddMarker();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            Debug.Log("[FPS->Workshop] MainMenu preparado.");
        }

        // ------------------------------------------------------------- nodes

        private static void CreateSpawnSystem(bool spawnPlayer)
        {
            Scene scene = SceneManager.GetActiveScene();

            var pts = new GameObject("EnemySpawnPoints");
            SceneManager.MoveGameObjectToScene(pts, scene);
            AddChild(pts, "EnemySpawn0", new Vector3(12f, 1f, 6f));
            AddChild(pts, "EnemySpawn1", new Vector3(-12f, 1f, 8f));
            AddChild(pts, "EnemySpawn2", new Vector3(14f, 1f, -8f));
            AddChild(pts, "EnemySpawn3", new Vector3(-8f, 1f, -12f));

            var enemies = new GameObject("Enemies");
            SceneManager.MoveGameObjectToScene(enemies, scene);
            var spawner = enemies.AddComponent<EnemySpawner>();
            SetProp(spawner, "enemyPrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsResDir + "/EnemyBase.prefab"));

            if (!spawnPlayer)
                return;

            var player = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsResDir + "/PlayerBase.prefab"), scene);
            if (player != null)
            {
                player.name = "Player";
                player.transform.position = new Vector3(0f, 1f, 0f);
            }

            var ps = new GameObject("PlayerSpawn");
            SceneManager.MoveGameObjectToScene(ps, scene);
            ps.transform.position = new Vector3(0f, 1f, 0f);
        }

        private static void CreateBackgroundNode(string importPath)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject bg = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsResDir + "/BackgroundBase.prefab"), scene);
            if (bg == null)
                return;
            bg.name = "Background";
            bg.transform.position = new Vector3(0f, 10f, -70f);
            bg.transform.localScale = new Vector3(100f, 40f, 2f);
            var ph = bg.GetComponentInChildren<ModelPlaceholder>();
            if (ph != null)
                ph.expectedImportPath = importPath;
        }

        private static void CreateAudioGroup(string name)
        {
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, SceneManager.GetActiveScene());
            root.AddComponent<AudioManager>();

            var music = new GameObject("Music");
            music.transform.SetParent(root.transform, false);
            music.AddComponent<MusicController>();

            var ambient = new GameObject("AmbientSounds");
            ambient.transform.SetParent(root.transform, false);
            ambient.AddComponent<AmbientSoundController>();

            var sfx = new GameObject("SFX");
            sfx.transform.SetParent(root.transform, false);
            sfx.AddComponent<SFXController>();
        }

        private static void CreateDialogueNode()
        {
            var go = new GameObject("DialogueManager");
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.AddComponent<DialogueManager>();
        }

        private static void CreateGameManagerNode(string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.AddComponent<GameManager>();
        }

        private static void CreateLevelManagerNode(string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.AddComponent<FPS.Levels.LevelManager>();
        }

        private static void CreateExitZone(string name, float z)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.transform.position = new Vector3(0f, 1f, z);
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(30f, 6f, 8f);
            go.AddComponent<FPS.Levels.ExitZone>();
        }

        private static void CreateUiNode(string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.AddComponent<PauseMenu>();
            go.AddComponent<GameOverMenu>();
            go.AddComponent<MissionCompleteMenu>();
            go.AddComponent<FpsHud>();
        }

        private static void CreateNavigationNode(string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            Debug.Log("[FPS->Workshop] " + name + ": bake o NavMesh após importar a geometria real.");
        }

        private static void CreateLight()
        {
            var go = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateFloor(string parentName)
        {
            var parent = new GameObject(parentName);
            SceneManager.MoveGameObjectToScene(parent, SceneManager.GetActiveScene());

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.position = new Vector3(0f, -1f, 0f);
            floor.transform.localScale = new Vector3(90f, 1f, 90f);
        }

        private static void AddChild(GameObject parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
        }

        private static void OpenScene(string path)
        {
            if (EditorSceneManager.GetActiveScene().path == path)
                return;
            EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
        }

        // ------------------------------------------------------------- settings

        private static void EnsureBuildSettings()
        {
            var entries = new List<EditorBuildSettingsScene>();
            entries.Add(new EditorBuildSettingsScene(ScenesDir + "/Level01.unity", true));
            if (File.Exists(ScenesDir + "/MainMenu.unity"))
                entries.Add(new EditorBuildSettingsScene(ScenesDir + "/MainMenu.unity", true));
            for (int lv = 2; lv <= LevelCount; lv++)
            {
                string p = ScenesDir + "/Level" + lv.ToString("D2") + ".unity";
                if (File.Exists(p))
                    entries.Add(new EditorBuildSettingsScene(p, true));
            }
            if (File.Exists(ScenesDir + "/Test/TestScene.unity"))
                entries.Add(new EditorBuildSettingsScene(ScenesDir + "/Test/TestScene.unity", true));
            EditorBuildSettings.scenes = entries.ToArray();
            Debug.Log("[FPS->Workshop] EditorBuildSettings: " + entries.Count + " cenas (Level01 = inicial).");
        }

        private static void BackupLevel01()
        {
            string src = ScenesDir + "/Level01.unity";
            if (!File.Exists(src))
                return;
            string dst = BackupDir + "/Level01_backup_original.unity";
            if (File.Exists(dst))
                return;
            if (!Directory.Exists(BackupDir))
                Directory.CreateDirectory(BackupDir);
            File.Copy(src, dst);

            string navSrc = ScenesDir + "/Level01";
            string navDst = BackupDir + "/Level01";
            if (Directory.Exists(navSrc) && !Directory.Exists(navDst))
            {
                Directory.CreateDirectory(navDst);
                foreach (string f in Directory.GetFiles(navSrc))
                    File.Copy(f, navDst + "/" + Path.GetFileName(f));
            }
            AssetDatabase.Refresh();
            Debug.Log("[FPS->Workshop] Backup do Level01 criado em " + BackupDir);
        }

        // ------------------------------------------------------------- serialized helpers

        private static void SetProp(Object target, string name, int value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning("[FPS] prop nula (int): " + name); return; }
            p.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetProp(Object target, string name, float value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning("[FPS] prop nula (float): " + name); return; }
            p.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetProp(Object target, string name, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning("[FPS] prop nula (obj): " + name); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif