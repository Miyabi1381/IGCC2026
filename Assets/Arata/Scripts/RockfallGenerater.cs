using UnityEngine;

public class RockfallGenerater : MonoBehaviour
{
    [SerializeField] private GameObject rockPrefab; // The rock prefab 岩のプレハブ
    [SerializeField] private float spawnInterval = 0.8f; // Interval between rock spawns 岩の生成間隔
    [SerializeField] private float width = 5f; // Width of the spawn area 生成エリアの幅

    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem warningParticle; // particle system for warning 岩の落下警告用パーティクルシステム

    void Start()
    {
        // ゲーム開始時にも同期させる
        SyncParticleRadius();

        // パーティクルが割り当てられていれば再生を開始する
        if (warningParticle != null)
        {
            warningParticle.Play();
        }
    }

    void Update()
    {
        // 一定の間隔で岩を生成する
        if (Time.time % spawnInterval < Time.deltaTime)
        {
            SpawnRock();
        }
    }

    // スポーン位置をランダムに決定して岩を生成するメソッド
    // This method randomly determines a spawn position and instantiates a rock
    private void SpawnRock()
    {
        // オブジェクトの現在のX座標を基準に、指定された幅の範囲内でスポーン位置をランダムに決定する
        float randomX = transform.position.x + Random.Range(-width / 2, width / 2);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, transform.position.z);

        // 計算された位置に岩のプレハブをインスタンス化する
        Instantiate(rockPrefab, spawnPosition, Quaternion.identity);

    }

    // インスペクターの数値を変更した瞬間に自動で呼ばれるメソッド
    private void OnValidate()
    {
        SyncParticleRadius();
    }

    // パーティクルのRadiusを横幅に同期させる処理
    // Process to synchronise the particle’s radius with its width
    private void SyncParticleRadius()
    {
        if (warningParticle != null)
        {
            // パーティクルのモジュールを変数に入れる
            var shape = warningParticle.shape;

            // Radiusは、全体の横幅の半分にする
            shape.radius = width / 2f;
        }
    }

    private void OnDrawGizmos()
    {
        // オブジェクトのX座標を基準に左端と右端のX座標を計算
        float leftX = transform.position.x - width / 2;
        float rightX = transform.position.x + width / 2;

        // スポーンエリアを視覚化するためにGizmosを描画する
        Gizmos.color = Color.red;

        // 左端の縦線
        Gizmos.DrawLine(new Vector3(leftX, transform.position.y, transform.position.z), new Vector3(leftX, transform.position.y - 1, transform.position.z));
        // 右端の縦線 
        Gizmos.DrawLine(new Vector3(rightX, transform.position.y, transform.position.z), new Vector3(rightX, transform.position.y - 1, transform.position.z));
        // 範囲を示す横線
        Gizmos.DrawLine(new Vector3(leftX, transform.position.y, transform.position.z), new Vector3(rightX, transform.position.y, transform.position.z));
    }
}