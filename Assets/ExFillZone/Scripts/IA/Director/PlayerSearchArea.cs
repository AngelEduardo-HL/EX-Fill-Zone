using System.Collections.Generic;
using ExFillZone.AI.Enemy;
using UnityEngine;

namespace ExFillZone.AI.Director
{
    // Shared search data only: individual movement and timers belong to EnemyBrain.
    public sealed class PlayerSearchArea
    {
        private readonly List<EnemyBrain> members = new List<EnemyBrain>();

        public Vector3 Center { get; }
        public float Radius { get; }
        public float CreatedAt { get; }
        public int MemberCount => members.Count;

        public PlayerSearchArea(Vector3 center, float radius, float createdAt)
        {
            Center = center;
            Radius = radius;
            CreatedAt = createdAt;
        }

        public bool Contains(EnemyBrain enemy) => members.Contains(enemy);
        public int IndexOf(EnemyBrain enemy) => members.IndexOf(enemy);

        public void Join(EnemyBrain enemy)
        {
            if (enemy != null && !members.Contains(enemy)) members.Add(enemy);
        }

        public void Leave(EnemyBrain enemy)
        {
            members.Remove(enemy);
        }
    }
}
