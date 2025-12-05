using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [Tooltip("O koľko sa tlačidlo zväčší pri hoveri.")]
    public float hoverScale = 1.1f;

    [Tooltip("Rýchlosť animácie pri zväčšení/zmenšení.")]
    public float scaleSpeed = 8f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // Plynulá animácia medzi current scale a target scale
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed   // funguje aj počas pause
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
