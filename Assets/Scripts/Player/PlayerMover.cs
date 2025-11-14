using System.Collections;
using System.Collections.Generic;
using IdleScaper.World;
using UnityEngine;

namespace IdleScaper.Player
{
    /// <summary>
    /// Moves the player along grid paths.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        private CharacterController controller;
        private Coroutine moveRoutine;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Moves the player to the world position of the target using grid pathfinding.
        /// </summary>
        public void MoveTo(Vector3 targetWorldPosition)
        {
            var start = GridManager.WorldToGrid(transform.position);
            var goal = GridManager.WorldToGrid(targetWorldPosition);

            var path = GridPathfinder.FindPath(start, goal);
            if (path == null || path.Count == 0)
                return;

            if (moveRoutine != null)
                StopCoroutine(moveRoutine);

            moveRoutine = StartCoroutine(MoveAlongPath(path));
        }

        private IEnumerator MoveAlongPath(List<GridPosition> path)
        {
            // skip first node if it's current tile
            for (int i = 0; i < path.Count; i++)
            {
                var targetPos = GridManager.GridToWorld(path[i]);

                while (Vector3.Distance(transform.position, targetPos) > 0.05f)
                {
                    var dir = (targetPos - transform.position).normalized;
                    controller.Move(dir * (moveSpeed * Time.deltaTime));
                    yield return null;
                }
            }

            moveRoutine = null;
        }
    }
}