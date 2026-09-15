using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(Rigidbody2D))]
public class RollingBoulder : MonoBehaviour
{
    [Header("Mobility Settings 移動設定")]
    [SerializeField] private float acceleration = 20f; // 岩を転がすために加える力
    [SerializeField] private float maxSpeed = 10f;  // 岩が到達できる最大速度
    [SerializeField] private bool moveRight = true; // 移動方向 右true 左false

    [Header("Target Settings 破壊対象の設定")]
    [SerializeField] private string targetTag = "Player"; // 岩が破壊できるオブジェクトのタグ
    [SerializeField] private LayerMask targetLayer; // 岩が破壊できるオブジェクトのレイヤー

    [Header("Exclusion Settings 除外設定")]
    [SerializeField] private string aliveScriptName = "PlayerController";// ぶつかった相手に付いている場合、破壊を防ぐスクリプトの名前

    [Header("Lifetime Settings 生存時間設定")]
    [SerializeField] private float lifetime = 8f; // 生成されてから消えるまでの秒数

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 子オブジェクトに当たった場合でも、Rigidbodyを持つ親玉を取得する
        GameObject hitObject = collision.collider.attachedRigidbody != null ? collision.collider.attachedRigidbody.gameObject : collision.gameObject;

        bool matchTag = !string.IsNullOrEmpty(targetTag) && hitObject.CompareTag(targetTag);
        bool matchLayer = ((1 << hitObject.layer) & targetLayer) != 0;

        if (matchTag || matchLayer)
        {
            bool isAlivePlayer = false;
            bool isCorpse = false;

            // オブジェクトに付いているすべてのスクリプトを取得
            MonoBehaviour[] allScripts = hitObject.transform.root.GetComponentsInChildren<MonoBehaviour>(true);
            bool foundDeadFlag = false;

            // 変数がどこかのスクリプトにないか全力で探す
            foreach (var script in allScripts)
            {
                if (script == null) continue;

                // 変数として定義されているかチェック
                FieldInfo deadField = script.GetType().GetField("isDead", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (deadField != null)
                {
                    bool isDead = (bool)deadField.GetValue(script);
                    if (isDead) isCorpse = true; else isAlivePlayer = true;
                    foundDeadFlag = true;
                    break;
                }

                // プロパティとして定義されているかチェック
                PropertyInfo deadProp = script.GetType().GetProperty("isDead", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (deadProp != null)
                {
                    bool isDead = (bool)deadProp.GetValue(script);
                    if (isDead) isCorpse = true; else isAlivePlayer = true;
                    foundDeadFlag = true;
                    break;
                }
            }

            // isDeadが見つからなかった場合
            if (!foundDeadFlag && !string.IsNullOrEmpty(aliveScriptName))
            {
                foreach (var script in allScripts)
                {
                    if (script != null && script.GetType().Name == aliveScriptName)
                    {
                        if (script.enabled) isAlivePlayer = true;
                        else isCorpse = true;
                        break;
                    }
                }
            }

            // 判定結果に基づく処理
            if (isAlivePlayer)
            {
                // 生きているプレイヤーなら、岩自身だけを破壊する
                Destroy(gameObject);
                return;
            }
            else if (isCorpse)
            {
                // 死体なら、死体を消して岩も消える
                Destroy(hitObject);
                Destroy(gameObject);
                return;
            }

            // プレイヤー以外の対象なら両方消す
            Destroy(hitObject);
            Destroy(gameObject);
        }
    }
}