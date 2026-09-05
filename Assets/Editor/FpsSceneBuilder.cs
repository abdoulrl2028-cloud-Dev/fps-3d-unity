#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UI;
using FPS.Weapons;
using System.Collections.Generic;

namespace FPS.EditorTools
{
    public static class FpsSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level01.unity";
        private static GameObject _playerRoot;
        private static GameObject _weaponsRig;

        static FpsSceneBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfMissing;
        }

        [DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            EditorApplication.delayCall += AutoBuildIfMissing;
        }

        /// <summary>
        /// Automatically builds the FPS scene once the first time the project loads.
        /// </summary>
        private static void AutoBuildIfMissing()
        {
            if (System.IO.File.Exists(ScenePath))
                return;

            try
            {
                BuildFpsScene();
            }
            catch (System.Exception e)
            {
                Debug.LogError("[FPS] Automatic scene build failed: " + e.Message);
            }
        }

        [MenuItem("FPS/Build FPS Scene")]
        public static void BuildFpsScene()
        {
            // Generate audio assets first so clips exist
            FpsAudioGenerator.GenerateAll();

            // Only run if not currently in hot-swap of play mode
            if (!BuildPipeline.isBuildingPlayer && Application.isPlaying)
                return;

            NewScene();
            BuildLights();

            // Ground/street area
            Vector3 spawnPos = BuildEnvironment();

            // Player
            BuildPlayer(spawnPos);

            // Checkpoints
            List<GameObject> checkpoints = BuildCheckpoints();

            // Enemies
            BuildEnemies(checkpoints);

            // Final mission zone
            BuildFinalZone();

            // HUD UI
            BuildHud();

            // Audio
            BuildAudio();

            // Managers + Menus
            BuildGameManager();

            // NavMesh
            BakeNavMesh();

            // Save
            SaveScene();

            Selection.activeObject = _playerRoot;
            Debug.Log("[FPS] Scene built successfully! Press Play to test.");
        }

        [MenuItem("FPS/Build FPS Scene + Save", true)]
        private static bool ValidateBuild() => true;

        private static void NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void SaveScene()
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void BuildLights()
        {
            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.95f, 0.85f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);

            var skybox = new GameObject("Skybox");
            var cam = skybox.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.cullingMask = 0;
            cam.depth = -10f;
            cam.enabled = false;
        }

        private static Vector3 BuildEnvironment()
        {
            // Ground plane (street sidewalk area)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.tag = "Ground";
            ground.transform.localScale = new Vector3(120f, 1f, 120f);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("GroundMat", new Color(0.35f, 0.4f, 0.45f));
            ground.GetComponent<Collider>().material = CreatePhysMat(0.9f, 0.1f);
            ground.isStatic = true;
            MarkNavStatic(ground);

            // Street (asphalt strip)
            GameObject street = GameObject.CreatePrimitive(PrimitiveType.Cube);
            street.name = "Street";
            street.transform.localScale = new Vector3(18f, 0.06f, 120f);
            street.transform.position = new Vector3(16f, 0f, 0f);
            street.GetComponent<Renderer>().sharedMaterial = CreateMaterial("StreetMat", new Color(0.15f, 0.16f, 0.18f));
            street.isStatic = true;
            MarkNavStatic(street);

            // Houses (buildings) along sides
            BuildHouse(new Vector3(-28f, 0.5f, -20f), new Vector3(10f, 4f, 8f), "House_1");
            BuildHouse(new Vector3(-28f, 0.5f, 8f), new Vector3(10f, 5f, 8f), "House_2");
            BuildHouse(new Vector3(-28f, 0.5f, 28f), new Vector3(10f, 3.5f, 8f), "House_3");
            BuildHouse(new Vector3(-28f, 0.5f, -40f), new Vector3(10f, 4.5f, 8f), "House_4");
            BuildHouse(new Vector3(-50f, 0.5f, -20f), new Vector3(10f, 4f, 14f), "House_5");

            // Shop buildings on other side
            BuildHouse(new Vector3(46f, 0.5f, -10f), new Vector3(8f, 4.5f, 6f), "Shop_1");
            BuildHouse(new Vector3(46f, 0.5f, 15f), new Vector3(8f, 4f, 6f), "Shop_2");

            // Trees
            BuildTree(new Vector3(-12f, 0f, -30f));
            BuildTree(new Vector3(38f, 0f, 30f));
            BuildTree(new Vector3(-12f, 0f, -50f));
            BuildTree(new Vector3(38f, 0f, -15f));
            BuildTree(new Vector3(-2f, 0f, 45f));

            // Cars
            BuildCar(new Vector3(14f, 0f, -10f));
            BuildCar(new Vector3(16f, 0f, -34f));
            BuildCar(new Vector3(6f, 0f, 25f));

            // Cover walls forming corridors/saloon
            BuildWall(new Vector3(-20f, 1f, 0f), new Vector3(14f, 2f, 0.8f), "Cover_1");
            BuildWall(new Vector3(-20f, 1f, -28f), new Vector3(14f, 2f, 0.8f), "Cover_2");
            BuildWall(new Vector3(-20f, 1f, 25f), new Vector3(14f, 2f, 0.8f), "Cover_3");
            BuildWall(new Vector3(-20f, 1f, 50f), new Vector3(14f, 2f, 0.8f), "Cover_4");

            BuildWall(new Vector3(26f, 1f, 0f), new Vector3(0.8f, 2f, 16f), "Corridor_1");
            BuildWall(new Vector3(26f, 1f, 16f), new Vector3(10f, 2f, 0.8f), "Corridor_2");

            // Blocks/crates as cover
            BuildCrate(new Vector3(-6f, 0.5f, 5f), new Vector3(1.2f, 1f, 1.2f));
            BuildCrate(new Vector3(-6f, 0.5f, 8f), new Vector3(1.2f, 1f, 1.2f));
            BuildCrate(new Vector3(30f, 0.5f, 8f), new Vector3(1.2f, 1f, 1.2f));
            BuildCrate(new Vector3(30f, 0.5f, 22f), new Vector3(1.2f, 1f, 1.2f));

            return new Vector3(0f, 0.75f, 65f);
        }

        private static void BuildHouse(Vector3 pos, Vector3 size, string name)
        {
            GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
            house.name = name;
            house.transform.localScale = size;
            house.transform.position = pos;
            house.tag = "Ground";
            house.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", new Color(0.8f, 0.75f, 0.68f));
            house.isStatic = true;
            MarkNavStatic(house);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = name + "_Roof";
            roof.transform.localScale = new Vector3(size.x + 1f, 0.35f, size.z + 1f);
            roof.transform.position = pos + Vector3.up * (size.y / 2f + 0.2f);
            roof.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_RoofMat", new Color(0.55f, 0.2f, 0.2f));
            roof.isStatic = true;
        }

        private static void BuildTree(Vector3 pos)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Tree_Trunk";
            trunk.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
            trunk.transform.position = pos + Vector3.up * 1.5f;
            trunk.GetComponent<Renderer>().sharedMaterial = CreateMaterial("TreeTrunkMat", new Color(0.45f, 0.3f, 0.15f));
            trunk.GetComponent<Collider>().enabled = false;

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Tree_Crown";
            crown.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            crown.transform.position = pos + Vector3.up * 4f;
            crown.GetComponent<Renderer>().sharedMaterial = CreateMaterial("TreeCrownMat", new Color(0.2f, 0.55f, 0.25f));
            crown.GetComponent<Collider>().enabled = false;

            GameObject trunkCollider = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunkCollider.name = "Tree_Collider";
            trunkCollider.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            trunkCollider.transform.position = pos + Vector3.up * 0.5f;
            trunkCollider.GetComponent<Renderer>().enabled = false;
            trunkCollider.isStatic = true;
            MarkNavStatic(trunkCollider);
        }

        private static void BuildCar(Vector3 pos)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Car_Body";
            body.transform.localScale = new Vector3(2f, 0.7f, 4.5f);
            body.transform.position = pos + Vector3.up * 0.7f;
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarBodyMat", new Color(0.75f, 0.15f, 0.15f));
            body.tag = "Ground";

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Car_Roof";
            roof.transform.localScale = new Vector3(1.6f, 0.5f, 2.4f);
            roof.transform.position = pos + Vector3.up * 1.25f;
            roof.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CarRoofMat", new Color(0.6f, 0.6f, 0.7f));
            roof.tag = "Ground";
        }

        private static void BuildWall(Vector3 pos, Vector3 size, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.localScale = size;
            wall.transform.position = pos;
            wall.tag = "Ground";
            wall.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", new Color(0.6f, 0.55f, 0.5f));
            wall.isStatic = true;
            MarkNavStatic(wall);
        }

        private static void BuildCrate(Vector3 pos, Vector3 size)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Crate";
            crate.transform.localScale = size;
            crate.transform.position = pos;
            crate.tag = "Ground";
            crate.GetComponent<Renderer>().sharedMaterial = CreateMaterial("CrateMat", new Color(0.6f, 0.4f, 0.25f));
            crate.isStatic = true;
            MarkNavStatic(crate);
        }

        private static void BuildPlayer(Vector3 spawnPos)
        {
            _playerRoot = new GameObject("Player");
            _playerRoot.tag = "Player";

            Vector3 finalSpawn = new Vector3(0f, 0.9f, 55f);
            _playerRoot.transform.position = finalSpawn;

            var cc = _playerRoot.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 1f, 0f);

            var controller = _playerRoot.AddComponent<FPS.Player.FpsPlayerController>();
            var health = _playerRoot.AddComponent<FPS.Health.HealthSystem>();
            _playerRoot.AddComponent<FPS.Player.PlayerDeathHandler>();

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(_playerRoot.transform);
            camGo.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();

            // Assign camera to controller
            var so = new SerializedObject(controller);
            so.FindProperty("playerCamera").objectReferenceValue = cam;
            so.ApplyModifiedProperties();

            // WeaponRig attached to camera
            _weaponsRig = new GameObject("WeaponsRig");
            _weaponsRig.transform.SetParent(camGo.transform);
            _weaponsRig.transform.localPosition = new Vector3(0.35f, -0.3f, 0.55f);

            var weaponController = _playerRoot.AddComponent<FPS.Weapons.WeaponController>();
            var weaponSo = new SerializedObject(weaponController);
            weaponSo.FindProperty("aimCamera").objectReferenceValue = cam;

            // Muzzle = camera
            weaponSo.FindProperty("muzzlePoint").objectReferenceValue = camGo.transform;

            // Effects
            var spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "HitSpark";
            spark.transform.localScale = Vector3.one * 0.12f;
            spark.GetComponent<Renderer>().sharedMaterial = CreateMaterial("HitSparkMat", new Color(1f, 0.8f, 0.2f));
            var sparkCollider = spark.GetComponent<Collider>();
            if (sparkCollider) Object.DestroyImmediate(sparkCollider);
            spark.SetActive(false);
            spark.tag = "Weapon";

            var blood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blood.name = "BloodImpact";
            blood.transform.localScale = Vector3.one * 0.15f;
            blood.GetComponent<Renderer>().sharedMaterial = CreateMaterial("BloodImpactMat", new Color(0.8f, 0.05f, 0.05f));
            var bloodCollider = blood.GetComponent<Collider>();
            if (bloodCollider) Object.DestroyImmediate(bloodCollider);
            blood.SetActive(false);
            blood.tag = "Weapon";

            weaponSo.FindProperty("hitSpark").objectReferenceValue = spark;
            weaponSo.FindProperty("bloodImpact").objectReferenceValue = blood;
            weaponSo.ApplyModifiedProperties();

            // Weapons: 3 WeaponData assets
            WeaponData pistol = CreateWeaponData("Pistol", 12, 100f, 20, 60, 0.25f, 1.2f, 0f, 1);
            WeaponData rifle = CreateWeaponData("Rifle", 30, 120f, 8, 120, 0.12f, 2f, 0.2f, 2);
            WeaponData shotgun = CreateWeaponData("Shotgun", 40, 40f, 6, 24, 0.9f, 2.5f, 3f, 3, 6);

            BuildWeaponView(_weaponsRig.transform, weaponController, "PistolView", pistol, new Vector3(0f, 0f, 0f));
            BuildWeaponView(_weaponsRig.transform, weaponController, "RifleView", rifle, new Vector3(0f, 0f, 0f));
            BuildWeaponView(_weaponsRig.transform, weaponController, "ShotgunView", shotgun, new Vector3(0f, 0f, 0f));
        }

        private static void BuildWeaponView(Transform parent, FPS.Weapons.WeaponController wc, string name, WeaponData data, Vector3 offset)
        {
            GameObject view = GameObject.CreatePrimitive(PrimitiveType.Cube);
            view.name = name;
            view.tag = "Weapon";
            var c = view.GetComponent<Collider>();
            if (c) Object.DestroyImmediate(c);

            Color gunColor = new Color(0.15f, 0.15f, 0.18f);
            switch (data.slot)
            {
                case 1: gunColor = new Color(0.2f, 0.25f, 0.35f); break;
                case 2: gunColor = new Color(0.25f, 0.2f, 0.15f); break;
                case 3: gunColor = new Color(0.12f, 0.15f, 0.12f); break;
            }
            view.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", gunColor);

            view.transform.SetParent(parent);
            view.transform.localPosition = offset;
            // Scale to look like a gun
            view.transform.localScale = new Vector3(0.09f, 0.12f, 0.5f);

            var audioGo = new GameObject(name + "_Audio");
            audioGo.transform.SetParent(view.transform);
            audioGo.transform.localPosition = Vector3.zero;
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

            // Build WeaponEntry via SerializedObject
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

            // Set initial ammo via serialized props
            var stateSo = new SerializedObject(wc);
            stateSo.FindProperty("currentAmmo").intValue = data.magSize;
            stateSo.FindProperty("reserveAmmo").intValue = data.reserveAmmo;
            stateSo.ApplyModifiedProperties();

            view.SetActive(data.slot == 1);
        }

        private static WeaponData CreateWeaponData(string name, int magSize, float range, int damage, int reserve, float fireRate, float reloadTime, float spread, int slot, int pellets = 1)
        {
            WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
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

            string path = "Assets/Prefabs/Weapons/WeaponData_" + name + ".asset";
            if (!System.IO.File.Exists(path))
            {
                AssetDatabase.CreateAsset(data, path);
            }
            else
            {
                data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            }
            AssetDatabase.SaveAssets();
            return data;
        }

        private static List<GameObject> BuildCheckpoints()
        {
            var list = new List<GameObject>();
            Vector3[] positions =
            {
                new Vector3(-2f, 0.25f, 25f),
                new Vector3(2f, 0.25f, -15f),
                new Vector3(-8f, 0.25f, 20f),
                new Vector3(0f, 0.25f, -28f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject cp = new GameObject("Checkpoint_" + (i + 1));
                cp.tag = "Checkpoint";
                cp.transform.position = positions[i];
                var col = cp.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(6f, 3f, 6f);
                cp.AddComponent<FPS.Checkpoints.Checkpoint>();
                list.Add(cp);
            }
            return list;
        }

        private static void BuildEnemies(List<GameObject> checkpoints)
        {
            BuildEnemy("Enemy_Rifle", new Vector3(-6f, 0.5f, 30f), new Vector3[] { new Vector3(-6f, 0.5f, 30f), new Vector3(10f, 0.5f, 30f) }, 10, 8f, 60f, 12f, 2.5f);
            BuildEnemy("Enemy_Shotgun", new Vector3(12f, 0.5f, -20f), new Vector3[] { new Vector3(12f, 0.5f, -20f), new Vector3(12f, 0.5f, -35f) }, 15, 12f, 45f, 6f, 2f);
            BuildEnemy("Enemy_Rifle_2", new Vector3(-15f, 0.5f, 12f), new Vector3[] { new Vector3(-15f, 0.5f, 12f), new Vector3(-15f, 0.5f, -10f) }, 10, 8f, 60f, 12f, 2.5f);
            BuildEnemy("Enemy_Pistol", new Vector3(30f, 0.5f, 5f), new Vector3[] { new Vector3(30f, 0.5f, 5f), new Vector3(30f, 0.5f, 20f) }, 8, 6f, 70f, 8f, 3f);
            BuildEnemy("Enemy_Boss", new Vector3(-4f, 0.5f, -35f), new Vector3[] { new Vector3(-4f, 0.5f, -35f), new Vector3(-4f, 0.5f, -55f) }, 25, 16f, 40f, 20f, 1.5f);
            BuildEnemy("Enemy_Rifle_3", new Vector3(5f, 0.5f, 45f), new Vector3[] { new Vector3(5f, 0.5f, 45f), new Vector3(20f, 0.5f, 45f) }, 10, 8f, 60f, 12f, 2.5f);
        }

        private static void BuildEnemy(string name, Vector3 pos, Vector3[] patrol, int damage, float detectionRange, float fov, float attackRange, float maxHealth)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            enemy.tag = "Enemy";
            enemy.transform.position = pos;
            enemy.transform.localScale = new Vector3(1.2f, 1.6f, 1.2f);
            enemy.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", new Color(0.75f, 0.1f, 0.1f));

            var col = enemy.GetComponent<Collider>();
            if (col == null) col = enemy.AddComponent<CapsuleCollider>();

            var agent = enemy.AddComponent<NavMeshAgent>();
            agent.height = 2.4f;
            agent.radius = 0.5f;
            agent.speed = 2.5f;
            agent.acceleration = 8f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 0f;

            var health = enemy.AddComponent<FPS.Health.HealthSystem>();
            var so = new SerializedObject(health);
            so.FindProperty("maxHealth").intValue = (int)maxHealth;
            so.FindProperty("currentHealth").intValue = (int)maxHealth;
            so.ApplyModifiedProperties();

            // Eye transform (child at top)
            Transform[] patrolT = new Transform[patrol.Length];
            for (int i = 0; i < patrol.Length; i++)
            {
                GameObject point = new GameObject(name + "_Patrol_" + i);
                point.tag = "Checkpoint";
                point.transform.position = patrol[i];
                patrolT[i] = point.transform;
            }

            var ai = enemy.AddComponent<FPS.Enemies.EnemyAI>();
            var aiSo = new SerializedObject(ai);
            aiSo.FindProperty("attackDamage").intValue = damage;
            aiSo.FindProperty("detectionRange").floatValue = detectionRange;
            aiSo.FindProperty("fovAngle").floatValue = fov;
            aiSo.FindProperty("attackRange").floatValue = attackRange;
            aiSo.FindProperty("chaseSpeed").floatValue = 4.5f;
            aiSo.ApplyModifiedProperties();

            ai.SetPatrolPoints(patrolT);
        }

        private static void BuildFinalZone()
        {
            GameObject zone = new GameObject("MissionFinalZone");
            zone.tag = "FinalZone";
            zone.transform.position = new Vector3(0f, 0.25f, -52f);
            var col = zone.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(16f, 4f, 16f);
            col.center = new Vector3(0f, 1.5f, 0f);

            var checkpoint = zone.AddComponent<FPS.Checkpoints.Checkpoint>();
            var so = new SerializedObject(checkpoint);
            so.FindProperty("isFinal").boolValue = true;
            so.ApplyModifiedProperties();
        }

        private static void BuildHud()
        {
            var canvasGo = new GameObject("HUDCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasGo.transform.SetParent(_playerRoot.transform);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var hudGo = new GameObject("FpsHud");
            hudGo.transform.SetParent(canvasGo.transform, false);
            var hud = hudGo.AddComponent<FPS.UI.FpsHud>();
            var hudSo = new SerializedObject(hud);

            // Health bar
            var healthBgGo = CreateImage("Health_BG", canvasGo.transform, new Vector2(40, 40), new Vector2(420, 30), new Color(0.15f, 0.15f, 0.15f, 0.8f));
            var healthFillGo = CreateImage("Health_Fill", canvasGo.transform, new Vector2(40, 40), new Vector2(410, 22), new Color(0.1f, 0.8f, 0.2f));
            var healthFill = healthFillGo.GetComponent<UnityEngine.UI.Image>();
            healthFill.type = UnityEngine.UI.Image.Type.Filled;
            healthFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            healthFill.fillOrigin = 0;

            var healthText = CreateText("Health_Text", canvasGo.transform, new Vector2(40, 100), new Vector2(140, 30), "100", 24, TextAnchor.MiddleLeft);
            var ammoText = CreateText("Ammo_Text", canvasGo.transform, new Vector2(1780, 40), new Vector2(120, 40), "30", 44, TextAnchor.MiddleRight);
            var reserveText = CreateText("Reserve_Text", canvasGo.transform, new Vector2(1780, 90), new Vector2(120, 24), "120", 22, TextAnchor.MiddleRight);
            var weaponNameText = CreateText("Weapon_Text", canvasGo.transform, new Vector2(1740, 130), new Vector2(160, 28), "Rifle", 20, TextAnchor.MiddleRight);
            var messageText = CreateText("Message_Text", canvasGo.transform, new Vector2(960, 350), new Vector2(600, 60), "", 30, TextAnchor.MiddleCenter);
            messageText.gameObject.SetActive(false);

            // Crosshair
            var crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform, false);
            Image cImage = crosshairGo.AddComponent<UnityEngine.UI.Image>();
            cImage.color = Color.white;
            var cRect = crosshairGo.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(24, 24);
            var crosshairRect = cRect;

            // Wire HUD references
            hudSo.FindProperty("healthFill").objectReferenceValue = healthFill;
            hudSo.FindProperty("healthText").objectReferenceValue = healthText;
            hudSo.FindProperty("ammoText").objectReferenceValue = ammoText;
            hudSo.FindProperty("reserveText").objectReferenceValue = reserveText;
            hudSo.FindProperty("weaponNameText").objectReferenceValue = weaponNameText;
            hudSo.FindProperty("messageText").objectReferenceValue = messageText;
            hudSo.FindProperty("crosshair").objectReferenceValue = crosshairGo;

            // HUD reference to player systems
            var weaponController = _playerRoot.GetComponent<FPS.Weapons.WeaponController>();
            if (weaponController != null)
                hudSo.FindProperty("weaponController").objectReferenceValue = weaponController;

            var healthSystem = _playerRoot.GetComponent<FPS.Health.HealthSystem>();
            if (healthSystem != null)
                hudSo.FindProperty("playerHealth").objectReferenceValue = healthSystem;

            hudSo.ApplyModifiedProperties();
        }

        private static GameObject CreateImage(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
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
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
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

        private static void BuildAudio()
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

        private static void BuildGameManager()
        {
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<FPS.Managers.GameManager>();
            var so = new SerializedObject(gm);

            // Player reference
            so.FindProperty("playerTransform").objectReferenceValue = _playerRoot.transform;

            // Initial spawn point
            var initialSpawn = GameObject.Find("InitialSpawn");
            if (initialSpawn == null)
            {
                initialSpawn = new GameObject("InitialSpawn");
                initialSpawn.transform.position = new Vector3(0f, 0.9f, 55f);
            }
            so.FindProperty("initialSpawnPoint").objectReferenceValue = initialSpawn.transform;

            // Menus
            var hudCanvas = GameObject.Find("HUDCanvas");
            if (hudCanvas != null)
                so.FindProperty("hudObject").objectReferenceValue = hudCanvas;

            so.ApplyModifiedProperties();

            // Pause menu
            var pauseGo = new GameObject("PauseMenu", typeof(FPS.UI.PauseMenu));
            // GameOver menu
            var gameOverGo = new GameObject("GameOverMenu", typeof(FPS.UI.GameOverMenu));
            // Mission complete
            var mcGo = new GameObject("MissionCompleteMenu", typeof(FPS.UI.MissionCompleteMenu));

            var gmSo2 = new SerializedObject(gm);
            gmSo2.FindProperty("pauseMenu").objectReferenceValue = pauseGo;
            gmSo2.FindProperty("gameOverMenu").objectReferenceValue = gameOverGo;
            gmSo2.FindProperty("missionCompleteMenu").objectReferenceValue = mcGo;
            gmSo2.ApplyModifiedProperties();
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

        private static PhysicsMaterial CreatePhysMat(float dynamicFriction, float bounciness)
        {
            PhysicsMaterial m = new PhysicsMaterial("FPSFriction");
            m.dynamicFriction = dynamicFriction;
            m.staticFriction = dynamicFriction;
            m.bounciness = bounciness;
            return m;
        }

        private static void MarkNavStatic(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic);
        }

        private static void BakeNavMesh()
        {
            // Build navmesh from Navigation settings (all marked objects)
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        }
    }
}
#endif