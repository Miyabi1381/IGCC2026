using UnityEngine;

public class Easing : MonoBehaviour
{
    private static float multiplier = -5f;
    public static float OutExpo(float time)
    {
        time = Mathf.Clamp01(time);
        return (time == 1f) ? 1f : 1f - Mathf.Pow(2f, multiplier * time);
    }
    public static float OutQuint(float time)
    {
        return 1 - Mathf.Pow(1 - time, 5);
    }
    public static float InOutExpo(float time)
    {
        return (time == 0f) ? 0f : (time == 1f) ? 1f : time < 0.5 ? Mathf.Pow(2, 20 * time - 10) / 2 : (2 - Mathf.Pow(2, -20 * time + 10) / 2);
    }
}
