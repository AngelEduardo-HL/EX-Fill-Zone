using UnityEngine;

namespace ExFillZone.Gameplay.Combat
{
    public abstract class Damageable : MonoBehaviour
    {
        public abstract void TakeDamage(float damage);
    }
}