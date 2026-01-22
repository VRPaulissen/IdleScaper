using System.Collections.Generic;
using DG.Tweening;
using Player;
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
        private const string NORMAL_SUFFIX = "";
        private const string CRIT_SUFFIX = "!";
        private const string ULTRA_SUFFIX = "!!";

        [Header("Prefab & Parent")]
        [SerializeField] private TMP_Text damageTextPrefab;
        [SerializeField] private RectTransform spawnParentOverride;
        [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 40f);

        [Header("Colors")]
        [SerializeField] private Color critColor = new Color(1f, 0.55f, 0f, 1f);     
        [SerializeField] private Color ultraCritColor = new Color(0.65f, 0.25f, 1f, 1f); 

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
        private Color normalColor;

        private RectTransform spawnParent => spawnParentOverride != null
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

            normalColor = damageTextPrefab.color;

            Prewarm();
  
            node.DamageApplied += HandleDamageApplied;
            node.DepletionStateChanged += HandleDepletionStateChanged;
        }

        private void OnDisable()
        {
            DOTween.Kill(spawnParent, complete: false);
        }

        private void OnDestroy()
        {
            node.DamageApplied -= HandleDamageApplied;
            node.DepletionStateChanged -= HandleDepletionStateChanged;

            DOTween.Kill(spawnParent, complete: false);
        }

        private void HandleDepletionStateChanged(ResourceNode resourceNode, bool isDepleted)
        {
            // No baseline logic needed anymore; damage is driven by the roll.
            // Still useful to ignore events while depleted.
        }

        private void HandleDamageApplied(ResourceNode resourceNode, GatheringDamageRoll roll)
        {
            var text = Rent();
            text.gameObject.SetActive(true);

            text.DOKill(complete: false);

            text.alpha = 1f;
            text.color = GetColor(roll.HitType);

            var suffix = GetSuffix(roll.HitType);
            if (suffix.Length == 0)
                text.SetText(roll.FinalDamage.ToString());
            else
                text.SetText($"{roll.FinalDamage}{suffix}");

            var rect = text.rectTransform;
            rect.SetParent(spawnParent, worldPositionStays: false);

            var startPos = spawnOffset;
            rect.anchoredPosition = startPos;

            var driftX = Random.Range(-horizontalDrift, horizontalDrift);
            var endPos = startPos + new Vector2(driftX, riseDistance);

            var sequence = DOTween.Sequence()
                .SetTarget(spawnParent)
                .SetUpdate(isIndependentUpdate: false);

            sequence.Join(rect.DOAnchorPos(endPos, lifetimeSeconds).SetEase(moveEase));
            sequence.Join(text.DOFade(0f, lifetimeSeconds).SetEase(fadeEase));

            sequence.OnComplete(() =>
            {
                if (text == null)
                    return;

                text.DOKill(complete: false);
                Return(text);
            });

            sequence.Play();
        }

        private Color GetColor(GatheringHitType hitType)
        {
            switch (hitType)
            {
                case GatheringHitType.Crit:
                    return critColor;

                case GatheringHitType.UltraCrit:
                    return ultraCritColor;

                default:
                    return normalColor;
            }
        }

        private static string GetSuffix(GatheringHitType hitType)
        {
            switch (hitType)
            {
                case GatheringHitType.Crit:
                    return CRIT_SUFFIX;

                case GatheringHitType.UltraCrit:
                    return ULTRA_SUFFIX;

                default:
                    return NORMAL_SUFFIX;
            }
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
            var instance = Instantiate(damageTextPrefab, spawnParent, worldPositionStays: false);
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}
