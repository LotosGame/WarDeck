using System.Collections;
using UnityEngine;

public class ArrowStormEffect : MonoBehaviour
{
    public static void Spawn(Vector3 targetWorldPos, System.Action onImpact = null)
    {
        GameObject effectObj = new GameObject("ArrowStormEffect");
        effectObj.transform.position = targetWorldPos;
        ArrowStormEffect effect = effectObj.AddComponent<ArrowStormEffect>();
        effect.StartCoroutine(effect.PlayEffect(targetWorldPos, onImpact));
    }

    private IEnumerator PlayEffect(Vector3 targetPos, System.Action onImpact)
    {
        // Создаем падающие стрелы (3-5 лучей/стрел)
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(1.8f, 2.5f), 0);
            Vector3 startPos = targetPos + offset;
            Vector3 endPos = targetPos + new Vector3(offset.x * 0.4f, Random.Range(-0.2f, 0.2f), 0);

            GameObject arrow = new GameObject($"Arrow_{i}");
            arrow.transform.position = startPos;

            LineRenderer lr = arrow.AddComponent<LineRenderer>();
            lr.startWidth = 0.04f;
            lr.endWidth = 0.015f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.95f, 0.85f, 0.4f, 1f); // Золотисто-желтые стрелы
            lr.endColor = new Color(0.7f, 0.2f, 0.1f, 0.8f);
            lr.sortingLayerName = "Units";
            lr.sortingOrder = 30;

            lr.SetPosition(0, startPos);
            lr.SetPosition(1, startPos - new Vector3(0.1f, 0.35f, 0));

            StartCoroutine(AnimateArrow(arrow, lr, startPos, endPos));
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.2f);

        // Вспышка попадания на клетке
        GameObject impactObj = new GameObject("ImpactCircle");
        impactObj.transform.position = targetPos;
        SpriteRenderer sr = impactObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.3f, 0.1f, 0.7f);
        sr.sortingLayerName = "Units";
        sr.sortingOrder = 28;
        impactObj.transform.localScale = Vector3.zero;

        onImpact?.Invoke();

        float timer = 0f;
        while (timer < 0.35f)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1.2f, timer / 0.35f);
            impactObj.transform.localScale = new Vector3(scale, scale * 0.6f, 1f);
            sr.color = Color.Lerp(new Color(1f, 0.3f, 0.1f, 0.8f), new Color(1f, 0.9f, 0.2f, 0f), timer / 0.35f);
            yield return null;
        }

        Destroy(impactObj);
        Destroy(gameObject);
    }

    private IEnumerator AnimateArrow(GameObject arrowObj, LineRenderer lr, Vector3 start, Vector3 target)
    {
        float t = 0f;
        float duration = 0.2f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = t / duration;
            Vector3 curr = Vector3.Lerp(start, target, pct);
            Vector3 tail = curr + (start - target).normalized * 0.35f;

            if (lr != null)
            {
                lr.SetPosition(0, tail);
                lr.SetPosition(1, curr);
            }
            yield return null;
        }

        Destroy(arrowObj);
    }

    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float alpha = Mathf.SmoothStep(1f, 0f, dist / radius);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
    }
}
