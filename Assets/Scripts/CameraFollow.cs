using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5); // Local offset
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float pitchAngle = 15f; // Degrees down from horizontal

    private void LateUpdate()
    {
        // Desired position relative to player
        Vector3 desiredPosition = player.position + player.rotation * offset;

        // Smooth position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Rotation: look at player but with fixed pitch
        Vector3 directionToPlayer = player.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized);
        // Apply pitch offset (rotate around local X axis)
        targetRotation *= Quaternion.Euler(pitchAngle, 0, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }

}

