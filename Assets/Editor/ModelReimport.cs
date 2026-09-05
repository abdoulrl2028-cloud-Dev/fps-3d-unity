#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace FPS.EditorTools
{
    /// <summary>
    /// Self-healing reimport for the Poly Haven models moved to Assets/Models/Environment.
    /// On every domain reload it checks each .gltf: if it produced no Mesh sub-assets
    /// (e.g. transient import failure after the folder move), it force-reimports the
    /// asset synchronously. If a reimport still fails, the importer's stored errors are
    /// written to the Console so the real cause is visible.
    /// </summary>
    [InitializeOnLoad]
    public static class ModelReimport
    {
        private static bool _ran;

        static ModelReimport()
        {
            EditorApplication.delayCall += AutoRun;
        }

        [DidReloadScripts]
        private static void OnDidReloadScripts()
        {
            EditorApplication.delayCall += AutoRun;
        }

        /// <summary>
        /// Public entry used by the level pipeline: re-imports every .gltf that
        /// produced no Mesh sub-asset. Returns true when every model is valid.
        /// </summary>
        public static bool EnsureModelsImported()
        {
            string root = "Assets/Models/Environment";
            if (!AssetDatabase.IsValidFolder(root))
                return false;

            bool allOk = true;
            string[] gltfs = Directory.GetFiles(root, "*.gltf", SearchOption.AllDirectories);
            foreach (string path in gltfs)
            {
                if (AssetDatabase.LoadAllAssetsAtPath(path).Any(a => a is Mesh))
                    continue;
                CheckAndReimport(path);

                if (!AssetDatabase.LoadAllAssetsAtPath(path).Any(a => a is Mesh))
                    allOk = false;
            }
            return allOk;
        }

        private static void AutoRun()
        {
            if (_ran || Application.isPlaying || BuildPipeline.isBuildingPlayer)
                return;
            _ran = true;

            string root = "Assets/Models/Environment";
            if (!AssetDatabase.IsValidFolder(root))
                return;

            string[] gltfs = Directory.GetFiles(root, "*.gltf", SearchOption.AllDirectories);
            foreach (string path in gltfs)
                CheckAndReimport(path);
        }

        private static void CheckAndReimport(string path)
        {
            if (AssetDatabase.LoadAllAssetsAtPath(path).Any(a => a is Mesh))
                return;

            AssetDatabase.ImportAsset(path,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            int meshes = assets.Count(a => a is Mesh);
            int materials = assets.Count(a => a is Material);

            if (meshes > 0)
            {
                Debug.Log("[FPS] Model reimported OK: " + path + " (" + meshes + " meshes, " +
                          materials + " materials)");
                return;
            }

            Debug.LogError("[FPS] Model reimport still failed: " + path);
            LogImporterErrors(path);
        }

        private static void LogImporterErrors(string path)
        {
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer == null)
                return;

            FieldInfo field = importer.GetType().GetField("reportItems",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                return;

            var items = field.GetValue(importer) as System.Array;
            if (items == null || items.Length == 0)
                return;

            foreach (object item in items)
            {
                PropertyInfo typeProp = item.GetType().GetProperty("Type",
                    BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo messagesProp = item.GetType().GetProperty("Messages",
                    BindingFlags.Public | BindingFlags.Instance);
                if (messagesProp == null)
                    continue;

                string type = typeProp != null && typeProp.GetValue(item, null) != null
                    ? typeProp.GetValue(item, null).ToString()
                    : "Info";
                string[] messages = messagesProp.GetValue(item, null) as string[];
                if (messages == null)
                    continue;

                Debug.LogError("[FPS]   " + type + ": " + string.Join(" | ", messages));
            }
        }
    }
}
#endif