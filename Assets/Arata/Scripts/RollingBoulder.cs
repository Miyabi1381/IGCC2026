using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RollingBoulder : MonoBehaviour
{
    [Header("Mobility Settings 移動設定")]
    [SerializeField] private float acceleration = 20f; // The force applied to the boulder to make it roll 岩を転がすために加える力
    [SerializeField] private float maxSpeed = 10f;  // The maximum speed the boulder can reach 岩が到達できる最大速度
    [SerializeField] private bool moveRight = true; // Direction of movement: true for right, false for left 移動方向 右true 左false

    [Header("Target Settings 破壊対象の設定")]
    [SerializeField] private string targetTag = "Player"; // The tag of the objects that the boulder can destroy 岩が破壊できるオブジェクトのタグ
    [SerializeField] private LayerMask targetLayer; // The layer of the objects that the boulder can destroy 岩が破壊できるオブジェクトのレイヤー

    [Header("Exclusion Settings 除外設定")]
    [SerializeField] private string aliveScriptName = "PlayerController";// The name of the script that, if attached to the target, will prevent destruction ぶつかった相手に付いている場合、破壊を防ぐスクリプトの名前

    [Header("Lifetime Settings 生存時間設定")]
    [SerializeField] private float lifetime = 8f; // Time in seconds before the boulder is automatically destroyed 生成されてから消えるまでの秒数

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        Vector2 direction = moveRight ? Vector2.right : Vector2.left;
        rb.AddForce(direction * acceleration);

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            float clampedX = Mathf.Sign(rb.linearVelocity.x) * maxSpeed;
            rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 子オブジェクトに当たった場合でも、Rigidbodyを持つ親玉を取得する
        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        bool matchTag = !string.IsNullOrEmpty(targetTag) && hitObject.CompareTag(targetTag);
        bool matchLayer = ((1 << hitObject.layer) & targetLayer) != 0;

        if (matchTag || matchLayer)
        {
            if (!string.IsNullOrEmpty(aliveScriptName))
            {
                // 親オブジェクトを遡って生存確認用スクリプトを探す
                MonoBehaviour targetScript = FindAliveScript(hitObject.transform);

                // そのスクリプトが付いていて、かつ有効状態なら
                if (targetScript != null && targetScript.enabled)
                {
                    // プレイヤーへの破壊処理は行わず、大岩自身だけを破壊してストップ
                    Destroy(gameObject);
                    return;
                }
            }

            // スクリプトが無効かスクリプトが付いていない場合は相手を破壊する
            Destroy(hitObject);

            // 死体を巻き込んだ後、大岩自身も破壊する
            Destroy(gameObject);
        }
    }

    // 子から親へ向かって順番にスクリプトを探す
    // Search for the script from child to parent
    private MonoBehaviour FindAliveScript(Transform currentTransform)
    {
        while (currentTransform != null)
        {
            MonoBehaviour script = currentTransform.GetComponent(aliveScriptName) as MonoBehaviour;
            if (script != null)
            {
                return script;
            }
            currentTransform = currentTransform.parent;
        }
        return null;
    }
}