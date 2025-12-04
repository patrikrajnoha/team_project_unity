using UnityEngine;

public class HideHead : MonoBehaviour
{
    [SerializeField] private string headBoneName = "mixamorig:Head";
    [SerializeField] private Camera firstPersonCamera;   // FPS kamera

    private Transform head;
    private Vector3 originalScale;

    private void Start()
    {
        head = FindChildRecursive(transform, headBoneName);

        if (head != null)
        {
            originalScale = head.localScale;
        }
        else
        {
            Debug.LogWarning("HideHead: Head bone not found!");
        }
    }

    private void LateUpdate()
    {
        if (head == null || firstPersonCamera == null) return;

        if (firstPersonCamera.enabled)
        {
            // FPS – skryť hlavu
            head.localScale = Vector3.zero;
        }
        else
        {
            // TPS – vrátiť pôvodnú veľkosť
            head.localScale = originalScale;
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
