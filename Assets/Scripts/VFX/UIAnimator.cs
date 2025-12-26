using UnityEngine;

namespace Rumbax.VFX
{
    /// <summary>
    /// Animates UI elements with various effects.
    /// </summary>
    public class UIAnimator : MonoBehaviour
    {
        [Header("Punch Scale")]
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _punchDuration = 0.2f;

        [Header("Shake")]
        [SerializeField] private float _shakeIntensity = 10f;
        [SerializeField] private float _shakeDuration = 0.3f;

        [Header("Fade")]
        [SerializeField] private float _fadeDuration = 0.3f;

        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalPosition = transform.localPosition;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Punch scale animation (button press effect).
        /// </summary>
        public void PunchScale()
        {
            StopAllCoroutines();
            StartCoroutine(PunchScaleCoroutine());
        }

        private System.Collections.IEnumerator PunchScaleCoroutine()
        {
            float elapsed = 0f;
            float halfDuration = _punchDuration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(1f, _punchScale, t);
                transform.localScale = _originalScale * scale;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(_punchScale, 1f, t);
                transform.localScale = _originalScale * scale;
                yield return null;
            }

            transform.localScale = _originalScale;
        }

        /// <summary>
        /// Shake animation (error/notification effect).
        /// </summary>
        public void Shake()
        {
            StopAllCoroutines();
            StartCoroutine(ShakeCoroutine());
        }

        private System.Collections.IEnumerator ShakeCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < _shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float decay = 1f - (elapsed / _shakeDuration);
                float offsetX = Random.Range(-_shakeIntensity, _shakeIntensity) * decay;
                float offsetY = Random.Range(-_shakeIntensity, _shakeIntensity) * decay;
                
                transform.localPosition = _originalPosition + new Vector3(offsetX, offsetY, 0);
                yield return null;
            }

            transform.localPosition = _originalPosition;
        }

        /// <summary>
        /// Fade in animation.
        /// </summary>
        public void FadeIn()
        {
            if (_canvasGroup == null) return;
            StopAllCoroutines();
            StartCoroutine(FadeCoroutine(0f, 1f));
        }

        /// <summary>
        /// Fade out animation.
        /// </summary>
        public void FadeOut()
        {
            if (_canvasGroup == null) return;
            StopAllCoroutines();
            StartCoroutine(FadeCoroutine(1f, 0f));
        }

        private System.Collections.IEnumerator FadeCoroutine(float from, float to)
        {
            float elapsed = 0f;
            _canvasGroup.alpha = from;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _fadeDuration;
                _canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _canvasGroup.alpha = to;
        }

        /// <summary>
        /// Bounce animation (reward effect).
        /// </summary>
        public void Bounce()
        {
            StopAllCoroutines();
            StartCoroutine(BounceCoroutine());
        }

        private System.Collections.IEnumerator BounceCoroutine()
        {
            float[] keyframes = { 1f, 1.15f, 0.9f, 1.05f, 0.95f, 1f };
            float frameDuration = 0.1f;

            for (int i = 0; i < keyframes.Length - 1; i++)
            {
                float elapsed = 0f;
                while (elapsed < frameDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / frameDuration;
                    float scale = Mathf.Lerp(keyframes[i], keyframes[i + 1], t);
                    transform.localScale = _originalScale * scale;
                    yield return null;
                }
            }

            transform.localScale = _originalScale;
        }

        /// <summary>
        /// Slide in from direction.
        /// </summary>
        public void SlideIn(Vector2 fromOffset)
        {
            StopAllCoroutines();
            StartCoroutine(SlideCoroutine(fromOffset, Vector2.zero));
        }

        /// <summary>
        /// Slide out to direction.
        /// </summary>
        public void SlideOut(Vector2 toOffset)
        {
            StopAllCoroutines();
            StartCoroutine(SlideCoroutine(Vector2.zero, toOffset));
        }

        private System.Collections.IEnumerator SlideCoroutine(Vector2 from, Vector2 to)
        {
            float elapsed = 0f;
            float duration = 0.3f;

            Vector3 startPos = _originalPosition + new Vector3(from.x, from.y, 0);
            Vector3 endPos = _originalPosition + new Vector3(to.x, to.y, 0);
            transform.localPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutBack(elapsed / duration);
                transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, t);
                yield return null;
            }

            transform.localPosition = endPos;
        }

        /// <summary>
        /// Pop in animation (scale from 0).
        /// </summary>
        public void PopIn()
        {
            StopAllCoroutines();
            StartCoroutine(PopInCoroutine());
        }

        private System.Collections.IEnumerator PopInCoroutine()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutBack(elapsed / duration);
                transform.localScale = _originalScale * t;
                yield return null;
            }

            transform.localScale = _originalScale;
        }

        /// <summary>
        /// Pop out animation (scale to 0).
        /// </summary>
        public void PopOut()
        {
            StopAllCoroutines();
            StartCoroutine(PopOutCoroutine());
        }

        private System.Collections.IEnumerator PopOutCoroutine()
        {
            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInBack(elapsed / duration);
                transform.localScale = _originalScale * (1f - t);
                yield return null;
            }

            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Pulse animation (continuous).
        /// </summary>
        public void StartPulse()
        {
            StopAllCoroutines();
            StartCoroutine(PulseCoroutine());
        }

        public void StopPulse()
        {
            StopAllCoroutines();
            transform.localScale = _originalScale;
        }

        private System.Collections.IEnumerator PulseCoroutine()
        {
            float elapsed = 0f;
            float cycleDuration = 1f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = (Mathf.Sin(elapsed * Mathf.PI * 2f / cycleDuration) + 1f) / 2f;
                float scale = Mathf.Lerp(0.95f, 1.05f, t);
                transform.localScale = _originalScale * scale;
                yield return null;
            }
        }

        // Easing functions
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        private float EaseOutElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            
            if (t == 0) return 0;
            if (t == 1) return 1;
            
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
    }
}
