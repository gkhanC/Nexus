using UnityEngine;

namespace Nexus.Mathematics
{
    /// <summary>
    /// NexusHueShifter: Visual utility for shifting colors over time.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public static class NexusHueShifter
    {
        private static float GetBaseHue()
        {
#if UNITY_EDITOR
            // Request repaint for smooth animation in Inspector if using Odin
            // Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); 
            return Mathf.Cos((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f;
#else
            return Mathf.Cos(Time.time + 1f) * 0.225f + 0.325f;
#endif
        }

        public static Color GetColor(float offset = 0f)
        {
            return Color.HSVToRGB((GetBaseHue() + offset) % 1f, 1, 1);
        }

        public static Color SoftBlue => GetColor(0.2f);
        public static Color SoftGreen => GetColor(0.4f);
        public static Color SoftRed => GetColor(0.6f);
        public static Color SoftYellow => GetColor(0.8f);
    }
}
