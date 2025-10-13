using UnityEngine;

public class WorldUI : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.up * 2f;
    private Camera mainCamera;

    private void Start() => mainCamera = Camera.main;

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
}
