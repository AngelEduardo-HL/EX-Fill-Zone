using UnityEngine;

namespace ExFillZone.Gameplay.CameraSystem
{
    public sealed class BuildingFadeGroup : MonoBehaviour
    {
        [SerializeField] private BuildingFadeTarget[] targets;

        private int activeUsers;

        public void Enter()
        {
            activeUsers++;

            if (activeUsers > 1) return;

            foreach (BuildingFadeTarget target in targets)
            {
                if (target != null) target.AddUser();
            }
        }

        public void Exit()
        {
            activeUsers = Mathf.Max(0, activeUsers - 1);

            if (activeUsers > 0) return;

            foreach (BuildingFadeTarget target in targets)
            {
                if (target != null) target.RemoveUser();
            }
        }
    }
}