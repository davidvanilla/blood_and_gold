
using Unity.Cinemachine;
using UnityEngine;

public class HitReaction : MonoBehaviour
{
    private CinemachineImpulseSource impulseSource;

    void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
    }

    public void TriggerHitReaction(float intensity)
    {
        // Generate impulse upward with given intensity
        impulseSource.GenerateImpulse(new Vector3(0, intensity * 0.5f, 0));
    }
}