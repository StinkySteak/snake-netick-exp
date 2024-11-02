using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;

    private void LateUpdate()
    {
        if (Target == null) return;

        transform.position = Target.transform.position;
    }
}
