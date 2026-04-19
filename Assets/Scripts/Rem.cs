using UnityEngine;

public class Rem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = true;
        transform.position += new Vector3(0, 1f, 0);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAnimatorMove()
    {
        if (_animator == null) return;

        Vector3 delta = _animator.deltaPosition;
        transform.position += delta;
        transform.rotation *= _animator.deltaRotation;
    }
}
