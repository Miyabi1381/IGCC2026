using UnityEngine;

public class Rockfall : MonoBehaviour
{
    [Header("エフェクト設定")]
    [SerializeField] private GameObject breakEffectPrefab; // 壊れる際のエフェクトプレハブ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // プレハブが設定されていれば、岩の現在位置にエフェクトを生成する
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 岩のオブジェクトを破壊する
        Destroy(gameObject);
    }
}