using UnityEngine;

namespace ExFillZone.AI.Shared.Data
{
    public readonly struct GunshotClue
    {
        public Vector3 SoundOrigin { get; }

        public Vector3 InvestigationPoint { get; }

        public float CreatedAt { get; }

        public float Priority { get; }

        public GunshotClue(
            Vector3 soundOrigin,
            Vector3 investigationPoint,
            float createdAt,
            float priority
        )
        {
            SoundOrigin = soundOrigin;
            InvestigationPoint = investigationPoint;
            CreatedAt = createdAt;
            Priority = priority;
        }
    }
}