using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JudgePatienceSystem : MonoBehaviour
{
    [Header("Patience config")]
    [Tooltip("Minimo decremento al valor de paciencia al castigar")]
    [SerializeField][Range(0f,1f)] private float _minPunishDecrement;
    [Tooltip("Maximo decremento al valor de paciencia al castigar")]
    [SerializeField][Range(0f, 1f)] private float _maxPunishDecrement;
    [Tooltip("Duración de la animación del slider")]
    [SerializeField] private float _animationDuration = 0.5f;
    [Header("References")]
    [SerializeField] private GameObject _holder;
    [SerializeField] private Slider _slider;

    private float _patience = 1f;
    private Coroutine _coroutine;

    /// <summary>
    /// Genera un booleano random en funcion del valor de paciencia actual. Conforme menor paciencia, mas probabilidad de 
    /// devolver false. Si la paciencia = 1 100% true, si la paciencia = 0, 0% true.
    /// </summary>
    /// <returns></returns>
    public bool RollPatience()
    {
        float random = Random.Range(0f, 1f);

        return random < _patience;
    }

    /// <summary>
    /// Activa o desactiva el visual del sistema de paciencia
    /// </summary>
    /// <param name="on"></param>
    public void TogglePatienceVisual(bool on)
    {
        _holder.SetActive(on);
    }

    /// <summary>
    /// Baja la paciencia un valor random entre _minPunishDecrement y _maxPunishDecrement. Llamado al fallar recursiones o 
    /// si te recurren una prueba.
    /// </summary>
    public void DecrementPatience()
    {
        float decrement = Random.Range(_minPunishDecrement, _maxPunishDecrement);

        float oldPatinece = _patience;
        _patience = Mathf.Clamp01(_patience - decrement);

        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(updateVisual(oldPatinece, _patience));
    }



    /// <summary>
    /// Coroutina para animar la bajada del slider de paciencia
    /// </summary>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <returns></returns>
    private IEnumerator updateVisual(float startValue, float endValue)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / _animationDuration);

            float easedT = Mathf.SmoothStep(0, 1, t);

            _slider.value = Mathf.Lerp(startValue, endValue, easedT);

            yield return null;
        }

        _slider.value = endValue;

        _coroutine = null;
    }

    private void OnDisable()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }
}
