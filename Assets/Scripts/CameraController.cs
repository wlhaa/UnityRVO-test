using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float height = 50.0f;  // 相机高度

    void Start()
    {
        if (target == null)
        {
            target = FindObjectOfType<SphereManager>().transform;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 newPosition = target.position;
            newPosition.y += height;
            transform.position = newPosition;
            transform.LookAt(target);
        }
    }
}

