

using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount, bool isHead);

    //void TakeDamage(int amount, Collider collider, Vector3 hitPoint, Vector3 hitNormal);
}