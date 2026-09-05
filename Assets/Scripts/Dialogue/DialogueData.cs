using System;
using UnityEngine;

namespace FPS.Dialogue
{
    /// <summary>
    /// Reusable dialogue asset. Editor-created per level/encounter. Keep in
    /// Assets/Dialogue. Voice-ready: assign voiceClip to a line (optional).
    /// </summary>
    [CreateAssetMenu(fileName = "Dialogue", menuName = "FPS/Dialogue", order = 1)]
    public class DialogueData : ScriptableObject
    {
        [Serializable]
        public class Line
        {
            public string speaker = "NPC";
            [TextArea(2, 4)] public string text = "";
            public AudioClip voiceClip;   // future voice support (plays locally)
        }

        public string id = "";
        public Line[] lines = new Line[0];
        public bool playOnLevelStart = false;
        public bool playOnLevelEnd = false;
    }
}