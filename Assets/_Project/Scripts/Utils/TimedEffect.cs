using UnityEngine;

public class TimedEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;
    private float timer;

    // Bật hiệu ứng với đường kính cho trước
    public void Play(float diameter)
    {
        transform.localScale = new Vector3(diameter, diameter, 1f);
        timer = duration;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) PoolManager.Instance.Release(gameObject);
    }
}