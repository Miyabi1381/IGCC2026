using UnityEngine;

public class BoulderSpawner : MonoBehaviour
{
    [Header("Spawn Settings スポーン設定")]
    [SerializeField] private GameObject boulderPrefab; // The Prefab with the Rolling Rock 転がす岩のプレハブ
    [SerializeField] private float spawnInterval = 3f; // Interval between rocks appearing(s) 岩を出す間隔(秒)

    private float timer = 0f;

    void Update()
    {
        // 毎フレームの経過時間を足していく
        timer += Time.deltaTime;

        // タイマーが設定した間隔を超えたら
        if (timer >= spawnInterval)
        {
            SpawnBoulder();
            timer = 0f; // タイマーをリセットして次のカウントへ
        }
    }

    // 岩を生成するメソッド
    // This method spawns a boulder at the spawner's position
    void SpawnBoulder()
    {
        if (boulderPrefab != null)
        {
            // このオブジェクトと同じ位置に岩を生成する
            Instantiate(boulderPrefab, transform.position, Quaternion.identity);
        }
    }
}