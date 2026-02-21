using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Nexus.Mathematics
{
    /// <summary>
    /// NexusTrajectorySimulator: Predicts physics paths using a secondary simulation scene.
    /// Integrated from HypeFire framework architecture.
    /// </summary>
    public class NexusTrajectorySimulator : MonoBehaviour
    {
        [Header("Settings")]
        public int MaxIterations = 100;
        public LineRenderer Line;
        public Transform ObstaclesRoot;

        private Scene _simScene;
        private PhysicsScene _physicsScene;
        private readonly Dictionary<Transform, Transform> _syncMap = new();

        private void Start()
        {
            InitializeSimulation();
        }

        private void InitializeSimulation()
        {
            if (ObstaclesRoot == null) return;

            _simScene = SceneManager.CreateScene("Nexus_SimScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            _physicsScene = _simScene.GetPhysicsScene();

            foreach (Transform child in ObstaclesRoot)
            {
                var ghost = Instantiate(child.gameObject, child.position, child.rotation);
                ghost.GetComponent<Renderer>().enabled = false;
                
                // Disable children of ghosts (visuals, etc)
                for (int i = 0; i < ghost.transform.childCount; i++)
                    ghost.transform.GetChild(i).gameObject.SetActive(false);

                SceneManager.MoveGameObjectToScene(ghost, _simScene);
                if (!ghost.isStatic) _syncMap.Add(child, ghost.transform);
            }
        }

        public void Simulate(GameObject prefab, Vector3 pos, Vector3 velocity, ForceMode mode = ForceMode.Impulse)
        {
            // Sync dynamic obstacles
            foreach (var pair in _syncMap)
            {
                if (pair.Key == null || pair.Value == null) continue;
                pair.Value.SetPositionAndRotation(pair.Key.position, pair.Key.rotation);
            }

            var ghostBall = Instantiate(prefab, pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(ghostBall, _simScene);
            
            var rb = ghostBall.GetComponent<Rigidbody>();
            rb.AddForce(velocity, mode);

            Line.positionCount = MaxIterations;
            for (int i = 0; i < MaxIterations; i++)
            {
                _physicsScene.Simulate(Time.fixedDeltaTime);
                Line.SetPosition(i, ghostBall.transform.position);
            }

            Destroy(ghostBall);
        }
    }
}
