using UnityEngine;

namespace FPS.Tools
{
    /// <summary>
    /// Marks a placeholder object that awaits manual model import (Sketchfab
    /// model not downloadable automatically). Shows the exact URL + expected
    /// import path so the workflow is fully documented in-scene.
    /// </summary>
    [AddComponentMenu("FPS/Model Placeholder")]
    public class ModelPlaceholder : MonoBehaviour
    {
        [Tooltip("Sketchfab URL of the model to import (manual).")]
        public string sourceUrl = "";

        [Tooltip("Expected final import path under Assets/Models/...")]
        public string expectedImportPath = "";

        [Tooltip("Import license of the original model.")]
        public string licenseNote = "";

        [Tooltip("Category: Background | Player | Enemy | Weapon")]
        public string category = "";

        /// <summary>Name of the placeholder group (used by workshops to match).</summary>
        public string objectName = "";

        private void Awake()
        {
            objectName = gameObject.name;
        }

        private void OnValidate()
        {
            objectName = gameObject.name;
        }

        public void LogPending()
        {
            Debug.Log(string.Format(
                "[FPS→Model] Placeholder '{0}': import '{1}' -> {2} (licença: {3})",
                gameObject.name, sourceUrl, expectedImportPath, licenseNote));
        }
    }

    /// <summary>
    /// Finds the FPS placeholder prefabs by Resources path.
    /// </summary>
    public static class PlaceholderRegistry
    {
        private const string Root = "FPS/Prefabs/";

        public static GameObject FindEnemy()
        {
            return Resources.Load<GameObject>(Root + "EnemyBase");
        }

        public static GameObject FindPlayer()
        {
            return Resources.Load<GameObject>(Root + "PlayerBase");
        }

        public static GameObject FindWeapon()
        {
            return Resources.Load<GameObject>(Root + "WeaponBase");
        }

        public static GameObject FindBackground()
        {
            return Resources.Load<GameObject>(Root + "BackgroundBase");
        }
    }
}