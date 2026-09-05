#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPS.EditorTools
{
    /// <summary>
    /// Places the downloaded CC0 Poly Haven models (street lamps, benches, road barriers,
    /// building facade, electricity poles, city rat) into the FPS scene on the street/sidewalk.
    /// The game stays playable without them (primitives are used as fallback).
    /// </summary>
    public static class FpsCityProps
    {
        private const string Folder = "Assets/Models/Environment";
        private static int _placed;

        [MenuItem("FPS/Place Real City Props (Poly Haven)")]
        public static void PlaceCityProps()
        {
            if (SceneManager.GetActiveScene().name == string.Empty && SceneManager.sceneCount == 0)
            {
                Debug.LogWarning("[FPS] Open the FpsGame scene first (FpsGame.unity).");
                return;
            }

            var root = GameObject.Find("CityProps") ?? new GameObject("CityProps");
            Undo.RegisterCreatedObjectUndo(root, "Place City Props");

            _placed = 0;

            Place("street_lamp_01", new Vector3(28f, 0f, 30f), 4.5f, 0f);
            Place("street_lamp_01", new Vector3(32f, 0f, 18f), 4.5f, 0f);
            Place("street_lamp_01", new Vector3(28f, 0f, -25f), 4.5f, 0f);

            Place("painted_wooden_bench", new Vector3(-16f, 0f, 22f), 1.4f, 90f);
            Place("painted_wooden_bench", new Vector3(-16f, 0f, 30f), 1.4f, 90f);

            Place("concrete_road_barrier", new Vector3(14f, 0f, -5f), 1.6f, 0f);
            Place("concrete_road_barrier", new Vector3(25f, 0f, -5f), 1.6f, 0f);
            Place("concrete_road_barrier", new Vector3(14f, 0f, 20f), 1.6f, 0f);

            Place("modular_electricity_poles", new Vector3(22f, 0f, 40f), 8f, 0f);
            Place("modular_electricity_poles", new Vector3(5f, 0f, -45f), 8f, 40f);

            Place("modular_urban_apartments_facade", new Vector3(34f, 0f, -25f), 12f, 90f);

            Place("street_rat", new Vector3(-2f, 0f, 30f), 0.5f, 0f);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[FPS] Placed {_placed} real city props. Game remains playable with/without them.");
        }

        private static void Place(string id, Vector3 position, float targetSize, float yaw)
        {
            string path = Folder + "/" + id + "/" + id + ".gltf";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
            {
                Debug.LogWarning($"[FPS] Model not imported yet: {path}. Reimport after adding the glTFast package, then run this again.");
                return;
            }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(model);
            inst.name = id + "_Placed";
            inst.transform.SetParent(GameObject.Find("CityProps").transform, true);

            inst.transform.position = position;

            // Normalize to target size using renderer bounds
            var renderers = inst.GetComponentsInChildren<Renderer>();
            Bounds total = new Bounds(inst.transform.position, Vector3.zero);
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

            // Lift so the lowest point sits on the ground (y=0)
            Collider[] baseColliders = inst.GetComponentsInChildren<Collider>();
            float minY = float.MaxValue;
            foreach (var r in renderers)
                minY = Mathf.Min(minY, r.bounds.min.y);
            foreach (var c in baseColliders)
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

            _placed++;
        }
    }
}
#endif