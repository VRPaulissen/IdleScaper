using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Logger = Utilities.Logging.Logger;

namespace Resource.Runtime
{
    /// <summary>
    /// Spawns floating damage text in UI space for resource hits, animated via DOTween.
    /// Text is positioned relative to a <see cref="RectTransform"/> parent and returned to a pool after playback.
    /// </summary>
    public sealed class DamageView : MonoBehaviour
    {
        private const float DEFAULT_LIFETIME_SECONDS = 2f;

        [Header("Prefab & Parent")]
        [SerializeField] private TMP_Text damageTextPrefab;
        [SerializeField] private RectTransform spawnParentOverride;
        [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 40f);

        [Header("Animation")]
        [SerializeField, Min(0.1f)] private float lifetimeSeconds = DEFAULT_LIFETIME_SECONDS;
        [SerializeField, Min(0f)] private float riseDistance = 60f;
        [SerializeField, Min(0f)] private float horizontalDrift = 30f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int prewarmCount = 8;
        [SerializeField, Min(1)] private int maxPoolSize = 32;

        private readonly Queue<TMP_Text> pool = new Queue<TMP_Text>(32);

        private ResourceNode node;
        private int lastDurability;
        private bool hasBaseline;

        private RectTransform SpawnParent => spawnParentOverride != null
            ? spawnParentOverride
            : (RectTransform)transform;

        private void Awake()
        {
            node = GetComponentInParent<ResourceNode>();

            if (node == null)
            {
                Logger.LogError($"{nameof(DamageView)} on {name} could not find {nameof(ResourceNode)}.", this);
                enabled = false;
                return;
            }

            if (damageTextPrefab == null)
            {
                Logger.LogError($"{nameof(DamageView)} on {name} is missing {nameof(damageTextPrefab)}.", this);
                enabled = false;
                return;
            }

            Prewarm();
        }

        private void OnEnable()
        {
            node.DurabilityChanged += HandleDurabilityChanged;
            node.DepletionStateChanged += HandleDepletionStateChanged;

            hasBaseline = false;
        }

        private void OnDisable()
        {
            node.DurabilityChanged -= HandleDurabilityChanged;
            node.DepletionStateChanged -= HandleDepletionStateChanged;

            hasBaseline = false;

            // Kill any running tweens that target children under this view (safe if used as parent).
            DOTween.Kill(SpawnParent, complete: false);
        }

        private void HandleDepletionStateChanged(ResourceNode resourceNode, bool isDepleted)
        {
            if (isDepleted)
            {
                hasBaseline = false;
                return;
            }

            lastDurability = resourceNode.DurabilityCurrent;
            hasBaseline = true;
        }

        private void HandleDurabilityChanged(ResourceNode resourceNode, int current, int max)
        {
            if (!resourceNode.isActiveAndEnabled || resourceNode.IsDepleted)
                return;

            if (!hasBaseline)
            {
                lastDurability = current;
                hasBaseline = true;
                return;
            }

            var damage = Mathf.Max(0, lastDurability - current);
            lastDurability = current;

            if (damage <= 0)
                return;

            SpawnDamageText(damage);
        }

        private void SpawnDamageText(int damage)
        {
            var text = Rent();
            text.gameObject.SetActive(true);

            // Ensure no leftover tweens from previous use.
            text.DOKill(complete: false);

            text.SetText(damage.ToString());
            text.alpha = 1f;

            var rect = text.rectTransform;
            rect.SetParent(SpawnParent, worldPositionStays: false);

            var startPos = spawnOffset;
            rect.anchoredPosition = startPos;

            var driftX = Random.Range(-horizontalDrift, horizontalDrift);
            var endPos = startPos + new Vector2(driftX, riseDistance);

            // Use a single sequence for deterministic cleanup and pooling.
            // SetTarget allows killing all spawned sequences by killing SpawnParent target if desired.
            var sequence = DOTween.Sequence()
                .SetTarget(SpawnParent)
                .SetUpdate(isIndependentUpdate: false);

            sequence.Join(rect.DOAnchorPos(endPos, lifetimeSeconds).SetEase(moveEase));
            sequence.Join(text.DOFade(0f, lifetimeSeconds).SetEase(fadeEase));

            sequence.OnComplete(() =>
            {
                if (text == null)
                    return;

                // Defensive: kill any tweens that might still exist.
                text.DOKill(complete: false);
                Return(text);
            });

            sequence.Play();
        }

        private void Prewarm()
        {
            var count = Mathf.Clamp(prewarmCount, 0, maxPoolSize);
            for (var i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                Return(instance);
            }
        }

        private TMP_Text Rent()
        {
            return pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        }

        private void Return(TMP_Text text)
        {
            if (text == null)
                return;

            if (pool.Count >= maxPoolSize)
            {
                Destroy(text.gameObject);
                return;
            }

            text.gameObject.SetActive(false);
            pool.Enqueue(text);
        }

        private TMP_Text CreateInstance()
        {
            var instance = Instantiate(damageTextPrefab, SpawnParent, worldPositionStays: false);
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}
