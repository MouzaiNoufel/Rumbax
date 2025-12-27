using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Visual Effects System - Professional particle effects, animations, and polish.
    /// Handles all visual juice for a polished mobile game experience.
    /// </summary>
    public class VFXSystem : MonoBehaviour
    {
        public static VFXSystem Instance { get; private set; }

        // Object pools for performance
        private Queue<GameObject> _particlePool = new Queue<GameObject>();
        private Queue<GameObject> _floatTextPool = new Queue<GameObject>();
        private Queue<GameObject> _trailPool = new Queue<GameObject>();

        // Screen effects
        private GameObject _screenFlash;
        private Image _flashImage;
        private GameObject _vignetteOverlay;
        private Image _vignetteImage;

        // Camera reference
        private Camera _mainCamera;
        private Vector3 _originalCameraPos;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePools();
                CreateScreenEffects();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                _originalCameraPos = _mainCamera.transform.position;
            }
        }

        private void InitializePools()
        {
            // Pre-create pooled objects
            for (int i = 0; i < 20; i++)
            {
                _particlePool.Enqueue(CreatePooledParticle());
                _floatTextPool.Enqueue(CreatePooledFloatText());
            }
        }

        private GameObject CreatePooledParticle()
        {
            GameObject obj = new GameObject("PooledParticle");
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            return obj;
        }

        private GameObject CreatePooledFloatText()
        {
            GameObject obj = new GameObject("PooledFloatText");
            obj.transform.SetParent(transform);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            
            obj.SetActive(false);
            return obj;
        }

        private void CreateScreenEffects()
        {
            // Find or create UI canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Screen Flash
            _screenFlash = new GameObject("ScreenFlash");
            _screenFlash.transform.SetParent(canvas.transform, false);
            _flashImage = _screenFlash.AddComponent<Image>();
            _flashImage.color = new Color(1f, 1f, 1f, 0f);
            _flashImage.raycastTarget = false;
            RectTransform flashRect = _screenFlash.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.sizeDelta = Vector2.zero;
            _screenFlash.SetActive(false);

            // Vignette Overlay
            _vignetteOverlay = new GameObject("VignetteOverlay");
            _vignetteOverlay.transform.SetParent(canvas.transform, false);
            _vignetteImage = _vignetteOverlay.AddComponent<Image>();
            _vignetteImage.color = new Color(0f, 0f, 0f, 0f);
            _vignetteImage.raycastTarget = false;
            RectTransform vigRect = _vignetteOverlay.GetComponent<RectTransform>();
            vigRect.anchorMin = Vector2.zero;
            vigRect.anchorMax = Vector2.one;
            vigRect.sizeDelta = Vector2.zero;
        }

        // === PARTICLE EFFECTS ===

        public void SpawnExplosion(Vector3 position, Color color, float size = 1f)
        {
            StartCoroutine(ExplosionRoutine(position, color, size));
        }

        private IEnumerator ExplosionRoutine(Vector3 position, Color color, float size)
        {
            int particleCount = 12;
            List<GameObject> particles = new List<GameObject>();
            List<SpriteRenderer> renderers = new List<SpriteRenderer>();
            List<Vector3> velocities = new List<Vector3>();

            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = GetParticle();
                particle.transform.position = position;
                particle.transform.localScale = Vector3.one * size * 0.3f;

                SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
                if (sr == null) sr = particle.AddComponent<SpriteRenderer>();
                sr.sprite = Testing.SpriteGenerator.CreateCircle(16, color);
                sr.sortingOrder = 100;

                float angle = (360f / particleCount) * i + UnityEngine.Random.Range(-15f, 15f);
                float speed = UnityEngine.Random.Range(3f, 6f) * size;
                Vector3 velocity = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * speed,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * speed,
                    0
                );

                particles.Add(particle);
                renderers.Add(sr);
                velocities.Add(velocity);
            }

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                for (int i = 0; i < particles.Count; i++)
                {
                    particles[i].transform.position += velocities[i] * Time.deltaTime;
                    velocities[i] *= 0.95f; // Drag
                    
                    // Fade and shrink
                    Color c = renderers[i].color;
                    renderers[i].color = new Color(c.r, c.g, c.b, 1f - t);
                    particles[i].transform.localScale = Vector3.one * size * 0.3f * (1f - t);
                }

                yield return null;
            }

            foreach (var particle in particles)
            {
                ReturnParticle(particle);
            }
        }

        public void SpawnSparkle(Vector3 position, Color color)
        {
            StartCoroutine(SparkleRoutine(position, color));
        }

        private IEnumerator SparkleRoutine(Vector3 position, Color color)
        {
            GameObject sparkle = GetParticle();
            sparkle.transform.position = position;
            sparkle.transform.localScale = Vector3.zero;

            SpriteRenderer sr = sparkle.GetComponent<SpriteRenderer>();
            if (sr == null) sr = sparkle.AddComponent<SpriteRenderer>();
            sr.sprite = Testing.SpriteGenerator.CreateSquare(16, color);
            sr.sortingOrder = 100;

            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Pop in then fade
                float scale;
                if (t < 0.3f)
                    scale = Mathf.Lerp(0f, 0.5f, t / 0.3f);
                else
                    scale = Mathf.Lerp(0.5f, 0f, (t - 0.3f) / 0.7f);

                sparkle.transform.localScale = Vector3.one * scale;
                sparkle.transform.Rotate(0, 0, 360f * Time.deltaTime);
                sr.color = new Color(color.r, color.g, color.b, 1f - (t * t));

                yield return null;
            }

            ReturnParticle(sparkle);
        }

        public void SpawnCoinBurst(Vector3 position, int count = 5)
        {
            StartCoroutine(CoinBurstRoutine(position, count));
        }

        private IEnumerator CoinBurstRoutine(Vector3 position, int count)
        {
            Color goldColor = new Color(1f, 0.85f, 0.2f);
            List<GameObject> coins = new List<GameObject>();
            List<Vector3> velocities = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                GameObject coin = GetParticle();
                coin.transform.position = position;
                coin.transform.localScale = Vector3.one * 0.2f;

                SpriteRenderer sr = coin.GetComponent<SpriteRenderer>();
                if (sr == null) sr = coin.AddComponent<SpriteRenderer>();
                sr.sprite = Testing.SpriteGenerator.CreateCircle(12, goldColor);
                sr.sortingOrder = 100;

                Vector3 velocity = new Vector3(
                    UnityEngine.Random.Range(-2f, 2f),
                    UnityEngine.Random.Range(3f, 5f),
                    0
                );

                coins.Add(coin);
                velocities.Add(velocity);
            }

            float duration = 0.8f;
            float elapsed = 0f;
            float gravity = 15f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                for (int i = 0; i < coins.Count; i++)
                {
                    velocities[i] += Vector3.down * gravity * Time.deltaTime;
                    coins[i].transform.position += velocities[i] * Time.deltaTime;
                    coins[i].transform.Rotate(0, 0, 360f * Time.deltaTime);

                    SpriteRenderer sr = coins[i].GetComponent<SpriteRenderer>();
                    float alpha = 1f - (elapsed / duration);
                    sr.color = new Color(goldColor.r, goldColor.g, goldColor.b, alpha);
                }

                yield return null;
            }

            foreach (var coin in coins)
            {
                ReturnParticle(coin);
            }
        }

        public void SpawnTrail(Transform target, Color color, float duration = 0.3f)
        {
            StartCoroutine(TrailRoutine(target, color, duration));
        }

        private IEnumerator TrailRoutine(Transform target, Color color, float duration)
        {
            List<GameObject> trailPoints = new List<GameObject>();
            float elapsed = 0f;
            float spawnInterval = 0.02f;
            float nextSpawn = 0f;

            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;

                if (elapsed >= nextSpawn)
                {
                    nextSpawn += spawnInterval;

                    GameObject point = GetParticle();
                    point.transform.position = target.position;
                    point.transform.localScale = Vector3.one * 0.15f;

                    SpriteRenderer sr = point.GetComponent<SpriteRenderer>();
                    if (sr == null) sr = point.AddComponent<SpriteRenderer>();
                    sr.sprite = Testing.SpriteGenerator.CreateCircle(8, color);
                    sr.sortingOrder = 50;

                    trailPoints.Add(point);
                }

                // Fade existing trail points
                for (int i = trailPoints.Count - 1; i >= 0; i--)
                {
                    SpriteRenderer sr = trailPoints[i].GetComponent<SpriteRenderer>();
                    Color c = sr.color;
                    c.a -= Time.deltaTime * 3f;
                    sr.color = c;

                    trailPoints[i].transform.localScale *= 0.98f;

                    if (c.a <= 0)
                    {
                        ReturnParticle(trailPoints[i]);
                        trailPoints.RemoveAt(i);
                    }
                }

                yield return null;
            }

            // Cleanup remaining
            foreach (var point in trailPoints)
            {
                ReturnParticle(point);
            }
        }

        // === SCREEN EFFECTS ===

        public void FlashScreen(Color color, float duration = 0.2f)
        {
            if (_screenFlash != null)
            {
                StartCoroutine(ScreenFlashRoutine(color, duration));
            }
        }

        private IEnumerator ScreenFlashRoutine(Color color, float duration)
        {
            _screenFlash.SetActive(true);
            _flashImage.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
                _flashImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            _screenFlash.SetActive(false);
        }

        public void DamageVignette(float intensity = 0.5f)
        {
            StartCoroutine(VignetteRoutine(new Color(0.5f, 0f, 0f, intensity), 0.3f));
        }

        public void HealVignette(float intensity = 0.3f)
        {
            StartCoroutine(VignetteRoutine(new Color(0f, 0.5f, 0.2f, intensity), 0.3f));
        }

        private IEnumerator VignetteRoutine(Color color, float duration)
        {
            if (_vignetteOverlay == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = color.a * (1f - t);
                _vignetteImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            _vignetteImage.color = new Color(0, 0, 0, 0);
        }

        // === CAMERA EFFECTS ===

        public void ScreenShake(float duration = 0.2f, float magnitude = 0.1f)
        {
            if (_mainCamera != null)
            {
                StartCoroutine(ScreenShakeRoutine(duration, magnitude));
            }
        }

        private IEnumerator ScreenShakeRoutine(float duration, float magnitude)
        {
            Vector3 originalPos = _originalCameraPos;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float damping = 1f - (elapsed / duration);

                float x = UnityEngine.Random.Range(-1f, 1f) * magnitude * damping;
                float y = UnityEngine.Random.Range(-1f, 1f) * magnitude * damping;

                _mainCamera.transform.position = originalPos + new Vector3(x, y, 0);

                yield return null;
            }

            _mainCamera.transform.position = originalPos;
        }

        public void CameraZoom(float targetSize, float duration = 0.3f)
        {
            if (_mainCamera != null && _mainCamera.orthographic)
            {
                StartCoroutine(CameraZoomRoutine(targetSize, duration));
            }
        }

        private IEnumerator CameraZoomRoutine(float targetSize, float duration)
        {
            float startSize = _mainCamera.orthographicSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Ease out
                t = 1f - (1f - t) * (1f - t);
                _mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                yield return null;
            }

            _mainCamera.orthographicSize = targetSize;
        }

        public void CameraZoomPulse(float pulseAmount = 0.3f, float duration = 0.15f)
        {
            if (_mainCamera != null && _mainCamera.orthographic)
            {
                StartCoroutine(CameraZoomPulseRoutine(pulseAmount, duration));
            }
        }

        private IEnumerator CameraZoomPulseRoutine(float pulseAmount, float duration)
        {
            float originalSize = _mainCamera.orthographicSize;
            float targetSize = originalSize - pulseAmount;

            // Zoom in
            float elapsed = 0f;
            float halfDuration = duration / 2f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                _mainCamera.orthographicSize = Mathf.Lerp(originalSize, targetSize, t);
                yield return null;
            }

            // Zoom out
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                _mainCamera.orthographicSize = Mathf.Lerp(targetSize, originalSize, t);
                yield return null;
            }

            _mainCamera.orthographicSize = originalSize;
        }

        // === FLOATING TEXT ===

        public void ShowDamageNumber(Vector3 worldPos, int damage, bool isCritical = false)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null || Camera.main == null) return;

            StartCoroutine(DamageNumberRoutine(worldPos, damage, isCritical, canvas));
        }

        private IEnumerator DamageNumberRoutine(Vector3 worldPos, int damage, bool isCritical, Canvas canvas)
        {
            GameObject textObj = GetFloatText();
            textObj.transform.SetParent(canvas.transform, false);

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = isCritical ? $"CRIT!\n{damage}" : damage.ToString();
            tmp.fontSize = isCritical ? 32 : 24;
            tmp.color = isCritical ? Color.yellow : Color.white;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 80);

            float duration = 0.8f;
            float elapsed = 0f;
            
            Vector2 startPos = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 randomOffset = new Vector2(UnityEngine.Random.Range(-30f, 30f), 0);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Arc motion
                float yOffset = Mathf.Sin(t * Mathf.PI) * 50f + t * 30f;
                rect.position = startPos + randomOffset + Vector2.up * yOffset;

                // Scale pop
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * (isCritical ? 0.5f : 0.2f);
                rect.localScale = Vector3.one * scale;

                // Fade
                Color c = tmp.color;
                tmp.color = new Color(c.r, c.g, c.b, 1f - (t * t));

                yield return null;
            }

            ReturnFloatText(textObj);
        }

        public void ShowFloatingText(Vector3 worldPos, string text, Color color, float size = 28f)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            StartCoroutine(FloatingTextRoutine(worldPos, text, color, size, canvas));
        }

        private IEnumerator FloatingTextRoutine(Vector3 worldPos, string text, Color color, float size, Canvas canvas)
        {
            GameObject textObj = GetFloatText();
            textObj.transform.SetParent(canvas.transform, false);

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 80);

            float duration = 1.5f;
            float elapsed = 0f;

            Vector2 startPos = Camera.main != null 
                ? (Vector2)Camera.main.WorldToScreenPoint(worldPos) 
                : new Vector2(Screen.width / 2f, Screen.height / 2f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                rect.position = startPos + Vector2.up * (t * 100f);
                rect.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.3f);

                Color c = tmp.color;
                tmp.color = new Color(c.r, c.g, c.b, 1f - t);

                yield return null;
            }

            ReturnFloatText(textObj);
        }

        public void ShowCenterText(string text, Color color, float size = 48f, float duration = 1.5f)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            StartCoroutine(CenterTextRoutine(text, color, size, duration, canvas));
        }

        private IEnumerator CenterTextRoutine(string text, Color color, float size, float duration, Canvas canvas)
        {
            GameObject textObj = GetFloatText();
            textObj.transform.SetParent(canvas.transform, false);

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 100);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Scale animation
                float scale = 1f;
                if (t < 0.1f)
                    scale = Mathf.Lerp(0f, 1.2f, t / 0.1f);
                else if (t < 0.2f)
                    scale = Mathf.Lerp(1.2f, 1f, (t - 0.1f) / 0.1f);
                else
                    scale = 1f;

                rect.localScale = Vector3.one * scale;

                // Fade out in last 30%
                if (t > 0.7f)
                {
                    float fadeT = (t - 0.7f) / 0.3f;
                    tmp.color = new Color(color.r, color.g, color.b, 1f - fadeT);
                }

                yield return null;
            }

            ReturnFloatText(textObj);
        }

        // === OBJECT POOLING ===

        private GameObject GetParticle()
        {
            GameObject particle;
            if (_particlePool.Count > 0)
            {
                particle = _particlePool.Dequeue();
            }
            else
            {
                particle = CreatePooledParticle();
            }
            particle.SetActive(true);
            return particle;
        }

        private void ReturnParticle(GameObject particle)
        {
            particle.SetActive(false);
            particle.transform.SetParent(transform);
            _particlePool.Enqueue(particle);
        }

        private GameObject GetFloatText()
        {
            GameObject text;
            if (_floatTextPool.Count > 0)
            {
                text = _floatTextPool.Dequeue();
            }
            else
            {
                text = CreatePooledFloatText();
            }
            text.SetActive(true);
            return text;
        }

        private void ReturnFloatText(GameObject text)
        {
            text.SetActive(false);
            text.transform.SetParent(transform);
            _floatTextPool.Enqueue(text);
        }

        // === QUICK ACCESS EFFECTS ===

        public void OnEnemyDeath(Vector3 position, bool isBoss = false, bool isElite = false)
        {
            Color color = Color.red;
            float size = 1f;

            if (isBoss)
            {
                color = new Color(1f, 0.3f, 0.8f);
                size = 2f;
                ScreenShake(0.3f, 0.15f);
                FlashScreen(new Color(1f, 0.3f, 0.8f, 0.3f));
                CameraZoomPulse(0.2f, 0.2f);
            }
            else if (isElite)
            {
                color = new Color(1f, 0.8f, 0.2f);
                size = 1.5f;
                ScreenShake(0.1f, 0.05f);
            }

            SpawnExplosion(position, color, size);
            SpawnCoinBurst(position, isBoss ? 10 : (isElite ? 5 : 3));
        }

        public void OnMerge(Vector3 position, int newTier)
        {
            Color tierColor = Testing.SpriteGenerator.DefenderColors[Mathf.Min(newTier - 1, 4)];
            SpawnExplosion(position, tierColor, 0.8f);
            SpawnSparkle(position + Vector3.up * 0.3f, Color.white);
            SpawnSparkle(position + Vector3.left * 0.3f, tierColor);
            SpawnSparkle(position + Vector3.right * 0.3f, tierColor);
            CameraZoomPulse(0.1f, 0.1f);
        }

        public void OnPowerUpCollect(Vector3 position, Color color)
        {
            SpawnSparkle(position, color);
            FlashScreen(new Color(color.r, color.g, color.b, 0.2f), 0.15f);
        }

        public void OnUltimate()
        {
            FlashScreen(new Color(0.8f, 0.4f, 1f, 0.5f), 0.4f);
            ScreenShake(0.4f, 0.2f);
            CameraZoomPulse(0.3f, 0.2f);
        }

        public void OnFeverMode()
        {
            FlashScreen(new Color(1f, 0.5f, 0f, 0.3f), 0.3f);
            CameraZoomPulse(0.15f, 0.15f);
        }

        public void OnWaveComplete(int waveNumber)
        {
            ShowCenterText($"WAVE {waveNumber} COMPLETE!", new Color(0.3f, 1f, 0.4f), 42f, 2f);
            CameraZoomPulse(0.2f, 0.3f);
        }

        public void OnKillStreak(string streakName)
        {
            Color streakColor = new Color(1f, 0.9f, 0.2f);
            ShowCenterText(streakName, streakColor, 52f, 2f);
            ScreenShake(0.15f, 0.08f);
            CameraZoomPulse(0.15f, 0.15f);
        }
    }
}
