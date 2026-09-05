using UnityEngine;

namespace FPS.AI
{
    /// <summary>
    /// Spawns enemies at every child of the EnemySpawnPoints group at level
    /// start. Handles empty spawn points gracefully. Placeholder prefab can be
    /// swapped for the real imported enemy prefab later.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Tooltip("Prefab to spawn at each EnemySpawnPoint child (placeholder or real enemy).")]
        [SerializeField] private GameObject enemyPrefab;

        [Tooltip("Sensitivity for higher-difficulty levels (Level06+).")]
        [SerializeField] private float difficultyScale = 1f;

        public int SpawnedCount { get; private set; }

        private void Start()
        {
            if (enemyPrefab == null)
                enemyPrefab = FPS.Tools.PlaceholderRegistry.FindEnemy();
            SpawnAll();
        }

        public void SetDifficulty(float scale)
        {
            difficultyScale = Mathf.Max(1f, scale);
        }

        public void SpawnAll()
        {
            SpawnedCount = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform point = transform.GetChild(i);
                if (point.name.StartsWith("EnemySpawn"))
                {
                    SpawnAt(point, i);
                }
            }
        }

        public GameObject SpawnAt(Transform point, int index)
        {
            if (enemyPrefab == null)
                return null;

            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            enemy.name = "Enemy_" + index;
            enemy.transform.SetParent(point);
            SpawnedCount++;

            ApplyDifficulty(enemy, index);
            return enemy;
        }

        private void ApplyDifficulty(GameObject enemy, int index)
        {
            if (difficultyScale <= 1f)
                return;

            var ai = enemy.GetComponent<FPS.Enemies.EnemyAI>();
            if (ai == null)
                return;

            ai.MultiplyDifficulty(difficultyScale);
        }
    }
}