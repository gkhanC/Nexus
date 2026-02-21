#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nexus.Editor
{
    /// <summary>
    /// NexusRandmonoizer: Editor-side tool for rapid transform randomization.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public static class NexusRandmonoizer
    {
        public enum AxisType { All, X, Y, Z, XY, XZ, YZ, Circle, Sphere }

        public static void RandomizePosition(float min, float max, AxisType axis = AxisType.All)
        {
            foreach (var go in Selection.gameObjects)
            {
                var pos = go.transform.localPosition;
                if (axis == AxisType.All || axis == AxisType.X || axis == AxisType.XY || axis == AxisType.XZ) pos.x = Random.Range(min, max);
                if (axis == AxisType.All || axis == AxisType.Y || axis == AxisType.XY || axis == AxisType.YZ) pos.y = Random.Range(min, max);
                if (axis == AxisType.All || axis == AxisType.Z || axis == AxisType.XZ || axis == AxisType.YZ) pos.z = Random.Range(min, max);
                
                if (axis == AxisType.Circle) pos = (Vector3)(Random.insideUnitCircle * max);
                if (axis == AxisType.Sphere) pos = Random.insideUnitSphere * max;

                go.transform.localPosition = pos;
            }
        }

        public static void RandomizeRotation(float min, float max)
        {
            foreach (var go in Selection.gameObjects)
            {
                go.transform.localRotation = Quaternion.Euler(
                    Random.Range(min, max),
                    Random.Range(min, max),
                    Random.Range(min, max)
                );
            }
        }

        public static void RandomizeScale(float min, float max)
        {
            foreach (var go in Selection.gameObjects)
            {
                float s = Random.Range(min, max);
                go.transform.localScale = new Vector3(s, s, s);
            }
        }
    }

    public class NexusRandmonoizerWindow : EditorWindow
    {
        [MenuItem("Nexus/Randmonoizer")]
        public static void ShowWindow() => GetWindow<NexusRandmonoizerWindow>("Nexus Randmonoizer");

        private float _min = -1f;
        private float _max = 1f;

        private void OnGUI()
        {
            GUILayout.Label("Transform Randmonoizer", EditorStyles.boldLabel);
            _min = EditorGUILayout.FloatField("Min Range", _min);
            _max = EditorGUILayout.FloatField("Max Range", _max);

            if (GUILayout.Button("Randomize Position")) NexusRandmonoizer.RandomizePosition(_min, _max);
            if (GUILayout.Button("Randomize Rotation")) NexusRandmonoizer.RandomizeRotation(0, 360);
            if (GUILayout.Button("Randomize Scale")) NexusRandmonoizer.RandomizeScale(_min, _max);
        }
    }
}
#endif
