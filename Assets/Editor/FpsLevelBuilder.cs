#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UI;
using FPS.Weapons;
using FPS.Levels;
using System.Collections.Generic;

namespace FPS.EditorTools
{
    /// <summary>
    /// Fills every level scene with a complete, playable layout:
    /// ground, buildings, cover, trees/cars/NPCs, Poly Haven props, pickups,
    /// checkpoints, enemies, exit zone, HUD, GameManager, LevelManager, lighting,
    /// audio and baked NavMesh. Also builds reusable prefabs and the MainMenu.
    /// Runs automatically once per session (and via menu FPS > Build All Levels).
    /// </summary>
    [InitializeOnLoad]
    public static class FpsLevelBuilder
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
        private static bool _ran;

        static FpsLevelBuilder()
        {
            EditorApplication.delayCall += AutoRun;
        }

        [DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            EditorApplication.delayCall += AutoRun;
        }

        private static void AutoRun()
        {
            if (_ran || Application.isPlaying || BuildPipeline.isBuildingPlayer)
                return;
            _ran = true;
            try
            {
                BuildAll();
            }
            catch (System.Exception e)
            {
                Debug.LogError("[FPS] Level pipeline failed: " + e + "\n" + e.StackTrace);
            }
        }

        // ------------------------------------------------------------------ //
        //  PUBLIC ENTRY
        // ------------------------------------------------------------------ //

        [MenuItem("FPS/Build All Levels")]
        public static void BuildAll()
        {
            FpsAudioGenerator.GenerateAll();
            ModelReimport.EnsureModelsImported();

            BuildReusablePrefabs();

            int built = 0;
            foreach (var spec in Specs)
            {
                if (BuildLevelScene(spec))
                    built++;
            }
            if (IntegrateLevel01())
                built++;

            if (BuildMainMenu())
                built++;

            PatchBuildSettings();
            AssetDatabase.SaveAssets();

            string list = "";
            foreach (var spec in Specs)
                list += spec.Name.Replace("Level", "Level ") + "  ";

            Debug.Log("[FPS] Filled " + built + " scene(s): " + list + "MainMenu | Progresso salvo via PlayerPrefs (FPS_UnlockedLevel).");
            OpenMainMenu();
        }

        private static void OpenMainMenu()
        {
            string path = "Assets/Scenes/MainMenu.unity";
            if (System.IO.File.Exists(path))
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        // ------------------------------------------------------------------ //
        //  LEVEL SPECS (Levels 02..09)
        // ------------------------------------------------------------------ //

        private class LevelSpec
        {
            public string Name;
            public float HalfW;          // x half-extent of the playable area
            public float HalfD;          // z half-extent
            public Color GroundColor;
            public Color BuildingColor;
            public Color BuildingAlt;
            public Color LightColor;
            public float LightIntensity;
            public Color AmbientColor;
            public bool Night;
            public int Enemies;
            public bool Boss;
            public int RequiredItems;
            public int Checkpoints;
            public int Trees;
            public int Cars;
            public int Npcs;
            public int Crates;
            public int Walls;
        }

        private static readonly LevelSpec[] Specs =
        {
            new LevelSpec {
                Name="Level02", HalfW=55f, HalfD=55f,
                GroundColor=new Color(0.30f,0.34f,0.38f), BuildingColor=new Color(0.35f,0.38f,0.42f),
                BuildingAlt=new Color(0.45f,0.40f,0.36f), LightColor=new Color(0.95f,0.92f,0.85f),
                LightIntensity=1.0f, AmbientColor=new Color(0.45f,0.48f,0.55f), Night=false,
                Enemies=6, Boss=false, RequiredItems=1, Checkpoints=0, Trees=4, Cars=4, Npcs=0,
                Crates=6, Walls=2 },
            new LevelSpec {
                Name="Level03", HalfW=70f, HalfD=60f,
                GroundColor=new Color(0.44f,0.40f,0.32f), BuildingColor=new Color(0.55f,0.45f,0.32f),
                BuildingAlt=new Color(0.45f,0.42f,0.38f), LightColor=new Color(1f,0.94f,0.82f),
                LightIntensity=1.15f, AmbientColor=new Color(0.6f,0.56f,0.48f), Night=false,
                Enemies=9, Boss=false, RequiredItems=0, Checkpoints=1, Trees=3, Cars=0, Npcs=0,
                Crates=12, Walls=4 },
            new LevelSpec {
                Name="Level04", HalfW=60f, HalfD=65f,
                GroundColor=new Color(0.42f,0.36f,0.30f), BuildingColor=new Color(0.62f,0.45f,0.32f),
                BuildingAlt=new Color(0.40f,0.42f,0.55f), LightColor=new Color(1f,0.78f,0.55f),
                LightIntensity=0.95f, AmbientColor=new Color(0.55f,0.42f,0.42f), Night=false,
                Enemies=8, Boss=false, RequiredItems=0, Checkpoints=1, Trees=5, Cars=3, Npcs=2,
                Crates=4, Walls=2 },
            new LevelSpec {
                Name="Level05", HalfW=70f, HalfD=95f,
                GroundColor=new Color(0.30f,0.32f,0.36f), BuildingColor=new Color(0.42f,0.46f,0.55f),
                BuildingAlt=new Color(0.50f,0.42f,0.40f), LightColor=new Color(0.92f,0.93f,1f),
                LightIntensity=1.05f, AmbientColor=new Color(0.45f,0.5f,0.6f), Night=false,
                Enemies=11, Boss=false, RequiredItems=2, Checkpoints=2, Trees=4, Cars=3, Npcs=0,
                Crates=8, Walls=3 },
            new LevelSpec {
                Name="Level06", HalfW=80f, HalfD=70f,
                GroundColor=new Color(0.34f,0.44f,0.32f), BuildingColor=new Color(0.50f,0.55f,0.45f),
                BuildingAlt=new Color(0.42f,0.48f,0.55f), LightColor=new Color(1f,0.97f,0.88f),
                LightIntensity=1.2f, AmbientColor=new Color(0.55f,0.6f,0.5f), Night=false,
                Enemies=10, Boss=false, RequiredItems=1, Checkpoints=1, Trees=10, Cars=3, Npcs=0,
                Crates=6, Walls=3 },
            new LevelSpec {
                Name="Level07", HalfW=75f, HalfD=75f,
                GroundColor=new Color(0.20f,0.21f,0.25f), BuildingColor=new Color(0.25f,0.28f,0.36f),
                BuildingAlt=new Color(0.32f,0.30f,0.28f), LightColor=new Color(0.75f,0.85f,1f),
                LightIntensity=0.35f, AmbientColor=new Color(0.16f,0.18f,0.28f), Night=true,
                Enemies=13, Boss=false, RequiredItems=0, Checkpoints=2, Trees=3, Cars=2, Npcs=0,
                Crates=12, Walls=5 },
            new LevelSpec {
                Name="Level08", HalfW=55f, HalfD=55f,
                GroundColor=new Color(0.60f,0.45f,0.68f), BuildingColor=new Color(0.70f,0.35f,0.60f),
                BuildingAlt=new Color(0.55f,0.40f,0.75f), LightColor=new Color(1f,0.85f,1f),
                LightIntensity=1.15f, AmbientColor=new Color(0.6f,0.45f,0.7f), Night=false,
                Enemies=8, Boss=true, RequiredItems=1, Checkpoints=1, Trees=6, Cars=0, Npcs=0,
                Crates=8, Walls=4 },
            new LevelSpec {
                Name="Level09", HalfW=95f, HalfD=95f,
                GroundColor=new Color(0.30f,0.30f,0.32f), BuildingColor=new Color(0.42f,0.38f,0.33f),
                BuildingAlt=new Color(0.33f,0.40f,0.45f), LightColor=new Color(1f,0.92f,0.80f),
                LightIntensity=1.0f, AmbientColor=new Color(0.5f,0.45f,0.4f), Night=false,
                Enemies=12, Boss=true, RequiredItems=2, Checkpoints=3, Trees=8, Cars=3, Npcs=0,
                Crates=14, Walls=6 }
        };

        // ------------------------------------------------------------------ //
        //  GENERIC SCENE HELPERS
        // ------------------------------------------------------------------ //

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int lastSlash = path.LastIndexOf('/');
            string parent = lastSlash > 0 ? path.Substring(0, lastSlash) : "";
            string folder = path.Substring(lastSlash + 1);
            if (lastSlash > 0)
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string category = "Environment";
            if (name.Contains("View"))
                category = "Weapons";
            else if (name.StartsWith("Enemy"))
                category = "Characters";

            string path = "Assets/Materials/" + category + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                EnsureFolder("Assets/Materials/" + category);
                mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void MarkNavStatic(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic);
        }

        private static void BakeNavMesh()
        {
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        }

        private static void SetLighting(LevelSpec spec)
        {
            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = spec.LightIntensity;
            light.color = spec.LightColor;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = spec.AmbientColor;

            if (spec.Night)
            {
                foreach (Vector3 p in new[] { new Vector3(-20f, 4f, -20f), new Vector3(20f, 4f, 20f), new Vector3(0f, 4f, 40f) })
                {
                    var gl = new GameObject("NightLight");
                    gl.transform.position = p;
                    var pl = gl.AddComponent<Light>();
                    pl.type = LightType.Point;
                    pl.range = 30f;
                    pl.intensity = 6f;
                    pl.color = new Color(0.6f, 0.7f, 1f);
                }
            }
        }

        private static GameObject CreateGround(LevelSpec spec)
        {
            float w = spec.HalfW * 2f + 4f;
            float d = spec.HalfD * 2f + 4f;
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.tag = "Ground";
            ground.transform.localScale = new Vector3(w, 1f, d);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial(spec.Name + "_Ground", spec.GroundColor);
            ground.isStatic = true;
            MarkNavStatic(ground);
            return ground;
        }

        // ------------------------------------------------------------------ //
        //  PREFAB FACTORY
        // ------------------------------------------------------------------ //

        private static void BuildReusablePrefabs()
        {
            EnsureFolder("Assets/Prefabs/Player");
            EnsureFolder("Assets/Prefabs/Enemies");
            EnsureFolder("Assets/Prefabs/NPC");
            EnsureFolder("Assets/Prefabs/Vehicles");
            EnsureFolder("Assets/Prefabs/Weapons");
            EnsureFolder("Assets/Prefabs/Environment");
            EnsureFolder("Assets/Prefabs/Props");

            BuildPlayerPrefab();
            BuildEnemyPrefabs();
            BuildWeaponPrefabs();
            BuildEnvironmentPrefabs();
        }

        private static bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        private static void BuildPlayerPrefab()
        {
            if (FileExists(PlayerPrefabPath))
                return;

            var root = new GameObject("Player");
            root.tag = "Player";

            var cc = root.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 1f, 0f);

            root.AddComponent<FPS.Player.FpsPlayerController>();
            var health = root.AddComponent<FPS.Health.HealthSystem>();
            root.AddComponent<FPS.Player.PlayerDeathHandler>();

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(root.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();

            var controller = root.GetComponent<FPS.Player.FpsPlayerController>();
            var pSo = new SerializedObject(controller);
            pSo.FindProperty("playerCamera").objectReferenceValue = cam;
            pSo.ApplyModifiedProperties();

            var rig = new GameObject("WeaponsRig");
            rig.transform.SetParent(camGo.transform, false);
            rig.transform.localPosition = new Vector3(0.35f, -0.3f, 0.55f);

            var wc = root.AddComponent<FPS.Weapons.WeaponController>();
            var wSo = new SerializedObject(wc);
            wSo.FindProperty("aimCamera").objectReferenceValue = cam;
            wSo.FindProperty("muzzlePoint").objectReferenceValue = camGo.transform;

            GameObject spark = CreateEffectBall("HitSpark", new Color(1f, 0.8f, 0.2f), root.transform);
            GameObject blood = CreateEffectBall("BloodImpact", new Color(0.8f, 0.05f, 0.05f), root.transform);
            wSo.FindProperty("hitSpark").objectReferenceValue = spark;
            wSo.FindProperty("bloodImpact").objectReferenceValue = blood;
            wSo.ApplyModifiedProperties();

            WeaponData pistol = GetWeaponData("Pistol", 12, 100f, 20, 60, 0.25f, 1.2f, 0f, 1);
            WeaponData rifle = GetWeaponData("Rifle", 30, 120f, 8, 120, 0.12f, 2f, 0.2f, 2);
            WeaponData shotgun = GetWeaponData("Shotgun", 40, 40f, 6, 24, 0.9f, 2.5f, 3f, 3, 6);

            BuildWeaponView(rig.transform, wc, "PistolView", pistol, new Vector3(0f, 0f, 0.05f));
            BuildWeaponView(rig.transform, wc, "RifleView", rifle, new Vector3(0f, 0f, 0f));
            BuildWeaponView(rig.transform, wc, "ShotgunView", shotgun, new Vector3(0f, 0f, -0.05f));

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateEffectBall(string name, Color color, Transform parent)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = name;
            ball.transform.SetParent(parent, false);
            ball.transform.localScale = Vector3.one * 0.12f;
            ball.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "Mat", color);
            var c = ball.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
            ball.SetActive(false);
            return ball;
        }

        private static WeaponData GetWeaponData(string name, int magSize, float range, int damage, int reserve, float fireRate, float reloadTime, float spread, int slot, int pellets = 1)
        {
            string path = "Assets/Prefabs/Weapons/WeaponData_" + name + ".asset";
            WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (data != null)
                return data;

            data = ScriptableObject.CreateInstance<WeaponData>();
            data.displayName = name;
            data.fireRate = fireRate;
            data.range = range;
            data.damage = damage;
            data.reserveAmmo = reserve;
            data.reloadTime = reloadTime;
            data.magSize = magSize;
            data.spread = spread;
            data.slot = slot;
            data.pellets = pellets;
            EnsureFolder("Assets/Prefabs/Weapons");
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            return data;
        }

        private static void BuildWeaponView(Transform parent, FPS.Weapons.WeaponController wc, string name, WeaponData data, Vector3 offset)
        {
            GameObject view = new GameObject(name);
            view.tag = "Weapon";

            Color gunColor = new Color(0.15f, 0.15f, 0.18f);
            switch (data.slot)
            {
                case 1: gunColor = new Color(0.2f, 0.25f, 0.35f); break;
                case 2: gunColor = new Color(0.25f, 0.2f, 0.15f); break;
                case 3: gunColor = new Color(0.12f, 0.15f, 0.12f); break;
            }

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(view.transform, false);
            body.transform.localScale = new Vector3(0.1f, 0.14f, 0.5f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", gunColor);
            var bc = body.GetComponent<Collider>();
            if (bc != null) { bc.enabled = false; }

            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(view.transform, false);
            barrel.transform.localPosition = new Vector3(0f, 0.03f, 0.28f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localScale = new Vector3(0.35f, 0.09f, 0.35f);
            barrel.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Met", new Color(0.3f, 0.3f, 0.34f));
            var baColl = barrel.GetComponent<Collider>();
            if (baColl != null) { baColl.enabled = false; }

            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(view.transform, false);
            grip.transform.localPosition = new Vector3(0f, -0.16f, -0.12f);
            grip.transform.localScale = new Vector3(0.08f, 0.14f, 0.12f);
            grip.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Grip", new Color(0.2f, 0.16f, 0.12f));
            var gColl = grip.GetComponent<Collider>();
            if (gColl != null) { gColl.enabled = false; }

            view.transform.SetParent(parent, false);
            view.transform.localPosition = offset;

            var audioGo = new GameObject(name + "_Audio");
            audioGo.transform.SetParent(view.transform, false);
            var audioSource = audioGo.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.55f;

            string gunshotPath = "Assets/Audio/SFX/Gunshot_Pistol.wav";
            switch (data.slot)
            {
                case 2: gunshotPath = "Assets/Audio/SFX/Gunshot_Rifle.wav"; break;
                case 3: gunshotPath = "Assets/Audio/SFX/Gunshot_Shotgun.wav"; break;
            }
            var gunClip = AssetDatabase.LoadAssetAtPath<AudioClip>(gunshotPath);
            if (gunClip != null)
                audioSource.clip = gunClip;

            if (wc != null)
            {
                var so = new SerializedObject(wc);
                var listProp = so.FindProperty("weapons");
                int idx = listProp.arraySize;
                listProp.arraySize++;
                var entry = listProp.GetArrayElementAtIndex(idx);
                entry.FindPropertyRelative("id").stringValue = name;
                entry.FindPropertyRelative("data").objectReferenceValue = data;
                entry.FindPropertyRelative("view").objectReferenceValue = view;
                entry.FindPropertyRelative("audioSource").objectReferenceValue = audioSource;
                so.ApplyModifiedProperties();
            }

            view.SetActive(data.slot == 1);
        }

        private static void BuildWeaponPrefabs()
        {
            if (FileExists("Assets/Prefabs/Weapons/Weapon_Pistol.prefab"))
                return;

            BuildStandaloneWeapon("Weapon_Pistol", new Color(0.2f, 0.25f, 0.35f), new Color(0.35f, 0.4f, 0.5f));
            BuildStandaloneWeapon("Weapon_Rifle", new Color(0.25f, 0.2f, 0.15f), new Color(0.4f, 0.35f, 0.25f));
            BuildStandaloneWeapon("Weapon_Shotgun", new Color(0.12f, 0.15f, 0.12f), new Color(0.3f, 0.35f, 0.3f));
        }

        private static void BuildStandaloneWeapon(string name, Color bodyColor, Color accent)
        {
            var root = new GameObject(name);
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.12f, 0.16f, 0.6f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", bodyColor);
            var bc = body.GetComponent<Collider>();
            if (bc != null) { bc.enabled = false; }

            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(root.transform, false);
            barrel.transform.localPosition = new Vector3(0f, 0.03f, 0.32f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
            barrel.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Met", accent);
            var baColl = barrel.GetComponent<Collider>();
            if (baColl != null) { baColl.enabled = false; }

            var grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(root.transform, false);
            grip.transform.localPosition = new Vector3(0f, -0.18f, -0.15f);
            grip.transform.localScale = new Vector3(0.1f, 0.16f, 0.14f);
            grip.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Grip", new Color(0.2f, 0.16f, 0.12f));
            var gColl = grip.GetComponent<Collider>();
            if (gColl != null) { gColl.enabled = false; }

            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Weapons/" + name + ".prefab");
            Object.DestroyImmediate(root);
        }

        private static void BuildEnemyPrefabs()
        {
            BuildEnemyPrefab("Enemy_01", new Color(0.75f, 0.1f, 0.1f), 1.3f, 80, 12, 9f, 50f, 2.2f, 4.5f);
            BuildEnemyPrefab("Enemy_02", new Color(0.6f, 0.35f, 0.1f), 1.3f, 100, 15, 12f, 55f, 3f, 4.8f);
            BuildEnemyPrefab("Enemy_03", new Color(0.2f, 0.2f, 0.55f), 1.45f, 140, 20, 10f, 45f, 2.6f, 4.2f);
            BuildEnemyPrefab("Enemy_Boss", new Color(0.5f, 0.05f, 0.05f), 2.3f, 260, 30, 16f, 40f, 2.8f, 4f);
        }

        private static void BuildEnemyPrefab(string prefabName, Color color, float scale, int maxHp, int damage, float detection, float fov, float attackRange, float chase)
        {
            string path = "Assets/Prefabs/Enemies/" + prefabName + ".prefab";
            if (FileExists(path))
                return;

            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = prefabName;
            root.tag = "Enemy";
            root.transform.localScale = new Vector3(scale, scale, scale);
            root.GetComponent<Renderer>().sharedMaterial = CreateMaterial(prefabName + "_Mat", color);

            var col = root.GetComponent<Collider>();
            if (col == null) root.AddComponent<CapsuleCollider>();

            var agent = root.AddComponent<NavMeshAgent>();
            agent.height = 1.8f * scale;
            agent.radius = 0.5f;
            agent.speed = 2.5f;
            agent.acceleration = 8f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 0f;
            agent.avoidancePriority = 50;

            var health = root.AddComponent<FPS.Health.HealthSystem>();
            var hSo = new SerializedObject(health);
            hSo.FindProperty("maxHealth").intValue = maxHp;
            hSo.FindProperty("currentHealth").intValue = maxHp;
            hSo.ApplyModifiedProperties();

            var ai = root.AddComponent<FPS.Enemies.EnemyAI>();
            var aSo = new SerializedObject(ai);
            aSo.FindProperty("attackDamage").intValue = damage;
            aSo.FindProperty("detectionRange").floatValue = detection;
            aSo.FindProperty("fovAngle").floatValue = fov;
            aSo.FindProperty("attackRange").floatValue = attackRange;
            aSo.FindProperty("chaseSpeed").floatValue = chase;
            aSo.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildEnvironmentPrefabs()
        {
            BuildTreePrefab("Tree_01", new Color(0.45f, 0.3f, 0.15f), new Color(0.2f, 0.55f, 0.25f), 1f);
            BuildTreePrefab("Tree_02", new Color(0.5f, 0.35f, 0.18f), new Color(0.15f, 0.45f, 0.2f), 0.8f);
            BuildBushPrefab("Bush_01", new Color(0.25f, 0.5f, 0.2f));
            BuildCarPrefab("Car_01", new Color(0.75f, 0.15f, 0.15f), new Color(0.6f, 0.6f, 0.7f), new Vector3(1.2f, 0.8f, 0f));
            BuildCarPrefab("Car_02", new Color(0.2f, 0.3f, 0.5f), new Color(0.5f, 0.55f, 0.6f), new Vector3(1.1f, 1.4f, 0f));
            BuildCaptiveCapsulePrefab("NPC_01", new Color(0.4f, 0.5f, 0.7f), "Assets/Prefabs/NPC/NPC_01.prefab");

            BuildPickupPrefab("Pickup_Ammo", new Color(0.2f, 0.4f, 0.9f), PickupType.Ammo, 20);
            BuildPickupPrefab("Pickup_Health", new Color(0.1f, 0.8f, 0.2f), PickupType.HealthKit, 30);
            BuildPickupPrefab("Pickup_Objective", new Color(1f, 0.85f, 0.1f), PickupType.ObjectiveItem, 1);

            BuildCheckpointPrefab();
            BuildExitZonePrefab();
        }

        private static void BuildTreePrefab(string name, Color trunkColor, Color crownColor, float scale)
        {
            string path = "Assets/Prefabs/Environment/" + name + ".prefab";
            if (FileExists(path))
                return;

            var root = new GameObject(name);

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localScale = new Vector3(scale, scale * 1.8f, scale);
            trunk.transform.localPosition = new Vector3(0f, scale * 1.8f, 0f);
            trunk.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Trunk", trunkColor);

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.SetParent(root.transform, false);
            crown.transform.localScale = new Vector3(scale * 3f, scale * 3f, scale * 3f);
            crown.transform.localPosition = new Vector3(0f, scale * 4.6f, 0f);
            crown.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Crown", crownColor);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildBushPrefab(string name, Color color)
        {
            string path = "Assets/Prefabs/Environment/" + name + ".prefab";
            if (FileExists(path))
                return;

            var root = new GameObject(name);
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.transform.SetParent(root.transform, false);
            bush.transform.localScale = new Vector3(1.3f, 0.8f, 1.3f);
            bush.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            bush.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", color);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildCarPrefab(string name, Color paint, Color glass, Vector3 roofOffsetScale)
        {
            string path = "Assets/Prefabs/Vehicles/" + name + ".prefab";
            if (FileExists(path))
                return;

            var root = new GameObject(name);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(2f, 0.7f, 4.5f);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Body", paint);
            body.tag = "Ground";

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(root.transform, false);
            roof.transform.localScale = new Vector3(1.8f, roofOffsetScale.y, 2.6f);
            roof.transform.localPosition = new Vector3(0f, 0.7f + roofOffsetScale.y / 2f + 0.35f, -roofOffsetScale.z * 0.4f);
            roof.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Roof", glass);
            roof.tag = "Ground";

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildCaptiveCapsulePrefab(string name, Color color, string path)
        {
            if (FileExists(path))
                return;

            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = name;
            root.transform.localScale = new Vector3(1.1f, 1.4f, 1.1f);
            root.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", color);
            root.tag = "NPC";

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildPickupPrefab(string name, Color color, PickupType type, int amount)
        {
            string path = "Assets/Prefabs/Props/" + name + ".prefab";
            if (FileExists(path))
                return;

            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = name;
            root.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            root.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", color);

            var bc = root.GetComponent<Collider>();
            if (bc != null)
            {
                bc.isTrigger = true;
                bc.enabled = true;
            }

            var pickup = root.AddComponent<ItemPickup>();
            var so = new SerializedObject(pickup);
            so.FindProperty("type").enumValueIndex = (int)type;
            so.FindProperty("amount").intValue = amount;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildCheckpointPrefab()
        {
            string path = "Assets/Prefabs/Environment/Checkpoint.prefab";
            if (FileExists(path))
                return;

            var root = new GameObject("Checkpoint");
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6f, 3f, 6f);
            root.AddComponent<FPS.Checkpoints.Checkpoint>();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void BuildExitZonePrefab()
        {
            string path = "Assets/Prefabs/Props/ExitZone.prefab";
            if (FileExists(path))
                return;

            var root = new GameObject("ExitZone");
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(14f, 4f, 14f);
            root.AddComponent<ExitZone>();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // ------------------------------------------------------------------ //
        //  LEVEL SCENE CONSTRUCTION
        // ------------------------------------------------------------------ //

        private static bool SceneHas<T>() where T : Component
        {
            return Object.FindFirstObjectByType<T>() != null;
        }

        private static bool BuildLevelScene(LevelSpec spec)
        {
            string path = "Assets/Scenes/" + spec.Name + ".unity";
            if (System.IO.File.Exists(path))
            {
                var s = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                bool has = Object.FindFirstObjectByType<FPS.Levels.LevelManager>() != null;
                EditorSceneManager.CloseScene(s, true);
                if (has)
                    return false;
            }

            Debug.Log("[FPS] Building " + spec.Name + " ...");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetLighting(spec);
            CreateGround(spec);

            Vector3 spawn = new Vector3(0f, 0.9f, spec.HalfD - 8f);
            Vector3 exit = new Vector3(0f, 0.5f, -spec.HalfD + 8f);

            BuildBuildings(spec);
            BuildCover(spec);
            BuildVegetation(spec, spawn, exit);
            BuildStreet(spec, exit);
            BuildCars(spec);
            BuildNpcs(spec);
            BuildPolyHavenProps(spec, exit);
            PlacePickups(spec, spawn, exit);
            PlaceCheckpoints(spec, spawn, exit);

            GameObject player = BuildPlayer(spawn);
            BuildHud(player);
            BuildEnemies(spec, spawn, exit);
            BuildExitZone(exit);
            BuildLevelAudio();
            BuildGameManager(player, spawn, path);

            GameObject lmGo = new GameObject("LevelManager");
            var lm = lmGo.AddComponent<FPS.Levels.LevelManager>();
            var lmSo = new SerializedObject(lm);
            lmSo.FindProperty("requiredItems").intValue = spec.RequiredItems;
            lmSo.ApplyModifiedProperties();

            EnsureEventSystem();

            BakeNavMesh();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            AddToBuildList(path);
            return true;
        }

        private static void BuildBuildings(LevelSpec spec)
        {
            float inset = 6f;
            int i = 0;
            for (float x = -spec.HalfW + 12f; x <= spec.HalfW - 12f; x += 18f)
            {
                Building(spec.Name + "_B" + i, new Vector3(x, 0f, -spec.HalfD + inset + 5f), new Vector3(8f, HeightFor(i), 8f), spec);
                Building(spec.Name + "_B" + (i + 40), new Vector3(x, 0f, spec.HalfD - inset - 5f), new Vector3(8f, HeightFor(i + 2), 8f), spec);
                i++;
            }
            for (float z = -spec.HalfD + 12f; z <= spec.HalfD - 12f; z += 18f)
            {
                Building(spec.Name + "_B" + i, new Vector3(-spec.HalfW + inset + 5f, 0f, z), new Vector3(8f, HeightFor(i + 1), 8f), spec);
                Building(spec.Name + "_B" + (i + 80), new Vector3(spec.HalfW - inset - 5f, 0f, z), new Vector3(8f, HeightFor(i + 3), 8f), spec);
                i++;
            }
        }

        private static float HeightFor(int seed)
        {
            int v = (seed * 7) % 6;
            return 3.5f + v * 0.6f;
        }

        private static GameObject Building(string name, Vector3 groundCenter, Vector3 size, LevelSpec spec)
        {
            GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
            house.name = name;
            house.tag = "Ground";
            house.transform.localScale = size;
            house.transform.position = groundCenter + Vector3.up * (size.y / 2f);
            house.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", (name.GetHashCode() % 3 == 0) ? spec.BuildingAlt : spec.BuildingColor);
            house.isStatic = true;
            MarkNavStatic(house);
            return house;
        }

        private static void BuildCover(LevelSpec spec)
        {
            for (int i = 0; i < spec.Walls; i++)
            {
                float x = -20f + i * 18f;
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = spec.Name + "_Wall_" + i;
                wall.tag = "Ground";
                wall.transform.localScale = new Vector3(14f, 2f, 0.8f);
                wall.transform.position = new Vector3(x, 1f, 0f);
                wall.GetComponent<Renderer>().sharedMaterial = CreateMaterial(spec.Name + "_WallMat", new Color(0.55f, 0.5f, 0.45f));
                wall.isStatic = true;
                MarkNavStatic(wall);
            }

            for (int i = 0; i < spec.Crates; i++)
            {
                float angle = i * 37.5f * Mathf.Deg2Rad;
                float r = 12f + (i % 3) * 7f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * r, 0.5f, Mathf.Sin(angle) * r + 8f);
                GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = spec.Name + "_Crate_" + i;
                crate.tag = "Ground";
                crate.transform.localScale = new Vector3(1.4f, 1.1f, 1.4f);
                crate.transform.position = pos;
                crate.GetComponent<Renderer>().sharedMaterial = CreateMaterial(spec.Name + "_CrateMat", new Color(0.6f, 0.4f, 0.25f));
                crate.isStatic = true;
                MarkNavStatic(crate);
            }
        }

        private static void BuildVegetation(LevelSpec spec, Vector3 spawn, Vector3 exit)
        {
            const string treePrefab = "Assets/Prefabs/Environment/";
            for (int i = 0; i < spec.Trees; i++)
            {
                float s = Mathf.Sin(i * 47f);
                float c = Mathf.Cos(i * 73f);
                Vector3 pos = new Vector3((s * (spec.HalfW - 14f)), 0f, (c * (spec.HalfD - 14f)) + s * 12f);
                if (Vector3.Distance(pos, spawn) < 6f || Vector3.Distance(pos, exit) < 8f)
                    pos += new Vector3(4f, 0f, -4f);
                string treePath = (i % 2 == 0) ? treePrefab + "Tree_01.prefab" : treePrefab + "Tree_02.prefab";
                PlacePrefab(treePath, spec.Name + "_Tree_" + i, pos, 1f);
            }
        }

        private static void BuildStreet(LevelSpec spec, Vector3 exit)
        {
            GameObject street = GameObject.CreatePrimitive(PrimitiveType.Cube);
            street.name = spec.Name + "_Street";
            street.transform.localScale = new Vector3(11f, 0.06f, spec.HalfD * 2f);
            street.transform.position = new Vector3(0f, 0f, 0f);
            street.GetComponent<Renderer>().sharedMaterial = CreateMaterial(spec.Name + "_StreetMat", new Color(0.13f, 0.14f, 0.16f));
            street.isStatic = true;
            MarkNavStatic(street);
        }

        private static void BuildCars(LevelSpec spec)
        {
            for (int i = 0; i < spec.Cars; i++)
            {
                float side = (i % 2 == 0) ? 4f : -5f;
                float z = -spec.HalfD * 0.5f + i * (spec.HalfD / 3f);
                string carPath = (i % 2 == 0) ? "Assets/Prefabs/Vehicles/Car_01.prefab" : "Assets/Prefabs/Vehicles/Car_02.prefab";
                PlacePrefab(carPath, spec.Name + "_Car_" + i, new Vector3(side, 0f, z), 1f);
            }
        }

        private static void BuildNpcs(LevelSpec spec)
        {
            for (int i = 0; i < spec.Npcs; i++)
            {
                float side = (i % 2 == 0) ? -8f : 9f;
                float z = 8f + i * 18f;
                PlacePrefab("Assets/Prefabs/NPC/NPC_01.prefab", spec.Name + "_NPC_" + i, new Vector3(side, 0f, z), 1f);
            }
        }

        private static void BuildPolyHavenProps(LevelSpec spec, Vector3 exit)
        {
            PlacePropModel("street_lamp_01", new Vector3(-6f, 0f, -10f), 4.5f, 0f, spec);
            PlacePropModel("street_lamp_01", new Vector3(6f, 0f, 12f), 4.5f, 0f, spec);
            PlacePropModel("painted_wooden_bench", new Vector3(4f, 0f, 25f), 1.4f, 90f, spec);
            PlacePropModel("concrete_road_barrier", new Vector3(8f, 0f, -15f), 1.6f, 0f, spec);
            PlacePropModel("modular_electricity_poles", new Vector3(-30f, 0f, 20f), 8f, 40f, spec);
            if (spec.Name == "Level06" || spec.Name == "Level08")
                PlacePropModel("modular_urban_apartments_facade", new Vector3(-40f, 0f, -20f), 12f, 90f, spec);
            PlacePropModel("street_rat", new Vector3(0f, 0f, 30f), 0.5f, 0f, spec);
            if (Vector3.Distance(exit, Vector3.zero) > 30f)
                PlacePropModel("street_lamp_01", new Vector3(exit.x, 0f, exit.z + 4f), 4.5f, 0f, spec);
        }

        private static void PlacePropModel(string id, Vector3 position, float targetSize, float yaw, LevelSpec spec)
        {
            string path = "Assets/Models/Environment/" + id + "/" + id + ".gltf";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
            {
                Debug.LogWarning("[FPS] Prop model not importable yet: " + path);
                return;
            }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(model, SceneManager.GetActiveScene());
            inst.name = spec.Name + "_Prop_" + id;
            inst.transform.position = position;

            var renderers = inst.GetComponentsInChildren<Renderer>();
            Bounds total = new Bounds(position, Vector3.zero);
            bool hasBounds = false;
            foreach (var r in renderers)
            {
                Bounds b = r.bounds;
                if (!hasBounds) { total = b; hasBounds = true; }
                else total.Encapsulate(b);
            }
            if (hasBounds && total.size.sqrMagnitude > 0.0001f)
            {
                float scale = targetSize / Mathf.Max(total.size.x, total.size.z, total.size.y);
                inst.transform.localScale = inst.transform.localScale * scale;
            }

            float minY = float.MaxValue;
            foreach (var r in renderers)
                minY = Mathf.Min(minY, r.bounds.min.y);
            foreach (var c in inst.GetComponentsInChildren<Collider>())
                minY = Mathf.Min(minY, c.bounds.min.y);
            if (minY != float.MaxValue)
                inst.transform.position = new Vector3(position.x, position.y - minY, position.z);

            inst.transform.Rotate(0f, yaw, 0f, Space.World);

            if (inst.GetComponentInChildren<Collider>() == null)
            {
                var bc = inst.AddComponent<BoxCollider>();
                bc.size = Vector3.one * targetSize;
                bc.center = Vector3.up * (targetSize * 0.5f);
            }
            foreach (var c in inst.GetComponentsInChildren<Collider>())
                c.tag = "Ground";
        }

        private static void PlacePickups(LevelSpec spec, Vector3 spawn, Vector3 exit)
        {
            Vector3 mid = Vector3.Lerp(spawn, exit, 0.5f);
            PlacePrefab("Assets/Prefabs/Props/Pickup_Ammo.prefab", spec.Name + "_Ammo_1", new Vector3(mid.x + 3f, 0.6f, mid.z), 1f);
            PlacePrefab("Assets/Prefabs/Props/Pickup_Health.prefab", spec.Name + "_Health_1", new Vector3(spawn.x - 4f, 0.6f, spawn.z + 2f), 1f);
            if (spec.RequiredItems > 0)
            {
                for (int i = 0; i < spec.RequiredItems; i++)
                {
                    float t = 0.6f + i * 0.13f;
                    Vector3 pos = Vector3.Lerp(spawn, exit, Mathf.Min(t, 0.95f));
                    PlacePrefab("Assets/Prefabs/Props/Pickup_Objective.prefab",
                        spec.Name + "_Objective_" + (i + 1), new Vector3(pos.x - 3f, 0.6f, pos.z + 2f), 1f);
                }
            }
            else
            {
                PlacePrefab("Assets/Prefabs/Props/Pickup_Ammo.prefab", spec.Name + "_Ammo_2", new Vector3(exit.x + 4f, 0.6f, exit.z), 1f);
            }
        }

        private static void PlaceCheckpoints(LevelSpec spec, Vector3 spawn, Vector3 exit)
        {
            for (int i = 0; i < spec.Checkpoints; i++)
            {
                float t = 0.35f + i * 0.2f;
                Vector3 pos = Vector3.Lerp(spawn, exit, Mathf.Min(t, 0.9f));
                PlacePrefab("Assets/Prefabs/Environment/Checkpoint.prefab", spec.Name + "_Checkpoint_" + (i + 1), pos, 1f);
            }
        }

        private static void BuildEnemies(LevelSpec spec, Vector3 spawn, Vector3 exit)
        {
            for (int i = 0; i < spec.Enemies; i++)
            {
                float t = (float)(i + 1) / (float)(spec.Enemies + 1);
                float lateral = Mathf.Sin(i * 40f) * 6f;
                Vector3 pos = Vector3.Lerp(spawn, exit, t);
                pos += new Vector3(lateral, 0.5f, 0f);

                string prefab = "Enemy_01";
                if (spec.Boss && i == spec.Enemies - 1)
                    prefab = "Enemy_Boss";
                else if (i % 5 == 4)
                    prefab = "Enemy_03";
                else if (i % 3 == 1)
                    prefab = "Enemy_02";

                var inst = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + prefab + ".prefab"),
                    SceneManager.GetActiveScene());
                if (inst == null)
                    continue;
                inst.name = spec.Name + "_" + prefab + "_" + (i + 1);
                inst.transform.position = pos;

                var ai = inst.GetComponent<FPS.Enemies.EnemyAI>();
                if (ai != null)
                {
                    Vector3[] patrol = new Vector3[]
                    {
                        pos,
                        pos + new Vector3(6f, 0f, -4f) * ((i % 2 == 0) ? 1f : -1f)
                    };
                    Transform[] pts = new Transform[patrol.Length];
                    for (int p = 0; p < patrol.Length; p++)
                    {
                        GameObject point = new GameObject(inst.name + "_Patrol_" + p);
                        point.tag = "Checkpoint";
                        point.transform.position = patrol[p];
                        pts[p] = point.transform;
                    }
                    ai.SetPatrolPoints(pts);
                }
            }
        }

        private static void BuildExitZone(Vector3 exit)
        {
            PlacePrefab("Assets/Prefabs/Props/ExitZone.prefab", "ExitZone", exit, 1f);

            GameObject marker = new GameObject("PlayerSpawn");
            marker.tag = "Checkpoint";
            marker.transform.position = new Vector3(0f, 0.25f, exit.z > 0 ? -10f : 10f);
        }

        private static void BuildLevelAudio()
        {
            var audioGo = new GameObject("GameAudio");
            var music = audioGo.AddComponent<FPS.Audio.MusicPlayer>();
            var so = new SerializedObject(music);
            var ambientClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Ambient_Loop.wav");
            var combatClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Combat_Loop.wav");
            if (ambientClip != null)
                so.FindProperty("ambientClip").objectReferenceValue = ambientClip;
            if (combatClip != null)
                so.FindProperty("combatClip").objectReferenceValue = combatClip;
            so.ApplyModifiedProperties();
        }

        private static GameObject BuildPlayer(Vector3 spawnPos)
        {
            var player = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath),
                SceneManager.GetActiveScene());
            player.name = "Player";
            player.transform.position = spawnPos;
            return player;
        }

        private static void BuildHud(GameObject player)
        {
            var canvasGo = new GameObject("HUDCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var hudGo = new GameObject("FpsHud");
            hudGo.transform.SetParent(canvasGo.transform, false);
            var hud = hudGo.AddComponent<FPS.UI.FpsHud>();
            var hudSo = new SerializedObject(hud);

            var healthFill = CreateImage("Health_Fill", canvasGo.transform, new Vector2(40, 40), new Vector2(410, 22), new Color(0.1f, 0.8f, 0.2f));
            var fillImg = healthFill.GetComponent<UnityEngine.UI.Image>();
            fillImg.type = UnityEngine.UI.Image.Type.Filled;
            fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;

            var healthText = CreateText("Health_Text", canvasGo.transform, new Vector2(40, 100), new Vector2(140, 30), "100", 24, TextAnchor.MiddleLeft);
            var ammoText = CreateText("Ammo_Text", canvasGo.transform, new Vector2(1780, 40), new Vector2(120, 40), "30", 44, TextAnchor.MiddleRight);
            var reserveText = CreateText("Reserve_Text", canvasGo.transform, new Vector2(1780, 90), new Vector2(120, 24), "120", 22, TextAnchor.MiddleRight);
            var weaponNameText = CreateText("Weapon_Text", canvasGo.transform, new Vector2(1740, 130), new Vector2(160, 28), "Rifle", 20, TextAnchor.MiddleRight);
            var messageText = CreateText("Message_Text", canvasGo.transform, new Vector2(960, 350), new Vector2(700, 60), "", 30, TextAnchor.MiddleCenter);
            messageText.gameObject.SetActive(false);

            hudSo.FindProperty("healthFill").objectReferenceValue = fillImg;
            hudSo.FindProperty("healthText").objectReferenceValue = healthText;
            hudSo.FindProperty("ammoText").objectReferenceValue = ammoText;
            hudSo.FindProperty("reserveText").objectReferenceValue = reserveText;
            hudSo.FindProperty("weaponNameText").objectReferenceValue = weaponNameText;
            hudSo.FindProperty("messageText").objectReferenceValue = messageText;

            var wc = player.GetComponent<FPS.Weapons.WeaponController>();
            if (wc != null)
                hudSo.FindProperty("weaponController").objectReferenceValue = wc;
            var hs = player.GetComponent<FPS.Health.HealthSystem>();
            if (hs != null)
                hudSo.FindProperty("playerHealth").objectReferenceValue = hs;
            hudSo.ApplyModifiedProperties();
        }

        private static GameObject CreateImage(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            return go;
        }

        private static Text CreateText(string name, Transform parent, Vector2 pos, Vector2 size, string text, int fontSize, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.alignment = anchor;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private static void BuildGameManager(GameObject player, Vector3 spawn, string scenePath)
        {
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<FPS.Managers.GameManager>();
            var so = new SerializedObject(gm);
            so.FindProperty("playerTransform").objectReferenceValue = player.transform;

            var initialSpawn = new GameObject("InitialSpawn");
            initialSpawn.transform.position = spawn;
            so.FindProperty("initialSpawnPoint").objectReferenceValue = initialSpawn.transform;

            var hudCanvas = GameObject.Find("HUDCanvas");
            if (hudCanvas != null)
                so.FindProperty("hudObject").objectReferenceValue = hudCanvas;

            so.ApplyModifiedProperties();

            var pauseGo = new GameObject("PauseMenu", typeof(FPS.UI.PauseMenu));
            var gameOverGo = new GameObject("GameOverMenu", typeof(FPS.UI.GameOverMenu));
            var mcGo = new GameObject("MissionCompleteMenu", typeof(FPS.UI.MissionCompleteMenu));

            var gmSo2 = new SerializedObject(gm);
            gmSo2.FindProperty("pauseMenu").objectReferenceValue = pauseGo;
            gmSo2.FindProperty("gameOverMenu").objectReferenceValue = gameOverGo;
            gmSo2.FindProperty("missionCompleteMenu").objectReferenceValue = mcGo;
            gmSo2.ApplyModifiedProperties();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static GameObject PlacePrefab(string prefabPath, string name, Vector3 pos, float scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return null;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            if (inst == null)
                return null;
            inst.name = name;
            inst.transform.position = pos;
            if (scale != 1f)
                inst.transform.localScale = Vector3.one * scale;
            return inst;
        }

        // ------------------------------------------------------------------ //
        //  LEVEL 01 INTEGRATION (existing FpsSceneBuilder scene)
        // ------------------------------------------------------------------ //

        private static bool IntegrateLevel01()
        {
            string path = "Assets/Scenes/Level01.unity";
            if (!System.IO.File.Exists(path))
                return false;

            var s = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            bool has = Object.FindFirstObjectByType<FPS.Levels.LevelManager>() != null;
            if (has)
            {
                EditorSceneManager.CloseScene(s, true);
                return false;
            }

            Debug.Log("[FPS] Integrating progression into Level01 ...");

            // Disable the legacy auto-mission-complete final zone so winning requires
            // killing all enemies first (then reaching the exit).
            var finalZone = GameObject.Find("MissionFinalZone");
            if (finalZone != null)
            {
                var cp = finalZone.GetComponent<FPS.Checkpoints.Checkpoint>();
                if (cp != null)
                {
                    var cpSo = new SerializedObject(cp);
                    cpSo.FindProperty("isFinal").boolValue = false;
                    cpSo.ApplyModifiedProperties();
                }
            }

            Vector3 exit = new Vector3(0f, 0.25f, -52f);
            PlacePrefab("Assets/Prefabs/Props/ExitZone.prefab", "ExitZone", exit, 1f);

            var lmGo = new GameObject("LevelManager");
            var lm = lmGo.AddComponent<FPS.Levels.LevelManager>();
            var lmSo = new SerializedObject(lm);
            lmSo.FindProperty("requiredItems").intValue = 0;
            lmSo.ApplyModifiedProperties();

            EnsureEventSystem();

            EditorSceneManager.SaveScene(s, path);
            EditorSceneManager.CloseScene(s, true);
            AddToBuildList(path);
            return true;
        }

        // ------------------------------------------------------------------ //
        //  MAIN MENU
        // ------------------------------------------------------------------ //

        private static bool BuildMainMenu()
        {
            string path = "Assets/Scenes/MainMenu.unity";
            if (System.IO.File.Exists(path))
            {
                var s = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                bool has = Object.FindFirstObjectByType<FPS.UI.MainMenuController>() != null;
                EditorSceneManager.CloseScene(s, true);
                if (has)
                    return false;
            }

            Debug.Log("[FPS] Building MainMenu ...");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var rootGo = new GameObject("MainMenu");
            rootGo.AddComponent<FPS.UI.MainMenuController>();

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            AddToBuildList(path);
            return true;
        }

        // ------------------------------------------------------------------ //
        //  BUILD SETTINGS
        // ------------------------------------------------------------------ //

        private static readonly List<string> BuildScenes = new List<string>();

        private static void AddToBuildList(string path)
        {
            if (!BuildScenes.Contains(path))
                BuildScenes.Add(path);
        }

        private static void PatchBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true));
            for (int i = 1; i <= 9; i++)
                scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level" + i.ToString("D2") + ".unity", true));
            if (System.IO.File.Exists("Assets/Scenes/Test/TestScene.unity"))
            {
                scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Test/TestScene.unity", false));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif