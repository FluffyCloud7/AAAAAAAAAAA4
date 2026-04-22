using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartUI : MonoBehaviour
{
    public Image[] hearts;

    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    public PlayerHealth playerHealth;

    private Coroutine shakeCoroutine;
    private Coroutine idleCoroutine;
    private Coroutine healCoroutine;

    [Header("Shake")]
    public float weakShake = 1.5f;
    public float strongShake = 3f;

    [Header("Idle")]
    public float idleMinDelay = 25f;
    public float idleMaxDelay = 40f;

    [Header("Heal")]
    public float healStepDelay = 0.08f;

    private int lastHealth;

    private Coroutine lowHpShakeCoroutine;

    private void Start()
    {
        playerHealth.OnHealthChanged += OnHealthChanged;

        lastHealth = playerHealth.CurrentHealth;

        idleCoroutine = StartCoroutine(IdleAnimation());
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    // =========================
    // MAIN UPDATE
    // =========================
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth < lastHealth)
        {
            StartCoroutine(DamageSequence(lastHealth, currentHealth));
        }
        else if (currentHealth > lastHealth)
        {
            StartCoroutine(HealSequence(lastHealth, currentHealth));
        }

        lastHealth = currentHealth;
        HandleLowHpShake(currentHealth);
    }

    private void HandleLowHpShake(int health)
    {
        if (lowHpShakeCoroutine != null)
        {
            StopCoroutine(lowHpShakeCoroutine);
            lowHpShakeCoroutine = null;
        }

        int heartsCount = Mathf.CeilToInt(health / 2f);

        // 0 = нет HP (не трясём)
        if (heartsCount <= 0) return;

        int index = heartsCount - 1;
        index = Mathf.Clamp(index, 0, hearts.Length - 1);

        float intensity = 0f;

        if (health == 2) intensity = weakShake;
        else if (health == 1) intensity = strongShake;

        if (intensity > 0)
            lowHpShakeCoroutine = StartCoroutine(ShakeHeart(hearts[index].rectTransform, intensity));
    }

    private IEnumerator ShakeHeart(RectTransform rect, float intensity)
    {
        Vector3 baseRot = rect.localEulerAngles;

        while (true)
        {
            float angle = Mathf.Sin(Time.time * 18f) * intensity;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
    }

    // =========================
    // HEART RENDER
    // =========================
    private void UpdateHeartsInstant(int health)
    {
        int h = health;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (h >= 2)
            {
                hearts[i].sprite = fullHeart;
                h -= 2;
            }
            else if (h == 1)
            {
                hearts[i].sprite = halfHeart;
                h -= 1;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }

    // =========================
    // DAMAGE FX (ИСПРАВЛЕНО)
    // =========================
    private void HandleDamage(int oldHealth, int newHealth)
    {
        // старое и новое состояние по половинкам (0..)
        int oldHalfIndex = oldHealth - 1;
        int newHalfIndex = newHealth - 1;

        // какое сердце затронуто
        int heartIndex = oldHalfIndex / 2;

        heartIndex = Mathf.Clamp(heartIndex, 0, hearts.Length - 1);

        if (hearts[heartIndex] != null)
            StartCoroutine(DamagePunch(hearts[heartIndex].rectTransform));
    }

    private IEnumerator DamagePunch(RectTransform rect)
    {
        Vector3 baseScale = Vector3.one;
        Vector3 squash = new Vector3(1.3f, 0.7f, 1f);
        Vector3 stretch = new Vector3(0.85f, 1.2f, 1f);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / 0.05f;
            rect.localScale = Vector3.Lerp(baseScale, squash, t);
            yield return null;
        }

        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / 0.05f;
            rect.localScale = Vector3.Lerp(squash, stretch, t);
            yield return null;
        }

        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / 0.08f;
            rect.localScale = Vector3.Lerp(stretch, baseScale, t);
            yield return null;
        }

        rect.localScale = baseScale;
    }

    // =========================
    // IDLE (ОЧЕНЬ РЕДКИЙ)
    // =========================
    private IEnumerator IdleAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(idleMinDelay, idleMaxDelay));

            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] != null)
                    StartCoroutine(PulseHeart(hearts[i].rectTransform));

                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private IEnumerator PulseHeart(RectTransform rect)
    {
        Vector3 baseScale = Vector3.one;
        Vector3 big = baseScale * 1.12f;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / 0.12f;
            rect.localScale = Vector3.Lerp(baseScale, big, t);
            yield return null;
        }

        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / 0.12f;
            rect.localScale = Vector3.Lerp(big, baseScale, t);
            yield return null;
        }

        rect.localScale = baseScale;
    }

    // =========================
    // HEAL
    // =========================
    private IEnumerator HealAnimation(int from, int to)
    {
        for (int hp = from + 1; hp <= to; hp++)
        {
            UpdateHeartsInstant(hp);

            int index = (hp - 1) / 2;
            index = Mathf.Clamp(index, 0, hearts.Length - 1);

            if (hearts[index] != null)
                yield return StartCoroutine(PulseHeart(hearts[index].rectTransform));

            yield return new WaitForSeconds(healStepDelay);
        }
    }
    private IEnumerator DamageSequence(int from, int to)
    {
        HandleDamage(from, to);

        UpdateHeartsInstant(to);

        yield return null;
    }
    private IEnumerator HealSequence(int from, int to)
    {
        for (int hp = from + 1; hp <= to; hp++)
        {
            UpdateHeartsInstant(hp);

            int index = (hp - 1) / 2;
            index = Mathf.Clamp(index, 0, hearts.Length - 1);

            if (hearts[index] != null)
                yield return StartCoroutine(PulseHeart(hearts[index].rectTransform));

            yield return new WaitForSeconds(healStepDelay);
        }
        HandleLowHpShake(to);
    }
}