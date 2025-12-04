using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pri zásahu hráča na chvíľu odfarbí obraz (čiernobiela) a plynule ho vráti.
/// Pripoj na objekt s Volume (Global Volume).
/// </summary>
[RequireComponent(typeof(Volume))]
public class DamageScreenDesaturate : MonoBehaviour
{
    [Header("Nastavenia efektu")]
    [Tooltip("Saturácia pri zásahu (−100 = čiernobiela).")]
    public float hitSaturation = -100f;

    [Tooltip("Za aký čas sa má saturácia vrátiť späť (sekundy).")]
    public float returnDuration = 0.4f;

    private Volume _volume;
    private ColorAdjustments _colorAdj;

    private float _defaultSaturation = 0f;
    private Coroutine _currentRoutine;

    private void Awake()
    {
        _volume = GetComponent<Volume>();

        if (_volume == null || _volume.profile == null)
        {
            Debug.LogError("[DamageScreenDesaturate] Volume alebo VolumeProfile chýba!");
            return;
        }

        // nájdeme / pridáme ColorAdjustments
        if (!_volume.profile.TryGet(out _colorAdj))
        {
            _colorAdj = _volume.profile.Add<ColorAdjustments>(true);
        }

        _defaultSaturation = _colorAdj.saturation.value;
    }

    /// <summary>
    /// Zavolaj pri akomkoľvek damage hráča.
    /// </summary>
    public void PlayDamageEffect()
    {
        if (_colorAdj == null) return;

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        // okamžitý „flash“ do čiernobiela
        _colorAdj.saturation.Override(hitSaturation);

        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float lerp = t / returnDuration;
            float sat = Mathf.Lerp(hitSaturation, _defaultSaturation, lerp);
            _colorAdj.saturation.Override(sat);
            yield return null;
        }

        _colorAdj.saturation.Override(_defaultSaturation);
        _currentRoutine = null;
    }
}
