using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPS.Progression
{
    /// <summary>
    /// Persistent player progression: which level is unlocked and which one was
    /// last played. Uses PlayerPrefs. Level numbering is 1-based (Level01..Level09).
    /// </summary>
    public static class LevelProgress
    {
        private const string UnlockedKey = "FPS_UnlockedLevel";
        private const string LastPlayedKey = "FPS_LastPlayedLevel";

        public const int LevelCount = 9;
        public const int VictoryLevel = LevelCount;

        public static int UnlockedLevel
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(UnlockedKey, 1), 1, LevelCount); }
            private set { PlayerPrefs.SetInt(UnlockedKey, value); PlayerPrefs.Save(); }
        }

        public static int LastPlayed
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(LastPlayedKey, 1), 1, LevelCount); }
            private set { PlayerPrefs.SetInt(LastPlayedKey, value); PlayerPrefs.Save(); }
        }

        public static bool IsLevelUnlocked(int level)
        {
            return level >= 1 && level <= LevelCount && level <= UnlockedLevel;
        }

        /// <summary>Mark the given level finished: unlock the next one.</summary>
        public static void CompleteLevel(int level)
        {
            if (level < 1 || level > LevelCount)
                return;

            LastPlayed = level;
            UnlockedLevel = Mathf.Max(UnlockedLevel, Mathf.Min(level + 1, LevelCount));
        }

        public static void ResetAllProgress()
        {
            PlayerPrefs.DeleteKey(UnlockedKey);
            PlayerPrefs.DeleteKey(LastPlayedKey);
            PlayerPrefs.Save();
        }

        /// <summary>None when the given level is the final one (victory).</summary>
        public static string GetNextLevelName(int currentLevel)
        {
            if (currentLevel < 1 || currentLevel >= LevelCount)
                return "";
            return "Level" + (currentLevel + 1).ToString("D2");
        }

        /// <summary>"Level02" -> 2 ; anything else -> -1.</summary>
        public static int GetCurrentLevelFromSceneName()
        {
            string name = SceneManager.GetActiveScene().name;
            if (name != null && name.StartsWith("Level") && name.Length == 7)
            {
                int n;
                if (int.TryParse(name.Substring(5), out n))
                    return n;
            }
            return -1;
        }

        public static string SceneNameForLevel(int level)
        {
            if (level < 1 || level > LevelCount)
                return "";
            return "Level" + level.ToString("D2");
        }
    }
}