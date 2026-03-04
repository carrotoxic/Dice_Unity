using System.Collections;
using UnityEngine;

public class DieController : MonoBehaviour
{
    [Header("Rotation when each value is on TOP (Euler angles)")]
    public Vector3 face1TopEuler;
    public Vector3 face2TopEuler;
    public Vector3 face3TopEuler;
    public Vector3 face4TopEuler;
    public Vector3 face5TopEuler;
    public Vector3 face6TopEuler;

    [Header("Runtime")]
    [SerializeField] private int currentValue = 1;

    public int CurrentValue => currentValue;

    private Coroutine rotateCo;

    public int RandomizeFaceSmooth(float duration = 0.18f)
    {
        int v = Random.Range(1, 7);
        SetFaceSmooth(v, duration);
        return v;
    }

    public void SetFaceSmooth(int value, float duration = 0.18f)
    {
        currentValue = Mathf.Clamp(value, 1, 6);
        Quaternion target = Quaternion.Euler(GetEuler(currentValue));

        if (rotateCo != null) StopCoroutine(rotateCo);
        rotateCo = StartCoroutine(RotateTo(target, duration));
    }

    public void SetFaceInstant(int value)
    {
        currentValue = Mathf.Clamp(value, 1, 6);
        transform.rotation = Quaternion.Euler(GetEuler(currentValue));
    }

    private Vector3 GetEuler(int v)
    {
        return v switch
        {
            1 => face1TopEuler,
            2 => face2TopEuler,
            3 => face3TopEuler,
            4 => face4TopEuler,
            5 => face5TopEuler,
            _ => face6TopEuler,
        };
    }

    private IEnumerator RotateTo(Quaternion target, float duration)
    {
        Quaternion from = transform.rotation;

        if (duration <= 0.0001f)
        {
            transform.rotation = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // ✅ 子弹时间也按真实时间平滑
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u); // smoothstep
            transform.rotation = Quaternion.Slerp(from, target, s);
            yield return null;
        }

        transform.rotation = target;
        rotateCo = null;
    }
}